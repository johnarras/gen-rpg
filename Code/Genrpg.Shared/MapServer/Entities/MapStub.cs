using MessagePack;
using Genrpg.Shared.Interfaces;
using System;

namespace Genrpg.Shared.MapServer.Entities
{
    [MessagePackObject]
    public class MapStub : IStringId, IName
    { 
        [Key(0)] public string Id { get; set; }
        [Key(1)] public string Name { get; set; }
        [Key(2)] public string Desc { get; set; }
        [Key(3)] public string Icon { get; set; }
        [Key(4)] public string Art { get; set; }

        [Key(5)] public int MinLevel { get; set; }
        [Key(6)] public int MaxLevel { get; set; }

        [Key(7)] public int BlockCount { get; set; }
        [Key(8)] public float ZoneSize { get; set; }

        public MapStub()
        {
            MinLevel = 1;
            MaxLevel = 100;
            BlockCount = 5;
            ZoneSize = 1;
        }

        public void CopyFrom(IMapRoot map)
        {
            Id = map.Id;
            Name = map.Name;
            Desc = map.Desc;
            Icon = map.Icon;
            Art = map.Art;
            MinLevel = map.MinLevel;
            MaxLevel = map.MaxLevel;
            BlockCount = map.BlockCount;
            ZoneSize = map.ZoneSize;
        }
    }
}
