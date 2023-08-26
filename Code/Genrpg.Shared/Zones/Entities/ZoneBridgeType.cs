using MessagePack;
namespace Genrpg.Shared.Zones.Entities
{
    [MessagePackObject]
    public class ZoneBridgeType
    {
        [Key(0)] public long BridgeTypeId { get; set; }
        [Key(1)] public int Chance { get; set; }

        public ZoneBridgeType()
        {
        }
    }
}
