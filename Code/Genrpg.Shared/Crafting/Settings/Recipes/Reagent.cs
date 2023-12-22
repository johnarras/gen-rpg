using MessagePack;
namespace Genrpg.Shared.Crafting.Settings.Recipes
{
    [MessagePackObject]
    public class Reagent
    {
        [Key(0)] public long EntityTypeId { get; set; }
        [Key(1)] public long EntityId { get; set; }
        [Key(2)] public int Quantity { get; set; }

        [Key(3)] public string Name { get; set; }
    }
}
