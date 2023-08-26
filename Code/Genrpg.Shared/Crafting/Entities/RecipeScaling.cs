using MessagePack;
namespace Genrpg.Shared.Crafting.Entities
{
    [MessagePackObject]
    public class RecipeScaling
    {
        [Key(0)] public long ScalingTypeId { get; set; }
    }
}
