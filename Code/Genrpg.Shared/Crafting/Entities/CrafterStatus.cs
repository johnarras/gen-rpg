using MessagePack;

using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.DataStores.Core;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Units.Entities;
using System.Collections.Generic;
using System.Linq;
using Genrpg.Shared.Spells.Entities;

namespace Genrpg.Shared.Crafting.Entities
{
    [MessagePackObject]
    public class CrafterStatus : IStatusItem, IId
    {

        
        [Key(0)] public long IdKey { get; set; }
        [Key(1)] public int CraftingSkillPoints { get; set; }

        [Key(2)] public int GatheringSkillPoints { get; set; }


        public void AddSkillPoints(int skillCategory, int amount)
        {
            if (skillCategory == CraftingConstants.GatheringSkill)
            {
                GatheringSkillPoints += amount;
            }
            else if (skillCategory == CraftingConstants.CraftingSkill)
            {
                CraftingSkillPoints += amount;
            }
        }

        public int GetSkillPoints(int skillCategory)
        {
            if (skillCategory == CraftingConstants.CraftingSkill)
            {
                return CraftingSkillPoints;
            }
            else if (skillCategory == CraftingConstants.GatheringSkill)
            {
                return GatheringSkillPoints;
            }
            return 0;
        }

        public int GetLevel(GameState gs, int skillCategory)
        {
            if (skillCategory == CraftingConstants.CraftingSkill)
            {

                return 1 + CraftingSkillPoints / CraftingConstants.CraftingSkillPointsPerLevel;
            }
            else if (skillCategory == CraftingConstants.GatheringSkill)
            {

                return 1 + GatheringSkillPoints / CraftingConstants.GatheringSkillPointsPerLevel;
            }
            return 0;
        }
    }

    [MessagePackObject]
    public class CrafterData : IdObjectList<CrafterStatus>
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override List<CrafterStatus> Data { get; set; } = new List<CrafterStatus>();
        public override void AddTo(Unit unit) { unit.Set(this); }
        protected override bool CreateIfMissingOnGet()
        {
            return true;
        }
        public override void Delete(IRepositorySystem repoSystem) { repoSystem.Delete(this); }
    }

}
