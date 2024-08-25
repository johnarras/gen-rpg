using MessagePack;
using Genrpg.Shared.Inventory.PlayerData;

namespace Genrpg.Shared.Rewards.Entities
{

    [MessagePackObject]
    public class Reward
    {
        [Key(0)] public long EntityTypeId { get; set; }
        [Key(1)] public long EntityId { get; set; }
        [Key(2)] public long Quantity { get; set; }
        [Key(3)] public long QualityTypeId { get; set; }
        [Key(4)] public long Level { get; set; }
        [Key(5)] public Item ExtraData { get; set; }

    }
}
