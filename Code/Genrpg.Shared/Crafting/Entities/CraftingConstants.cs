using MessagePack;
namespace Genrpg.Shared.Crafting.Entities
{
    [MessagePackObject]
    public class CraftingConstants
    {
        public const int RecipeStartMaxLevel = 10;



        public const int GatheringSkill = 1;
        public const int CraftingSkill = 2;



        public const int GatheringSkillPointsPerLevel = 5;

        public const int CraftingSkillPointsPerLevel = 2;


        public const int ExtraCraftingLevelAllowed = 3;


        public const int StartSkillLevel = 1;
        public const int StartMaxSkillLevel = 10;


    }
}
