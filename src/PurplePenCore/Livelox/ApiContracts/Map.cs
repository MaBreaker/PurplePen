namespace PurplePen.Livelox.ApiContracts
{
    public class Map
    {
        public string FileName { get; set; }

        public string Name { get; set; }

        public double MapScale { get; set; }

        public MapCoordinate BottomLeftCornerPosition { get; set; }

        public MapCoordinate TopRightCornerPosition { get; set; }

        public Georeference Georeference { get; set; }
    }
}