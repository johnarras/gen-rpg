using MessagePack;
namespace Genrpg.Shared.Zones.Settings
{
    [MessagePackObject]
    public class ZoneFenceType
    {
        [Key(0)] public long FenceTypeId { get; set; }
        [Key(1)] public float Chance { get; set; }
        [Key(2)] public string Name { get; set; }

        public ZoneFenceType()
        {
            Chance = 100;
        }
    }
}
