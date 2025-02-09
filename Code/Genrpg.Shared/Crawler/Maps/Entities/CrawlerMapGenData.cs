using MessagePack;

namespace Genrpg.Shared.Crawler.Maps.Entities
{
    [MessagePackObject]
    public class CrawlerMapGenData
    {
        public CrawlerWorld World;
        public long MapType;
        [Key(0)] public int Level { get; set; } = 0;
        [Key(1)] public long ZoneTypeId { get; set; }
        [Key(2)] public bool Looping { get; set; }
        [Key(3)] public long FromMapId { get; set; }
        [Key(4)] public int FromMapX { get; set; }
        [Key(5)] public int FromMapZ { get; set; }
        [Key(6)] public int CurrFloor { get; set; } = 1;
        [Key(7)] public int MaxFloor { get; set; } = 1;
        [Key(8)] public string Name { get; set; }
        [Key(9)] public bool RandomWallsDungeon { get; set; }
        [Key(10)] public CrawlerMap PrevMap { get; set; }
        [Key(11)] public long ArtSeed { get; set; }
    }

}
