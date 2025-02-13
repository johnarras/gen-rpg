using MessagePack;
using System.Collections.Generic;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.Achievements.Settings;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.Crafting.Settings.Recipes;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Entities.Helpers;

namespace Genrpg.Shared.Crafting.Settings.Crafters
{
    [MessagePackObject]
    public class CraftingSettings : ParentSettings<CrafterType>
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public int LootLevelIncrement { get; set; }
        [Key(2)] public int PointsPerLevel { get; set; }
        [Key(3)] public int PointsPerCraft { get; set; }
        [Key(4)] public int ExtraCraftLevelsAllowed { get; set; }
        [Key(5)] public int LevelsPerExtraEffect { get; set; }
        [Key(6)] public int MaxExtraEffects { get; set; }


        public CraftingSettings()
        {
            LootLevelIncrement = 25;
            PointsPerLevel = 5;
            PointsPerCraft = 1;
            ExtraCraftLevelsAllowed = 5;
            LevelsPerExtraEffect = 4;
            MaxExtraEffects = 5;
        }
    }
    [MessagePackObject]
    public class CrafterType : ChildSettings, IIndexedGameItem
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string ParentId { get; set; }
        [Key(2)] public long IdKey { get; set; }
        [Key(3)] public override string Name { get; set; }
        [Key(4)] public string Desc { get; set; }
        [Key(5)] public string Icon { get; set; }
        [Key(6)] public string Art { get; set; }

        [Key(7)] public string MousePointer { get; set; }

        [Key(8)] public long ReagentItemTypeId { get; set; }

        [Key(9)] public float GatherSeconds { get; set; }
        [Key(10)] public float CraftingSeconds { get; set; }
        [Key(11)] public string GatherActionName { get; set; }
        [Key(12)] public string CraftActionName { get; set; }
        [Key(13)] public string GatherAnimation { get; set; }
        [Key(14)] public string CraftAnimation { get; set; }
    }

    [MessagePackObject]
    public class CrafterSettingsApi : ParentSettingsApi<CraftingSettings, CrafterType> { }

    [MessagePackObject]
    public class CrafterSettingsLoader : ParentSettingsLoader<CraftingSettings, CrafterType> { }

    [MessagePackObject]
    public class CraftingSettingsMapper : ParentSettingsMapper<CraftingSettings, CrafterType, CrafterSettingsApi> { }



    public class CrafterSettingsHelper : BaseEntityHelper<CraftingSettings,CrafterType>
    {
        public override long GetKey() { return EntityTypes.Crafter; }
    }



}
