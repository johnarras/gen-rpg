using MessagePack;
namespace Genrpg.Shared.Spawns.Entities
{
    [MessagePackObject]
    public class SpawnItem
    {
        [Key(0)] public long EntityTypeId { get; set; }
        [Key(1)] public long EntityId { get; set; }
        [Key(2)] public long MinQuantity { get; set; }
        [Key(3)] public long MaxQuantity { get; set; }
        [Key(4)] public double Weight { get; set; }
        [Key(5)] public int GroupId { get; set; }

        public SpawnItem()
        {
            MinQuantity = 1;
            MaxQuantity = 1;
            Weight = 100.0;
        }
    }
}
