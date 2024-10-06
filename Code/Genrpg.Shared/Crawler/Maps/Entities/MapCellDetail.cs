namespace Genrpg.Shared.Crawler.Maps.Entities
{
    public class MapCellDetail
    {
        public int X { get; set; }
        public int Z { get; set; }
        public long EntityTypeId { get; set; }
        public long EntityId { get; set; }
        public int ToX { get; set; }
        public int ToZ { get; set; }
    }

    public class ErrorMapCellDetail
    {
        public MapCellDetail Detail { get; set; }
        public string ErrorText { get; set; }
    }
}
