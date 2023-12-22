using MessagePack;
namespace Genrpg.Shared.Inventory.Settings.Qualities
{
    [MessagePackObject]
    public class QualityName
    {
        [Key(0)] public long QualityTypeId { get; set; }
        [Key(1)] public string Name { get; set; }
    }
}
