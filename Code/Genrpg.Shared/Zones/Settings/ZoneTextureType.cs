using MessagePack;
namespace Genrpg.Shared.Zones.Settings
{
    [MessagePackObject]
    public class ZoneTextureType
    {
        [Key(0)] public long TextureTypeId { get; set; }
        [Key(1)] public int TextureChannelId { get; set; }
        [Key(2)] public string Name { get; set; }
    }
}
