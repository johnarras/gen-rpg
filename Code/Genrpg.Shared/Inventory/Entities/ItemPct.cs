using MessagePack;
namespace Genrpg.Shared.Inventory.Entities
{
    /// <summary>
    /// This is used to list required Base reagents for crafting. It's a 
    /// percent so it scales according to the recipe core cost.
    /// </summary>
    [MessagePackObject]
    public class ItemPct
    {
        [Key(0)] public long ItemTypeId { get; set; }
        [Key(1)] public int Percent { get; set; }
    }
}
