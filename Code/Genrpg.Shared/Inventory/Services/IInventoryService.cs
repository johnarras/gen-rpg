using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Inventory.PlayerData;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.Units.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Inventory.Services
{
    public interface IInventoryService : IInjectable
    {
        int InventorySpaceLeft(MapObject obj, Item item);
        bool AddItem(MapObject obj, Item item, bool forceAdd);
        bool UnequipItem(MapObject obj, string itemId, bool calcStatsNow = true);
        Item RemoveItemQuantity(MapObject obj, string itemId, int quantity);
        Item RemoveItem(MapObject obj, string itemId,bool destroyItem);
        bool EquipItem(MapObject obj, string itemId, long equipSlotId, bool calcStatsNow = true);
        bool CanEquipItem(MapObject obj, Item item);
    }
}
