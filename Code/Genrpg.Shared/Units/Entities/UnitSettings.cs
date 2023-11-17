using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.DataStores.GameSettings;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Names.Entities;
using Genrpg.Shared.ProcGen.Entities;
using Genrpg.Shared.Spawns.Entities;
using Genrpg.Shared.Spells.Entities;
using Genrpg.Shared.GameSettings.Loaders;

namespace Genrpg.Shared.Units.Entities
{
    [MessagePackObject]
    public class UnitSettings : ParentSettings<UnitType>
    {
        [Key(0)] public override string Id { get; set; }


        public UnitType GetUnitType (long idkey)
        {
            return _lookup.Get<UnitType>(idkey);
        }

    }
    [MessagePackObject]
    public class UnitType : ChildSettings, IIndexedGameItem
    {

        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string ParentId { get; set; }
        [Key(2)] public long IdKey { get; set; }
        [Key(3)] public override string Name { get; set; }
        [Key(4)] public string Desc { get; set; }
        [Key(5)] public string Icon { get; set; }
        [Key(6)] public string Art { get; set; }

        [Key(7)] public float Height { get; set; }

        [Key(8)] public long TribeTypeId { get; set; }

        [Key(9)] public List<BaseEffect> Effects { get; set; }

        [Key(10)] public List<WeightedName> PrefixNames { get; set; }

        [Key(11)] public List<WeightedName> DoubleNameSuffixes { get; set; }

        [Key(12)] public List<WeightedName> SuffixNames { get; set; }


        [Key(13)] public List<WeightedName> AlternateNames { get; set; }

        [Key(14)] public List<MonsterFood> FoodSources { get; set; }

        [Key(15)] public List<SpawnItem> LootItems { get; set; }
        [Key(16)] public List<SpawnItem> InteractLootItems { get; set; }

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

    [MessagePackObject]
        public class UnitSettingsApi : ParentSettingsApi<UnitSettings, UnitType> { }

    [MessagePackObject]
        public class UnitSettingsLoasder : ParentSettingsLoader<UnitSettings, UnitType, UnitSettingsApi> { }
    }
}
