using Genrpg.Shared.Inventory.PlayerData;
using MessagePack;
namespace Genrpg.Shared.Vendors.WorldData
{
    [MessagePackObject]
    public class VendorItem
    {
        [Key(0)] public int Quantity { get; set; }
        [Key(1)] public Item Item { get; set; }
    }
}
