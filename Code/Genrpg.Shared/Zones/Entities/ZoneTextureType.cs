using MessagePack;
namespace Genrpg.Shared.Zones.Entities
{
    [MessagePackObject]
    public class ZoneTextureType
    {
        [Key(0)] public long TextureTypeId { get; set; }
        [Key(1)] public int TextureChannelId { get; set; }
    }
}
