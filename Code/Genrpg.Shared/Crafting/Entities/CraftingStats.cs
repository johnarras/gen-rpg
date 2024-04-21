using MessagePack;
using Genrpg.Shared.Stats.Entities;
using System.Collections.Generic;

namespace Genrpg.Shared.Crafting.Entities
{
    [MessagePackObject]
    public class CraftingStats
    {
        [Key(0)] public long EntityTypeId { get; set; }
        [Key(1)] public long EntityId { get; set; }
        [Key(2)] public List<Stat> Stats { get; set; }
        [Key(3)] public long Level { get; set; }
        [Key(4)] public long QualityTypeId { get; set; }
        [Key(5)] public long RecipeTypeId { get; set; }
        [Key(6)] public long ScalingTypeId { get; set; }
        [Key(7)] public int ReagentQuantity { get; set; }

        [Key(8)] public CraftingItemData Data { get; set; }

        [Key(9)] public bool IsValid { get; set; }

        [Key(10)] public string Message { get; set; }

        public CraftingStats()
        {
            Stats = new List<Stat>();
        }
    }
}
