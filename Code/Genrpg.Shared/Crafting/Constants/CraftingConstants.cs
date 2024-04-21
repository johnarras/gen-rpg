using MessagePack;
namespace Genrpg.Shared.Crafting.Constants
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


        public const int MinReagentQuantity = 1;
        public const int BadReagentQuantity = 1000;


    }
}
