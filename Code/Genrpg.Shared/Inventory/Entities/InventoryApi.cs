using MessagePack;
using System.Collections.Generic;
using Genrpg.Shared.DataStores.PlayerData;

namespace Genrpg.Shared.Inventory.Entities
{
    [MessagePackObject]
    public class InventoryApi : OwnerApiList<InventoryData,Item>
    {
        [Key(0)] public List<Item> AllItems { get; set; } = new List<Item>();

    }
}
