using MessagePack;
using Genrpg.Shared.Interfaces;

namespace Genrpg.Shared.Crafting.Entities
{
    [MessagePackObject]
    public class CrafterType : IIndexedGameItem
    {
        
        [Key(0)] public long IdKey { get; set; }
        [Key(1)] public string Name { get; set; }
        [Key(2)] public string Desc { get; set; }
        [Key(3)] public string Icon { get; set; }
        [Key(4)] public string Art { get; set; }

        [Key(5)] public string MousePointer { get; set; }

        [Key(6)] public long ReagentItemTypeId { get; set; }

        [Key(7)] public float GatherSeconds { get; set; }
        [Key(8)] public float CraftingSeconds { get; set; }
        [Key(9)] public string GatherActionName { get; set; }
        [Key(10)] public string CraftActionName { get; set; }
        [Key(11)] public string GatherAnimation { get; set; }
        [Key(12)] public string CraftAnimation { get; set; }

    }
}
