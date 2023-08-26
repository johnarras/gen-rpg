using MessagePack;
using System.Collections.Generic;

namespace Genrpg.Shared.Crafting.Entities
{
    [MessagePackObject]
    public class CraftingItemData
    {
        [Key(0)] public long RecipeTypeId { get; set; }
        [Key(1)] public long ScalingTypeId { get; set; }
        [Key(2)] public List<FullReagent> RecipeReagents { get; set; }
        [Key(3)] public FullReagent PrimaryReagent { get; set; }
        [Key(4)] public List<FullReagent> ExtraReagents { get; set; }


        public List<FullReagent> GetAllReagents()
        {
            List<FullReagent> retval = new List<FullReagent>();

            if (RecipeReagents != null)
            {
                retval.AddRange(RecipeReagents);
            }

            if (PrimaryReagent != null)
            {
                retval.Add(PrimaryReagent);
            }

            if (ExtraReagents != null)
            {
                retval.AddRange(ExtraReagents);
            }


            return retval;
        }
    }
}
