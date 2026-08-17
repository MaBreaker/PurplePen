using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member because it is UNDER CONSTRUCTION.

namespace PdfiumViewer
{
    public class PdfMatches
    {
        public int StartPage { get; private set; }

        public int EndPage { get; private set; }

        public IList<PdfMatch> Items { get; private set; }

        internal PdfMatches(int startPage, int endPage, IList<PdfMatch> matches)
        {
            if (matches == null)
                throw new ArgumentNullException("matches");

            StartPage = startPage;
            EndPage = endPage;
            Items = new ReadOnlyCollection<PdfMatch>(matches);
        }
    }
}
