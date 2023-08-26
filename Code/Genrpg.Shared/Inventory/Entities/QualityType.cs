using MessagePack;
using Genrpg.Shared.Interfaces;

namespace Genrpg.Shared.Inventory.Entities
{
    [MessagePackObject]
    public class QualityType : IIndexedGameItem
    {

        public const int Common = 1;
        public const int Uncommon = 2;
        public const int Rare = 3;
        public const int Epic = 4;
        public const int Legendary = 5;
        public const int Set = 6;
        public const int Max = 7;


        
        [Key(0)] public long IdKey { get; set; }
        [Key(1)] public string Name { get; set; }
        [Key(2)] public string Desc { get; set; }
        [Key(3)] public string Icon { get; set; }
        [Key(4)] public string Art { get; set; }

        // Scaling for generating items.
        [Key(5)] public int ItemSpawnWeight { get; set; }
        [Key(6)] public int ItemMinLevel { get; set; }
        [Key(7)] public int ItemStatPct { get; set; }
        [Key(8)] public int ItemCostPct { get; set; }

        // Scaling for generating monsters.
        [Key(9)] public string UnitName { get; set; }
        [Key(10)] public int UnitSpawnWeight { get; set; }
        [Key(11)] public int UnitMinLevel { get; set; }
        [Key(12)] public int UnitHealthPct { get; set; }
        [Key(13)] public int UnitDamPct { get; set; }
    }
}
