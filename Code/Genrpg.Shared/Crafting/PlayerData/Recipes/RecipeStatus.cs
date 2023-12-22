using MessagePack;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.Utils.Data;
using System.Collections.Generic;
using System.Linq;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.DataStores.Categories.PlayerData;
using Genrpg.Shared.Crafting.Constants;

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

        protected IdVal GetLevelById(int id, int startval)
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

        protected IdVal GetLevelObject() { return GetLevelById(LevelId, CraftingConstants.StartSkillLevel); }
        public int GetLevel() { return GetLevelObject().Val; }
        public void SetLevel(int level) { GetLevelObject().Val = level; }
        public void AddLevel(int level) { GetLevelObject().Val += level; }

        protected IdVal GetMaxLevelObject() { return GetLevelById(MaxLevelId, CraftingConstants.StartMaxSkillLevel); }
        public int GetMaxLevel() { return GetMaxLevelObject().Val; }
        public void SetMaxLevel(int level) { GetMaxLevelObject().Val = level; }
        public void AddMaxLevel(int level) { GetMaxLevelObject().Val += level; }
    }
}
