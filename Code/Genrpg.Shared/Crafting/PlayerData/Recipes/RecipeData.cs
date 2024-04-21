using Genrpg.Shared.Crafting.Constants;
using Genrpg.Shared.Crafting.PlayerData.Crafting;
using Genrpg.Shared.DataStores.Categories.PlayerData;
using Genrpg.Shared.DataStores.PlayerData;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Inventory.Entities;
using Genrpg.Shared.Units.Loaders;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Utils.Data;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Genrpg.Shared.Crafting.PlayerData.Recipes
{
    [MessagePackObject]
    public class RecipeStatus : OwnerPlayerData, IId
    {

        const int LevelId = 1;
        const int MaxLevelId = 2;

        [Key(0)] public override string Id { get; set; }
        [Key(1)] public long IdKey { get; set; }
        [Key(2)] public override string OwnerId { get; set; }

        [Key(3)] public List<IdVal> Levels { get; set; }

        public RecipeStatus()
        {
            Levels = new List<IdVal>();
        }

        protected IdVal GetById(int id, int startval)
        {
            if (Levels == null)
            {
                Levels = new List<IdVal>();
            }

            IdVal idval = Levels.FirstOrDefault(x => x.Id == id);
            if (idval == null)
            {
                idval = new IdVal() { Id = id };
                Levels.Add(idval);
            }
            if (idval.Val < startval)
            {
                idval.Val = startval;
            }

            return idval;
        }

        protected IdVal GetObject() { return GetById(LevelId, CraftingConstants.StartSkillLevel); }
        public int Get() { return GetObject().Val; }
        public void SetLevel(int level) { GetObject().Val = level; }
        public void AddLevel(int level) { GetObject().Val += level; }

        protected IdVal GetMaxLevelObject() { return GetById(MaxLevelId, CraftingConstants.StartMaxSkillLevel); }
        public int GetMaxLevel() { return GetMaxLevelObject().Val; }
        public void SetMaxLevel(int level) { GetMaxLevelObject().Val = level; }
        public void AddMaxLevel(int level) { GetMaxLevelObject().Val += level; }
    }
    [MessagePackObject]
    public class RecipeData : OwnerIdObjectList<RecipeStatus>
    {
        [Key(0)] public override string Id { get; set; }

        public override List<RecipeStatus> GetData()
        {
            return _data;
        }

        public void AddRecipeStatus(long recipeTypeId)
        {
            RecipeStatus status = new RecipeStatus()
            {
                IdKey = recipeTypeId,
                Id = HashUtils.NewGuid(),
                OwnerId = Id,
            };
        }
    }
    [MessagePackObject]
    public class RecipeDataApi : OwnerApiList<RecipeData, RecipeStatus> { }
    [MessagePackObject]
    public class RecipeDataLoader : OwnerIdDataLoader<RecipeData, RecipeStatus, RecipeDataApi> { }

}
