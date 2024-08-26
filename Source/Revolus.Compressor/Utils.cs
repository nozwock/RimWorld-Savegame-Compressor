using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Xml;

namespace Revolus.Compressor {
    internal class Utils {
        internal static T WithUncompressedXmlTextReader<T>(string filePath, Func<XmlTextReader, T> action) {
            T DoAction(Stream uncompressedBytesStream) {
                using (var textReader = new StreamReader(uncompressedBytesStream)) {
                    using (var xmlTextReader = new XmlTextReader(textReader)) {
                        return action(xmlTextReader);
                    }
                }
            }

            using (var maybeCompressedStream = new FileStream(filePath, FileMode.Open)) {
                var headerBuffer = new byte[3];
                if (maybeCompressedStream.Read(headerBuffer, 0, 3) < 3) {
                    throw new Exception("The input file is truncated or empty: " + filePath);
                }

                maybeCompressedStream.Seek(0, SeekOrigin.Begin);

                var isCompressed = (headerBuffer[0] == 0x1f && headerBuffer[1] == 0x8b && headerBuffer[2] == 0x08);
                if (!isCompressed) {
                    return DoAction(maybeCompressedStream);
                } else {
                    using (var uncompressedStream = new GZipStream(maybeCompressedStream, CompressionMode.Decompress)) {
                        return DoAction(uncompressedStream);
                    }
                }
            }
        }

        internal static bool GetType(Assembly assembly, string name, out Type type) {
            var val = assembly.GetType(name, throwOnError: false);
            type = val;
            return !(val is null);
        }

        internal static bool GetMethod(Type type, string name, bool staticOnly, out MethodInfo info) {
            var flags = BindingFlags.Public | BindingFlags.NonPublic | (staticOnly ? BindingFlags.Static : BindingFlags.Instance);
            var val = type.GetMethod(name, flags);
            info = val;
            return !(val is null);
        }

        internal static bool GetProp(Type type, string name, bool staticOnly, out PropertyInfo info) {
            var flags = BindingFlags.Public | BindingFlags.NonPublic | (staticOnly ? BindingFlags.Static : BindingFlags.Instance);
            var val = type.GetProperty(name, flags);
            info = val;
            return !(val is null);
        }

        public static bool IsGzipped(string filePath) {
            var gzipMagicNumber = new byte[] { 0x1F, 0x8B }; // for GZip version 4.3 and later

            using (FileStream fileStream = File.OpenRead(filePath)) {
                var fileHeader = new byte[2];
                fileStream.Read(fileHeader, 0, fileHeader.Length);

                if (fileHeader.SequenceEqual(gzipMagicNumber)) {
                    return true;
                } else {
                    return false;
                }
            }
        }
    }
}
