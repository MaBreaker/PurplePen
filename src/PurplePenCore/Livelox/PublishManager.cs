/* Copyright (c) 2006-2008, Peter Golde
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

using PurplePen.Graphics2D;
using PurplePen.Livelox.ApiContracts;
using System;
using System.ComponentModel.Design;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.Intrinsics.Arm;
using System.Threading.Tasks;

namespace PurplePen.Livelox
{
    public class PublishManager
    {
        private const string mapFileName = "map.png";
        private const string courseDataFileName = "coursedata.xml";

        public ImportableEvent CreateImportableEvent(Controller controller, SymbolDB symbolDB, double resolution, string temporaryDirectory)
        {
            var eventDB = controller.GetEventDB();

            MapDisplay clonedMapDisplay = controller.MapDisplay.CloneToFullIntensity();

            //clonedMapDisplay.AntiAlias = false; // mapExporter.CreateBitmap override this value anyways
            clonedMapDisplay.ColorModel = ColorModel.CMYK;
            clonedMapDisplay.SetCourse(null);
            clonedMapDisplay.SetPrintArea(null);

            var dpi = (float)(resolution /* in pixels per real-world meter */ * controller.MapScale * 0.0254);
            if (clonedMapDisplay.MapType == MapType.Bitmap)
            {
                // no need to export in higher resolution than the bitmap's resolution
                dpi = Math.Min(dpi, clonedMapDisplay.Dpi);
            }
            //JU: Set static DPI value for PDF map
            else if (clonedMapDisplay.MapType == MapType.PDF)
            {
                //dpi = 300;

                // TODO: PDF clonedMapDisplay is not always ready at this point !?! Redrawing ongoing maybe ?
                // "Parameter exception thrown" in CreateMapImage process and and PNG is blank
                // OCAD and JPG works fine every time !!! PDF mostly fails

                // TODO: How to wait here (withou re-reading source map file) and ensure that the cloned bitmap and exported PNG is NOT blank ?

                //JU: Dirty hack to get the PDF background map visible, otherwise CreateMapImage generates blank PNG
                clonedMapDisplay.SetMapFile(MapType.PDF, clonedMapDisplay.FileName);
            }

            // Size calculation used for georeferencing only
            // This must match to ExportBitmap.CreateBitmap() function, which use the same mapDisplay.Bounds and DPI values
            Size mapSize = new Size((int)Math.Ceiling((clonedMapDisplay.Bounds.Width / 25.4F) * dpi), (int)Math.Ceiling((clonedMapDisplay.Bounds.Height / 25.4F) * dpi));

            // Notice: mapExporter.CreateBitmap override Alias and Printing and MapIntensity values
            CreateMapImage(clonedMapDisplay, dpi, temporaryDirectory);
            
            CreateCourseDataXml(eventDB, clonedMapDisplay, temporaryDirectory);

            // Notice: Livelox service has problems with non georeferenced maps and course overlay images (Error: image must have positive width and height)
            if (clonedMapDisplay.CoordinateMapper != null)
            {
                // Do not render the actual map, just the courses.
                clonedMapDisplay.SetMapFile(MapType.None, null);
                CreateCourseImages(controller, eventDB, symbolDB, clonedMapDisplay, temporaryDirectory);
            }

            var importableEvent = CreateImportableEventObject(eventDB, clonedMapDisplay, mapSize, temporaryDirectory);

            clonedMapDisplay.Dispose();

            return importableEvent;
        }

        public string CreateTemporaryDirectory()
        {
            var tempDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(tempDirectory);
            return tempDirectory;
        }

        public void DeleteTemporatyDirectory(string temporaryDirectory)
        {
            try
            {
                Directory.Delete(temporaryDirectory);
            }
            catch
            {
                // just ignore, after all it is temporary
            }
        }

        private static void CreateMapImage(MapDisplay mapDisplay, float exportDpi, /* double resolution, */ string temporaryDirectory)
        {
            var mapExporter = new ExportBitmap(mapDisplay);
            
            // TODO: PDF mapDisplay is not always ready at this point !?! Redraw still ongoing ?

            mapExporter.CreateBitmap(
                Path.Combine(temporaryDirectory, mapFileName),
                mapDisplay.MapBounds,
                GraphicsBitmapFormat.PNG,
                exportDpi,
                mapDisplay.CoordinateMapper
            );
        }

        private static void CreateCourseDataXml(EventDB eventDB, MapDisplay mapDisplay, string temporaryDirectory)
        {
            var xmlExporter = new ExportXmlVersion3();
            xmlExporter.WriteXml(
                Path.Combine(temporaryDirectory, courseDataFileName),
                eventDB,
                mapDisplay.MapBounds,
                mapDisplay.CoordinateMapper
            );
        }

        private static void CreateCourseImages(Controller controller, EventDB eventDB, SymbolDB symbolDB, MapDisplay mapDisplay, string temporaryDirectory)
        {
            var coursePdfSettings = new CoursePdfSettings
            {
                mapDirectory = false,
                fileDirectory = false,
                outputDirectory = temporaryDirectory,
                ColorModel = ColorModel.CMYK,
                CourseIds = eventDB.AllCourseIds.ToArray(),
                CropLargePrintArea = true,
                FileCreation = CoursePdfSettings.PdfFileCreation.FilePerCourse,
                PrintMapExchangesOnOneMap = true,
                RenderControlDescriptions = false,  // Don't render control descriptions.
                ShowProgressDialog = false
            };

            var ev = eventDB.GetEvent();
            var courseAppearance = (CourseAppearance)ev.courseAppearance.Clone();

            var coursePdf = new CoursePdf(eventDB, symbolDB, controller, mapDisplay, coursePdfSettings, courseAppearance);
            coursePdf.CreatePdfs();
        }

        private static ImportableEvent CreateImportableEventObject(EventDB eventDB, MapDisplay mapDisplay, Size mapImageRectangle, string temporaryDirectory)
        {
            var ev = eventDB.GetEvent();

            //JU: Disable this Image.FromFile block as System.Drawing.Common is not compatible with Linux and MacOS
            //    use mapImageRectangle from CreateMapImage() instead
            //Rectangle mapImageRectangle;
            //using (var mapImage = Image.FromFile(Path.Combine(temporaryDirectory, mapFileName)))
            //{ mapImageRectangle = new Rectangle(0, 0, mapImage.Width, mapImage.Height); }

            Georeference mapGeoreference = null;
            if (mapDisplay.CoordinateMapper != null)
            {
                mapGeoreference = new Georeference()
                {
                    CoordinateMapping = new CoordinateMapping()
                    {
                        Positions = new[]
                        {
                            // note the order of Top and Bottom; Top has a lower value than Bottom
                            GetGeoPosition(mapDisplay.Bounds.Left, mapDisplay.Bounds.Top, mapDisplay.CoordinateMapper),      // bottom left
                            GetGeoPosition(mapDisplay.Bounds.Right, mapDisplay.Bounds.Top, mapDisplay.CoordinateMapper),     // bottom right
                            GetGeoPosition(mapDisplay.Bounds.Right, mapDisplay.Bounds.Bottom, mapDisplay.CoordinateMapper),  // top right
                            GetGeoPosition(mapDisplay.Bounds.Left, mapDisplay.Bounds.Bottom, mapDisplay.CoordinateMapper)    // top left
                        },
                        ImagePositions = new[]
                        {
                            new ImageCoordinate() {X = 0, Y = mapImageRectangle.Height},                        // bottom left
                            new ImageCoordinate() {X = mapImageRectangle.Width, Y = mapImageRectangle.Height},  // bottom right
                            new ImageCoordinate() {X = mapImageRectangle.Width, Y = 0},                         // top right
                            new ImageCoordinate() {X = 0, Y = 0}                                                // top left
                        }
                    }
                };

                if (mapGeoreference.CoordinateMapping.Positions.Any(o => o == null))
                {
                    // there is no georeference present
                    mapGeoreference = null;
                }
            }



            var map = new Map()
            {
                Name = Path.GetFileNameWithoutExtension(ev.mapFileName),
                FileName = "map.png",
                MapScale = ev.mapScale,
                // note the order of Top and Bottom; Top has a lower value than Bottom
                BottomLeftCornerPosition = new MapCoordinate() { X = mapDisplay.Bounds.Left / 1000, Y = mapDisplay.Bounds.Top / 1000 }, 
                TopRightCornerPosition = new MapCoordinate() { X = mapDisplay.Bounds.Right / 1000, Y = mapDisplay.Bounds.Bottom / 1000 },
                Georeference = mapGeoreference
            };

            //JU: CourseDataFileNames + CourseImageFileNames definitions in API will use the scale of the first map for everything
            /*
            If one would like to use different scales for individual courses other than original map has, 
            then instead of this simple list of IOF.xml and course.pdf files, one must build full IOF XML structure for constrols, courses etc. like 

              "controls": [{ "code": "S","type": "Start","position": {"latitude": 59.758863,"longitude": 17.708028},"mapPosition": {"x": 0.4411,"y": 0.5456},], ...
              "courses": ["name": "Course 1","controls":[] ... [{"courseImages": [{"fileName": "course-1.svg","mapScale": 7500}]},], ...
            */

            var importableEvent = new ImportableEvent()
            {
                Name = ev.title,
                Maps = new[] { map },
                CourseDataFileNames = new[] { "coursedata.xml" },
                //JU: Notice! Livelox service has problems with non georeferenced maps and course overlay images, hence no PDF is generated in such cases
                CourseImageFileNames = new DirectoryInfo(temporaryDirectory)
                    .GetFiles("*.pdf")
                    .Select(file => file.Name)
                    .ToArray()
            };

            return importableEvent;
        }

        private static GeoCoordinate GetGeoPosition(float x, float y, CoordinateMapper coordinateMapper)
        {
            //JU: Export non georeferenced maps
            if (coordinateMapper != null && coordinateMapper.GetLatLong(new PointF(x, y), out var latitude, out var longitude))
            {
                return new GeoCoordinate()
                {
                    Latitude = latitude,
                    Longitude = longitude
                };
            }
            return null;
        }
    }
}