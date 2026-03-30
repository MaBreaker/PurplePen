/* Copyright (c) 2011, Peter Golde
 * All rights reserved.
 * 
 * Redistribution and use in source and binary forms, with or without 
 * modification, are permitted provided that the following conditions are 
 * met:
 * 
 * 1. Redistributions of source code must retain the above copyright
 * notice, this list of conditions and the following disclaimer.
 * 
 * 2. Redistributions in binary form must reproduce the above copyright
 * notice, this list of conditions and the following disclaimer in the
 * documentation and/or other materials provided with the distribution.
 * 
 * 3. Neither the name of Peter Golde, nor "Purple Pen", nor the names
 * of its contributors may be used to endorse or promote products
 * derived from this software without specific prior written permission.
 * 
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND
 * CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES,
 * INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR
 * CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING,
 * BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
 * SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
 * INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
 * WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
 * NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE
 * USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY
 * OF SUCH DAMAGE.
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;

using SysDraw = System.Drawing;
using PointF = System.Drawing.PointF;
using RectangleF = System.Drawing.RectangleF;
using SizeF = System.Drawing.SizeF;

using PurplePen.MapModel;
using PurplePen.Graphics2D;

using PdfSharp.Pdf;
using PdfSharp.Drawing;
using PdfSharp.Fonts;

namespace PurplePen.MapModel
{
    // These should only be created by PdfWriter.
    internal class PdfDocumentWriter: IPdfDocumentWriter
    {
        private string fileName;
        private PdfDocument document;

        static PdfDocumentWriter()
        {
            // Set our font resolver so we can use our fonts in PDF output. This is tightly
            // coupled with PdfGraphicsTarget because it relies on the same font resolver to get the font data for measuring text.
           GlobalFontSettings.FontResolver = new PdfFontResolver();
        }

        // Create a PdfDocumentWriter with the given title.
        public PdfDocumentWriter(string fileName, string title, bool cmykMode)
        {
            this.fileName = fileName;
            document = new PdfDocument();
            document.Info.Title = title;
            document.Options.NoCompression = false;
            document.Options.CompressContentStreams = true;
            document.Options.ColorMode = cmykMode ? PdfColorMode.Cmyk : PdfColorMode.Rgb;
        }

        // Get a page.
        public IGraphicsTarget BeginPage(SizeF sizeInInches /* JU: Margin */, int margin)
        {
            // Create an empty page
            PdfPage page = document.AddPage();

            // Set the sizes
            //JU: Crop and Bleed
            //var pageSize = new PdfRectangle(new XRect(0, 0, sizeInInches.Width * 72.0F, sizeInInches.Height * 72.0F));
            //float marginInInches = (margin > 0 ? margin / 100F : 0F);
            float bleedInInches = (margin < 0 ? Math.Abs(margin) / 100F : 0F);
            var fullSize = new PdfRectangle(new XRect(0, 0, sizeInInches.Width * 72F + 2 * bleedInInches * 72F, sizeInInches.Height * 72F + 2 * bleedInInches * 72F));
            //var cropSize = new PdfRectangle(new XPoint(marginInInches * 72F, marginInInches * 72F), new XPoint(fullSize.X2 - marginInInches * 72F, fullSize.Y2 - marginInInches * 72F));
            var cropSize = fullSize;
            var viewSize = new PdfRectangle(new XPoint(cropSize.X1 + bleedInInches * 72F, cropSize.Y1 + bleedInInches * 72F), new XPoint(cropSize.X2 - bleedInInches * 72F, cropSize.Y2 - bleedInInches * 72F));
            page.MediaBox = fullSize;
            page.CropBox = page.BleedBox = cropSize;
            page.TrimBox = page.ArtBox = viewSize;
            /*
            if (margin > 0)
            {
                float marginInInches = margin / 100F;
                var fullSize = new PdfRectangle(new XRect(0, 0, sizeInInches.Width * 72.0F, sizeInInches.Height * 72.0F));
                var cropSize = new PdfRectangle(new XPoint(fullSize.X1 + marginInInches * 72.0F, fullSize.Y1 + marginInInches * 72.0F), new XPoint(fullSize.X2 - marginInInches * 72.0F, fullSize.Y2 - marginInInches * 72.0F));
                page.MediaBox = fullSize;
                page.CropBox = page.BleedBox = page.TrimBox = page.ArtBox = cropSize;
            }
            else if (margin < 0)
            {
                float bleedInInches = Math.Abs(margin) / 100F;
                var fullSize = new PdfRectangle(new XRect(0, 0, sizeInInches.Width * 72.0F + 2 * bleedInInches * 72.0F, sizeInInches.Height * 72.0F + 2 * bleedInInches * 72.0F));
                var viewSize = new PdfRectangle(new XPoint(fullSize.X1 + bleedInInches * 72.0F, fullSize.Y1 + bleedInInches * 72.0F), new XSize(sizeInInches.Width * 72.0F, sizeInInches.Height * 72.0F));
                page.MediaBox = page.CropBox = page.BleedBox = fullSize;
                page.TrimBox = page.ArtBox = viewSize;
            }
            else
            {
                var fullSize = new PdfRectangle(new XRect(0, 0, sizeInInches.Width * 72.0F, sizeInInches.Height * 72.0F));
                page.MediaBox = page.BleedBox = page.CropBox = page.TrimBox = page.ArtBox = fullSize;
            }
            */
            //page.TrimMargins, shrink page box / size to remove white edges etc. ?

            // Get an XGraphics object for drawing
            XGraphics gfx = XGraphics.FromPdfPage(page);

            // Get a graphics target
            IGraphicsTarget target = new Pdf_GraphicsTarget(gfx, document.Options.ColorMode == PdfColorMode.Cmyk);

            // Change units to hundreths of inch from points.
            Matrix matrix = new Matrix();
            //JU: Crop and Bleed
            matrix.Translate((float)page.TrimBox.X1, (float)page.TrimBox.Y1);
            matrix.Scale(72F / 100F, 72F / 100F);
            target.PushTransform(matrix);

            return target;
        }

        // Test whether a page is readable. Throws an exception if not.
        public static void TestReadingPage(string pathName, int pageNumber)
        {
            using (PdfImporter importer = new PdfImporter(pathName)) {
                PdfPage page = importer.GetPage(pageNumber);
                XForm xform = importer.GetXForm(pageNumber);
                xform.Dispose();
            }
        }

        // Get a page that is a copy of a PDF page.
        public IGraphicsTarget BeginCopiedPage(string importedPdfPath, int pageNumber)
        {
            using (PdfImporter pdfImporter = new PdfImporter(importedPdfPath)) {
                PdfPage pageToCopy = pdfImporter.GetPage(pageNumber);

                // Create an copy of an existing page
                PdfPage page = document.AddPage(pageToCopy);

                // Get an XGraphics object for drawing
                XGraphics gfx = XGraphics.FromPdfPage(page);

                // Get a graphics target
                IGraphicsTarget target = new Pdf_GraphicsTarget(gfx, document.Options.ColorMode == PdfColorMode.Cmyk);

//JU: TODO Crop and Bleed

            PointF cropBoxOriginInPoints = CropboxOriginInPoints(pageToCopy);

                // Change units to hundreths of inch from points.
                Matrix matrix = new Matrix();

                matrix.Translate(cropBoxOriginInPoints.X, cropBoxOriginInPoints.Y);
                matrix.Scale(72F / 100F, 72F / 100F);
                target.PushTransform(matrix);

                //pageToCopy.Close();

                return target;
            }
        }

        // Get a page that is a copy of a PDF page.
        // sizeInInches is the size of the new page, in inches.
        // partialSourcePageInInches is the rectangle on the source page to copy, in inches. This maps to destinationCropInInches.
        // destinationCropInInches in the rectangle on the destination page to draw into, in inches. If you want the whole page, use new RectangleF(0,0,sizeInInches.Width,sizeInInches.Height)
        public IGraphicsTarget BeginCopiedPartialPage(string importedPdfPath, int pageNumber, SizeF sizeInInches, RectangleF partialSourcePageInInches, RectangleF destinationCropInInches /* JU: Margin */ , int margins)
        {
            using (PdfImporter pdfImporter = new PdfImporter(importedPdfPath))
            using (XForm xformToCopy = pdfImporter.GetXForm(pageNumber)) {
                PdfPage pageToCopy = pdfImporter.GetPage(pageNumber);

                PointF cropBoxOriginInPoints = CropboxOriginInPoints(pageToCopy);

            //JU: PDF white margins
            float marginInInches = (margins > 0 ? margins / 100F : 0.0F);

//JU: TODO new PDF page with smaller size (remove margins) then draw it
//    and finally extend page size and top left by margins to get blank margins

            // Get a graphics target
            IGraphicsTarget target = BeginPage(sizeInInches /* JU: Margins */ , margins);

                // Initial target is entire page. Push a clip to restrict to destinationCropInInches.
                RectangleF destRect = new RectangleF(destinationCropInInches.Left * 100, destinationCropInInches.Top * 100, destinationCropInInches.Width * 100, destinationCropInInches.Height * 100);
                target.PushClip(destRect);

                // Create transform that maps the source page to the destination crop rect. Destination is in hundreths of inches so must match that.
                RectangleF srcRect = new RectangleF(partialSourcePageInInches.Left * 100, partialSourcePageInInches.Top * 100, partialSourcePageInInches.Width * 100, partialSourcePageInInches.Height * 100);
                srcRect.Offset(cropBoxOriginInPoints.X / 72 * 100, cropBoxOriginInPoints.Y / 72 * 100);
                Matrix transform = Geometry.CreateRectangleTransform(srcRect, destRect);

            target.PushTransform(transform);

            // Get an XGraphics object for drawing
            XGraphics xGraphics = ((Pdf_GraphicsTarget)target).XGraphics;
            //JU: Set crop box for white margins
            //       xGraphics.PdfPage.CropBox = new PdfRectangle(new XPoint(marginInInches * 72F, marginInInches * 72F), new XPoint(sizeInInches.Width * 72F - marginInInches * 72F, sizeInInches.Height * 72F - marginInInches * 72F)); ;
            //JU: PDF white margins, draws whole source PDF always, no matter what size rect bounds are set
            xGraphics.DrawImage(xformToCopy, new XRect(0, 0, xformToCopy.PointWidth / 72F * 100F, xformToCopy.PointHeight / 72F * 100F));
            //JU: Default crop box
            //       xGraphics.PdfPage.CropBox = new PdfRectangle(new XPoint(0, 0), new XPoint(sizeInInches.Width * 72F, sizeInInches.Height * 72F)); ;
            target.PopTransform();
            target.PopClip();

                return target;
            }
        }

        private PointF CropboxOriginInPoints(PdfPage pageToCopy)
        {
            PdfRectangle cropRect = pageToCopy.CropBox;
            if (!cropRect.IsZero) {
                double translateX = cropRect.Location.X;
                double translateY = pageToCopy.Height.Point - (cropRect.Location.Y + cropRect.Size.Height);
                return new PointF((float)translateX, (float)translateY);
            }
            else {
                return new PointF();
            }
        }

        public void EndPage(IGraphicsTarget target)
        {
            target.Dispose();
        }

        // Save the PDF 
        public void Save()
        {
            document.Save(fileName);
        }
    }

    // Class that can create PdfDocumentWriter instances.
    public class PdfWriter: IPdfWriter
    {
        public bool CanReadPdfPage(string pdfImport, int pageImport)
        {
            try {
                PdfDocumentWriter.TestReadingPage(pdfImport, pageImport);
            }
            catch (Exception) {
                return false;
            }

            return true;
        }

        public IPdfDocumentWriter CreateDocument(string fileName, string title, bool cmykMode)
        {
            return new PdfDocumentWriter(fileName, title, cmykMode);
        }
    }
}
