using MessagePack;
namespace Genrpg.Shared.Zones.Entities
{
    [MessagePackObject]
    public class ZoneFenceType
    {
        [Key(0)] public long FenceTypeId { get; set; }
        [Key(1)] public float Chance { get; set; }

        public ZoneFenceType()
        {
            Chance = 100;
        }
    }
}
