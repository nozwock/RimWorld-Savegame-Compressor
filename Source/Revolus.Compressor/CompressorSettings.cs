﻿using System;
using System.Collections.Concurrent;
using System.IO;
using System.IO.Compression;
using System.Threading;
using UnityEngine;
using Verse;

using CompressionLevel = System.IO.Compression.CompressionLevel;

namespace Revolus.Compressor {
    internal class CompressorSettings : ModSettings {
        internal static readonly bool prettyDefault = false;
        internal bool pretty = prettyDefault;

        internal static readonly int levelDefault = 0;
        internal int level = levelDefault;

        public override void ExposeData() {
            Scribe_Values.Look(ref this.pretty, "pretty", prettyDefault);
            Scribe_Values.Look(ref this.level, "level", levelDefault);
            base.ExposeData();
        }

        internal bool ShowAndChangeSettings(Listing_Standard listing) {
            int levelNew, levelOld = Math.Max(-1, Math.Min(this.level, +1));
            bool prettyNew, prettyOld = this.pretty;

            {
                var _wholeRect = listing.GetRect(Text.LineHeight);
                var labelRect = _wholeRect.LeftHalf().Rounded();
                var _dataRect = _wholeRect.RightHalf().Rounded();
                var dataSelectRect = _dataRect.LeftHalf().Rounded();
                var dataDescRect = _dataRect.RightHalf().Rounded();

                Text.Anchor = TextAnchor.MiddleRight;
                Widgets.Label(labelRect, "Compression level: ");

                levelNew = Mathf.RoundToInt(Widgets.HorizontalSlider(dataSelectRect, levelOld, -1, +1, roundTo: 1));

                Text.Anchor = TextAnchor.MiddleLeft;
                Widgets.Label(
                    dataDescRect,
                    levelNew < 0 ? " (uncompressed)" :
                    levelNew > 0 ? " (best compression)" : " (fastest compression)"
                );

                listing.Gap(listing.verticalSpacing + Text.LineHeight);
            }

            {
                var _wholeRect = listing.GetRect(Text.LineHeight);
                var labelRect = _wholeRect.LeftHalf().Rounded();
                var valueRect = _wholeRect.RightHalf().Rounded();

                Text.Anchor = TextAnchor.MiddleRight;
                Widgets.Label(labelRect, "Pretty print XML (use indentation): ");

                prettyNew = prettyOld;
                Widgets.CheckboxLabeled(valueRect, "", ref prettyNew, placeCheckboxNearText: true);

                listing.Gap((listing.verticalSpacing + Text.LineHeight) * 2f);
            }

            {
                var wholeRect = Utils.ClampHorizontally(listing.GetRect(Text.LineHeight * 2), 0.3f);
                Text.Anchor = TextAnchor.MiddleCenter;
                if (Widgets.ButtonText(wholeRect, "Compress All Saves")) {
                    Find.WindowStack.Add(new Dialog_Confirm("Are you sure? This may take a while.", () => {
                        var queue = new ConcurrentQueue<(int, int)>();
                        var thread = new Thread(new ThreadStart(() => {
                            var level = CompressorMod.Settings.level > 0 ? CompressionLevel.Optimal : CompressionLevel.Fastest;
                            var count = 0;
                            foreach (FileInfo file in GenFilePaths.AllSavedGameFiles) {
                                if (!Utils.IsGzipped(file.FullName) && CompressorMod.Settings.level >= 0) {
                                    Log.Message($"Compressing {file.Name}");
                                    var lastWriteTime = file.LastWriteTime;
                                    var data = File.ReadAllBytes(file.FullName);
                                    using (var fileStream = File.Create(file.FullName))
                                    using (var gzipStream = new GZipStream(fileStream, level, leaveOpen: false)) {
                                        gzipStream.Write(data, 0, data.Length);
                                    }

                                    File.SetLastWriteTime(file.FullName, lastWriteTime);

                                    count++;
                                    queue.Enqueue((0, count));
                                }
                            };

                            queue.Enqueue((1, count));
                        }));
                        thread.Start();

                        Find.WindowStack.Add(new Dialog_Progress("Saves processed: 0", (dialog) => {
                            if (queue.TryDequeue(out var item)) {
                                var (type, value) = item;
                                switch (type) {
                                    case 0:
                                        dialog.title = $"Saves processed: {value}";
                                        break;
                                    case 1:
                                        Find.WindowStack.TryRemove(dialog, true);
                                        Find.WindowStack.Add(new Dialog_MessageBox($"Compressed {value} saves."));
                                        break;
                                }
                            }
                        }));
                    }));
                };

                listing.Gap(Text.LineHeight / 3f);
            }

            {
                var wholeRect = Utils.ClampHorizontally(listing.GetRect(Text.LineHeight * 2), 0.3f);
                Text.Anchor = TextAnchor.MiddleCenter;
                if (Widgets.ButtonText(wholeRect, "Decompress All Saves")) {
                    Find.WindowStack.Add(new Dialog_Confirm("Are you sure? This may take a while.", () => {
                        var queue = new ConcurrentQueue<(int, int)>();
                        var thread = new Thread(new ThreadStart(() => {
                            var count = 0;
                            foreach (FileInfo file in GenFilePaths.AllSavedGameFiles) {
                                if (Utils.IsGzipped(file.FullName)) {
                                    Log.Message($"Decompressing {file.Name}");
                                    var lastWriteTime = file.LastWriteTime;
                                    var data = File.ReadAllBytes(file.FullName);
                                    using (var gzipStream = new GZipStream(new MemoryStream(data), CompressionMode.Decompress))
                                    using (var fileStream = File.Create(file.FullName)) {
                                        gzipStream.CopyTo(fileStream);
                                    };

                                    File.SetLastWriteTime(file.FullName, lastWriteTime);

                                    count++;
                                    queue.Enqueue((0, count));
                                }
                            };

                            queue.Enqueue((1, count));
                        }));
                        thread.Start();

                        Find.WindowStack.Add(new Dialog_Progress("Saves processed: 0", (dialog) => {
                            if (queue.TryDequeue(out var item)) {
                                var (type, value) = item;
                                switch (type) {
                                    case 0:
                                        dialog.title = $"Saves processed: {value}";
                                        break;
                                    case 1:
                                        Find.WindowStack.TryRemove(dialog, true);
                                        Find.WindowStack.Add(new Dialog_MessageBox($"Decompressed {value} saves."));
                                        break;
                                }
                            }
                        }));
                    }));
                }
            }

            var changed = false;
            if (prettyNew != prettyOld) {
                this.pretty = prettyNew;
                changed = true;
            }
            if (levelNew != levelOld) {
                this.level = levelNew;
                changed = true;
            }

            return changed;
        }
    }
}
