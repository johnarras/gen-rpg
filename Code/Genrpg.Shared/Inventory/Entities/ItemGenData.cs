using MessagePack;
namespace Genrpg.Shared.Inventory.Entities
{
    [MessagePackObject]
    public class ItemGenData
    {
        [Key(0)] public Item oldItem { get; set; }
        [Key(1)] public long ItemTypeId { get; set; }
        [Key(2)] public long Level { get; set; }
        [Key(3)] public long QualityTypeId { get; set; }
        [Key(4)] public long Quantity { get; set; }

        public ItemGenData()
        {
            Level = 1;
            QualityTypeId = 0;
            Quantity = 1;
        }
    }
}
