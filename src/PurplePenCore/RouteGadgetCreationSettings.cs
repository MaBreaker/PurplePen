namespace PurplePen
{
    // Settings for creating RouteGadget files.
    public class RouteGadgetCreationSettings
    {
        public bool mapDirectory, fileDirectory;   // directory to place output files in
        public string outputDirectory;             // the output directory if mapDirectory and fileDirectoy are false.
        public string fileBaseName;                // base name for file names which are .xml,.gif
        public int xmlVersion = 3;                 // version of IOF XML to use (2 or 3).

        public RouteGadgetCreationSettings Clone()
        {
            return (RouteGadgetCreationSettings)base.MemberwiseClone();
        }
    }
}