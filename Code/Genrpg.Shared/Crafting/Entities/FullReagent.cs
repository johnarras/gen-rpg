using MessagePack;
using Genrpg.Shared.Inventory.Entities;

namespace Genrpg.Shared.Crafting.Entities
{
    [MessagePackObject]
    public class FullReagent
    {
        [Key(0)] public Reagent ReagentMappedTo { get; set; }
        [Key(1)] public ItemPct ItemMappedTo { get; set; }
        [Key(2)] public string ItemId { get; set; }
        [Key(3)] public long ItemTypeId { get; set; }
        [Key(4)] public int Quantity { get; set; }
        [Key(5)] public long Level { get; set; }
        [Key(6)] public long QualityTypeId { get; set; }
    }
}
