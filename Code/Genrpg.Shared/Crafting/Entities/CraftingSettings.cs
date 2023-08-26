using MessagePack;
using Genrpg.Shared.DataStores.Categories;
using Genrpg.Shared.GameDatas;
using System.Collections.Generic;

namespace Genrpg.Shared.Crafting.Entities
{
    [MessagePackObject]
    public class CraftingSettings : BaseGameData
    {
        public override void Set(GameData gameData) { gameData.Set(this); }
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public int LootLevelIncrement { get; set; }
        [Key(2)] public int PointsPerLevel { get; set; }
        [Key(3)] public int PointsPerCraft { get; set; }
        [Key(4)] public int ExtraCraftLevelsAllowed { get; set; }
        [Key(5)] public int LevelsPerExtraEffect { get; set; }
        [Key(6)] public int MaxExtraEffects { get; set; }

        [Key(7)] public List<RecipeType> RecipeTypes { get; set; }
        [Key(8)] public List<CrafterType> CrafterTypes { get; set; }

        public RecipeType GetRecipeType(long idkey) { return _lookup.Get<RecipeType>(idkey); }
        public CrafterType GetCrafterType(long idkey) { return _lookup.Get<CrafterType>(idkey);}

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
}
