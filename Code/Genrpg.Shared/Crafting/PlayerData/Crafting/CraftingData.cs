using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Crafting.Constants;
using Genrpg.Shared.DataStores.Categories.PlayerData;
using Genrpg.Shared.DataStores.PlayerData;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Units.Loaders;
using MessagePack;

namespace Genrpg.Shared.Crafting.PlayerData.Crafting
{
    [MessagePackObject]
    public class CraftingStatus : OwnerPlayerData, IId
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string OwnerId { get; set; }
        [Key(2)] public long IdKey { get; set; }

        [Key(3)] public int CraftingSkillPoints { get; set; }

        [Key(4)] public int GatheringSkillPoints { get; set; }

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
    public class CraftingData : OwnerIdObjectList<CraftingStatus>
    {
        [Key(0)] public override string Id { get; set; }
    }
    [MessagePackObject]
    public class CraftingApi : OwnerApiList<CraftingData, CraftingStatus> { }
    [MessagePackObject]
    public class CrafterDataLoader : OwnerDataLoader<CraftingData, CraftingStatus, CraftingApi> { }
}
