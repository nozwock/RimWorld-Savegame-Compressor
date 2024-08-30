using System;
using System.Reflection.Emit;
using System.Security.AccessControl;
using UnityEngine;
using Verse;

namespace Revolus.Compressor {
    public class Dialog_Progress : Window {
        public string title;

        private readonly Action<Dialog_Progress> afterDraw;

        public override Vector2 InitialSize {
            get {
                return new Vector2(280f, 100f);
            }
        }

        public Dialog_Progress(string title, Action<Dialog_Progress> afterDraw) {
            this.forcePause = true;
            this.doCloseX = false;
            this.absorbInputAroundWindow = true;
            this.closeOnAccept = false;
            this.closeOnClickedOutside = false;
            this.closeOnCancel = false;

            this.title = title;
            this.afterDraw = afterDraw;
        }

        public override void DoWindowContents(Rect inRect) {
            var listing = new Listing_Standard();
            listing.Begin(inRect);
            try {
                Text.Anchor = TextAnchor.MiddleCenter;
                var rect = listing.GetRect(Text.LineHeight);
                rect.y += Text.LineHeight;
                Widgets.Label(rect, $"{this.title}");
            } finally {
                listing.End();
            }

            this.afterDraw(this);
        }
    }
}