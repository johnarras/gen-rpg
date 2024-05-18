using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Inventory.Settings.ItemTypes;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Inventory.Settings.LootRanks
{
    /// <summary>
    /// List of equipment slots for characters
    /// </summary>
    [MessagePackObject]
    public class LootRank : ChildSettings, IIndexedGameItem
    {

        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string ParentId { get; set; }
        [Key(2)] public long IdKey { get; set; }
        [Key(3)] public override string Name { get; set; }
        [Key(4)] public string Desc { get; set; }
        [Key(5)] public string Icon { get; set; }
        [Key(6)] public string Art { get; set; }

        [Key(7)] public long Armor { get; set; }
        [Key(8)] public long Damage { get; set; }

        [Key(9)] public long CostPct { get; set; } = 100;

    }

    [MessagePackObject]
    public class LootRankSettings : ParentSettings<LootRank>
    {
        [Key(0)] public override string Id { get; set; }

        [Key(1)] public double LevelsPerQuality { get; set; } = 5.0f;

        [Key(2)] public double ExtraQualityChance { get; set; } = 0.25f;

        [Key(3)] public double ArmorChance { get; set; } = 0.75f;
    }

    [MessagePackObject]
    public class LootRankSettingsApi : ParentSettingsApi<LootRankSettings, LootRank> { }
    [MessagePackObject]
    public class LootRankSettingsLoader : ParentSettingsLoader<LootRankSettings, LootRank> { }

    [MessagePackObject]
    public class ItemSettingsMapper : ParentSettingsMapper<LootRankSettings, LootRank, LootRankSettingsApi> { }

}
