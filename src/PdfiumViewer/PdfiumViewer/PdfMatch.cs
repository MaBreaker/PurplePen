using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member because it is UNDER CONSTRUCTION.

namespace PdfiumViewer
{
    public class PdfMatch
    {
        public PointF Location { get; private set; }

        public string Text { get; private set; }

        public int Page { get; private set; }

        internal PdfMatch(PointF location, string text, int page)
        {
            Location = location;
            Text = text;
            Page = page;
        }
    }
}
