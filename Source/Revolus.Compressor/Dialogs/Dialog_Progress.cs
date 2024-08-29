using System;
using System.Reflection.Emit;
using System.Security.AccessControl;
using UnityEngine;
using Verse;

namespace Revolus.Compressor {
    public class Dialog_Progress : Window {
        public string title;

        private Action beforeClose;

        public override Vector2 InitialSize {
            get {
                return new Vector2(280f, 100f);
            }
        }

        public Dialog_Progress(string title, Action beforeClose) {
            this.forcePause = true;
            this.doCloseX = false;
            this.absorbInputAroundWindow = true;
            this.closeOnAccept = false;
            this.closeOnClickedOutside = false;

            this.title = title;
            this.beforeClose = beforeClose;
        }

        public override void DoWindowContents(Rect inRect) {
            var listing = new Listing_Standard();
            listing.Begin(inRect);
            try {
                listing.Label($"{this.title}");
                this.beforeClose();
            } finally {
                listing.End();
            }

            Find.WindowStack.TryRemove(this, true);
        }
    }
}