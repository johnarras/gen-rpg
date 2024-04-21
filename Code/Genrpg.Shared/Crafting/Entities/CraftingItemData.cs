using MessagePack;
using System.Collections.Generic;

namespace Genrpg.Shared.Crafting.Entities
{
    [MessagePackObject]
    public class CraftingItemData
    {
        [Key(0)] public long RecipeTypeId { get; set; }
        [Key(1)] public long ScalingTypeId { get; set; }
        [Key(2)] public FullReagent BaseScalingReagent { get; set; }
        [Key(3)] public List<FullReagent> StatReagents { get; set; } = new List<FullReagent>();
        [Key(4)] public List<FullReagent> LevelQualityReagents { get; set; } = new List<FullReagent>();


        public List<FullReagent> GetAllReagents()
        {
            List<FullReagent> retval = new List<FullReagent>();

            retval.Add(BaseScalingReagent);
            retval.AddRange(StatReagents);
            retval.AddRange(LevelQualityReagents);

            return retval;
        }
    }
}
