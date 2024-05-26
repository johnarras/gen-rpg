using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Inventory.PlayerData;
using Genrpg.Shared.Units.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Inventory.Services
{
    public interface IInventoryService : IInitializable
    {
        int InventorySpaceLeft(Unit unit, Item item);
        bool AddItem(Unit unit, Item item, bool forceAdd);
        bool UnequipItem(Unit unit, string itemId, bool calcStatsNow = true);
        Item RemoveItemQuantity(Unit unit, string itemId, int quantity);
        Item RemoveItem(Unit unit, string itemId,bool destroyItem);
        bool EquipItem(Unit unit, string itemId, long equipSlotId, bool calcStatsNow = true);
        bool CanEquipItem(Unit unit, Item item);
    }
}
