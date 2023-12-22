using MessagePack;
namespace Genrpg.Shared.Crafting.Settings.Recipes
{
    [MessagePackObject]
    public class RecipeScaling
    {
        [Key(0)] public long ScalingTypeId { get; set; }
    }
}
