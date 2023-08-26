using MessagePack;
namespace Genrpg.Shared.Inventory.Entities
{
    [MessagePackObject]
    public class VendorItem
    {
        [Key(0)] public int Quantity { get; set; }
        [Key(1)] public Item Item { get; set; }
    }
}
