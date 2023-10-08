using MessagePack;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.DataStores.GameSettings;

namespace Genrpg.Shared.Inventory.Entities
{
    [MessagePackObject]
    public class QualityType : ChildSettings, IIndexedGameItem
    {

        public const int Common = 1;
        public const int Uncommon = 2;
        public const int Rare = 3;
        public const int Epic = 4;
        public const int Legendary = 5;
        public const int Set = 6;
        public const int Max = 7;



        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string ParentId { get; set; }
        [Key(2)] public long IdKey { get; set; }
        [Key(3)] public override string Name { get; set; }
        [Key(4)] public string Desc { get; set; }
        [Key(5)] public string Icon { get; set; }
        [Key(6)] public string Art { get; set; }

        // Scaling for generating items.
        [Key(7)] public int ItemSpawnWeight { get; set; }
        [Key(8)] public int ItemMinLevel { get; set; }
        [Key(9)] public int ItemStatPct { get; set; }
        [Key(10)] public int ItemCostPct { get; set; }

        // Scaling for generating monsters.
        [Key(11)] public string UnitName { get; set; }
        [Key(12)] public int UnitSpawnWeight { get; set; }
        [Key(13)] public int UnitMinLevel { get; set; }
        [Key(14)] public int UnitHealthPct { get; set; }
        [Key(15)] public int UnitDamPct { get; set; }
    }
}
