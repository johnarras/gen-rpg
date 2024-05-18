using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.Names.Settings;
using Genrpg.Shared.ProcGen.Settings.Monsters;
using Genrpg.Shared.Spawns.Settings;
using Genrpg.Shared.Spells.Settings.Effects;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.Purchasing.Settings;

namespace Genrpg.Shared.Units.Entities
{
    [MessagePackObject]
    public class UnitSettings : ParentSettings<UnitType>
    {
        [Key(0)] public override string Id { get; set; }

    }
    [MessagePackObject]
    public class UnitType : ChildSettings, IIndexedGameItem
    {

        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string ParentId { get; set; }
        [Key(2)] public long IdKey { get; set; }
        [Key(3)] public override string Name { get; set; }
        [Key(4)] public string PluralName { get; set; }
        [Key(5)] public string Desc { get; set; }
        [Key(6)] public string Icon { get; set; }
        [Key(7)] public string Art { get; set; }

        [Key(8)] public float Height { get; set; }

        [Key(9)] public long TribeTypeId { get; set; }

        [Key(10)] public bool PlayerRace { get; set; }

        [Key(11)] public List<UnitEffect> Effects { get; set; }

        [Key(12)] public List<WeightedName> PrefixNames { get; set; }

        [Key(13)] public List<WeightedName> DoubleNameSuffixes { get; set; }

        [Key(14)] public List<WeightedName> SuffixNames { get; set; }


        [Key(15)] public List<WeightedName> AlternateNames { get; set; }

        [Key(16)] public List<MonsterFood> FoodSources { get; set; }

        [Key(17)] public List<SpawnItem> LootItems { get; set; }
        [Key(18)] public List<SpawnItem> InteractLootItems { get; set; }

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
        public class UnitSettingsLoasder : ParentSettingsLoader<UnitSettings, UnitType> { }

    [MessagePackObject]
        public class UnitSettingsMapper : ParentSettingsMapper<UnitSettings, UnitType, UnitSettingsApi> { }
    }
}
