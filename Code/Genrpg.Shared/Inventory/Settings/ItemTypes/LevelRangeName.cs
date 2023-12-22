using MessagePack;
namespace Genrpg.Shared.Inventory.Settings.ItemTypes
{
    [MessagePackObject]
    public class LevelRangeName
    {
        [Key(0)] public long MinLevel { get; set; }
        [Key(1)] public long MaxLevel { get; set; }
        [Key(2)] public string Name { get; set; }

        [Key(3)] public string Icon { get; set; }
    }
}
