using MessagePack;
using Genrpg.Shared.Inventory.PlayerData;
using Genrpg.Shared.Crafting.Entities;

namespace Genrpg.Shared.Crafting.Messages
{
    [MessagePackObject]
    public class CraftingResult
    {
        [Key(0)] public CraftingStats Stats { get; set; }

        [Key(1)] public Item CraftedItem { get; set; }

        [Key(2)] public bool Succeeded { get; set; }

        [Key(3)] public string Message { get; set; }

    }
}
