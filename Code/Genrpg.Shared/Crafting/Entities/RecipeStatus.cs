using MessagePack;
using Genrpg.Shared.DataStores.Core;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.Utils.Data;
using System.Collections.Generic;
using System.Linq;


namespace Genrpg.Shared.Crafting.Entities
{
    [MessagePackObject]
    public class RecipeStatus : IStatusItem, IId
    {

        const int LevelId = 1;
        const int MaxLevelId = 2;

        
        [Key(0)] public long IdKey { get; set; }

        [Key(1)] public List<IdVal> Levels { get; set; }

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


    [MessagePackObject]
    public class RecipeData : IdObjectList<RecipeStatus>
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override List<RecipeStatus> Data { get; set; } = new List<RecipeStatus>();
        public override void AddTo(Unit unit) { unit.Set(this); }
        protected override bool CreateIfMissingOnGet()
        {
            return false;
        }
        public override void Delete(IRepositorySystem repoSystem) { repoSystem.Delete(this); }
    }

}
