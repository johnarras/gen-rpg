using MessagePack;
namespace Genrpg.Shared.Crawler.Maps.Entities
{
    [MessagePackObject]
    public class MapCellDetail
    {
        [Key(0)] public int X { get; set; }
        [Key(1)] public int Z { get; set; }
        [Key(2)] public long EntityTypeId { get; set; }
        [Key(3)] public long EntityId { get; set; }
        [Key(4)] public int ToX { get; set; }
        [Key(5)] public int ToZ { get; set; }
        [Key(6)] public int Index { get; set; }
    }

    [MessagePackObject]
    public class ErrorMapCellDetail
    {
        [Key(0)] public MapCellDetail Detail { get; set; }
        [Key(1)] public string ErrorText { get; set; }
    }
}
