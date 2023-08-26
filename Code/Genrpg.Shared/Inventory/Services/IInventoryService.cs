using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Inventory.Entities;
using Genrpg.Shared.Units.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Inventory.Services
{
    public interface IInventoryService : ISetupService
    {
        int InventorySpaceLeft(Unit unit, Item item);
        bool AddItem(GameState gs, Unit unit, Item item, bool forceAdd);
        bool UnequipItem(GameState gs, Unit unit, string itemId, bool calcStatsNow = true);
        Item RemoveItemQuantity(GameState gs, Unit unit, string itemId, int quantity);
        Item RemoveItem(GameState gs, Unit unit, string itemId,bool destroyItem);
        bool EquipItem(GameState gs, Unit unit, string itemId, long equipSlotId);
    }
}
