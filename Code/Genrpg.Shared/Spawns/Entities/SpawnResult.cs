using MessagePack;
using Genrpg.Shared.Inventory.PlayerData;

namespace Genrpg.Shared.Spawns.Entities
{

    public interface ISpawnResult
    {

        long EntityTypeId { get; set; }
        long EntityId { get; set; }
        long Quantity { get; set; }
        long QualityTypeId { get; set; }
        long Level { get; set; }
        Item Data { get; set; }
    }

    [MessagePackObject]
    public class SpawnResult : ISpawnResult
    {
        [Key(0)] public long EntityTypeId { get; set; }
        [Key(1)] public long EntityId { get; set; }
        [Key(2)] public long Quantity { get; set; }
        [Key(3)] public long QualityTypeId { get; set; }
        [Key(4)] public long Level { get; set; }
        [Key(5)] public Item Data { get; set; }

    }
}
