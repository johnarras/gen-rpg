using MessagePack;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Names.Entities;
using Genrpg.Shared.ProcGen.Entities;
using Genrpg.Shared.Spawns.Entities;
using Genrpg.Shared.Spells.Entities;
using System.Collections.Generic;

namespace Genrpg.Shared.Units.Entities
{
    [MessagePackObject]
    public class UnitType : IIndexedGameItem
    {

        
        [Key(0)] public long IdKey { get; set; }
        [Key(1)] public string Name { get; set; }
        [Key(2)] public string Desc { get; set; }
        [Key(3)] public string Icon { get; set; }
        [Key(4)] public string Art { get; set; }

        [Key(5)] public float Height { get; set; }

        [Key(6)] public long TribeTypeId { get; set; }

        [Key(7)] public List<BaseEffect> Effects { get; set; }

        [Key(8)] public List<WeightedName> PrefixNames { get; set; }

        [Key(9)] public List<WeightedName> DoubleNameSuffixes { get; set; }

        [Key(10)] public List<WeightedName> SuffixNames { get; set; }


        [Key(11)] public List<WeightedName> AlternateNames { get; set; }

        [Key(12)] public List<MonsterFood> FoodSources { get; set; }

        [Key(13)] public List<SpawnItem> LootItems { get; set; }
        [Key(14)] public List<SpawnItem> InteractLootItems { get; set; }

        public UnitType()
        {
            PrefixNames = new List<WeightedName>();
            DoubleNameSuffixes = new List<WeightedName>();
            SuffixNames = new List<WeightedName>();
            AlternateNames = new List<WeightedName>();
            FoodSources = new List<MonsterFood>();
            LootItems = new List<SpawnItem>();
            InteractLootItems = new List<SpawnItem>();

        }


    }
}
