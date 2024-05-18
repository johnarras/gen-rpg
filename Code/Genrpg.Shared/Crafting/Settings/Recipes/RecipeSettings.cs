using MessagePack;
using System.Collections.Generic;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings.Mappers;

namespace Genrpg.Shared.Crafting.Settings.Recipes
{
    [MessagePackObject]
    public class Reagent
    {
        [Key(0)] public long EntityTypeId { get; set; }
        [Key(1)] public long EntityId { get; set; }
        [Key(2)] public int Quantity { get; set; }

        [Key(3)] public string Name { get; set; }
    }
    [MessagePackObject]
    public class RecipeScaling
    {
        [Key(0)] public long ScalingTypeId { get; set; }
    }
    [MessagePackObject]
    public class RecipeSettings : ParentSettings<RecipeType>
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public int LootLevelIncrement { get; set; }
        [Key(2)] public int PointsPerLevel { get; set; }
        [Key(3)] public int PointsPerCraft { get; set; }
        [Key(4)] public int ExtraCraftLevelsAllowed { get; set; }
        [Key(5)] public int LevelsPerExtraEffect { get; set; }
        [Key(6)] public int MaxExtraEffects { get; set; }
        /// <summary>
        /// this is 2.5 meaning each 2.5pct of scaling for the recipe requires 1 reagent in all slots.
        /// </summary>
        [Key(7)] public double ReagentQuantityPerPercent { get; set; } = 0.025;

    }

    [MessagePackObject]
    public class RecipeType : ChildSettings, IIndexedGameItem
    {

        public const string RecipeItemName = "Recipe";


        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string ParentId { get; set; }
        [Key(2)] public long IdKey { get; set; }
        [Key(3)] public override string Name { get; set; }
        [Key(4)] public string NameId { get; set; }
        [Key(5)] public string Desc { get; set; }
        [Key(6)] public string Icon { get; set; }
        [Key(7)] public long EntityId { get; set; }
        [Key(8)] public long EntityTypeId { get; set; }
        [Key(9)] public int MinQuantity { get; set; } = 1;
        [Key(10)] public int MaxQuantity { get; set; } = 1;
        [Key(11)] public string Art { get; set; }
        [Key(12)] public int ScalingPct { get; set; } = 100;


        /// <summary>
        /// Use this for recipes that have a list of reagents rather than a choice.
        /// </summary>
        [Key(13)] public long CrafterTypeId { get; set; }


        [Key(14)] public int Flags { get; set; }
        public bool HasFlag(int flagBits) { return (Flags & flagBits) != 0; }
        public void AddFlags(int flagBits) { Flags |= flagBits; }
        public void RemoveFlags(int flagBits) { Flags &= ~flagBits; }


        [Key(15)] public List<Reagent> ExplicitReagents { get; set; } = new List<Reagent>();

    }


    [MessagePackObject]
    public class RecipeSettingsApi : ParentSettingsApi<RecipeSettings, RecipeType> { }
    [MessagePackObject]
    public class RecipeSettingsLoader : ParentSettingsLoader<RecipeSettings, RecipeType> { }

    [MessagePackObject]
    public class RecipeSettingsMapper : ParentSettingsMapper<RecipeSettings, RecipeType, RecipeSettingsApi> { }
}
