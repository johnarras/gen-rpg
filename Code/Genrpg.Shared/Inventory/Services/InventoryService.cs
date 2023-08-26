
using Genrpg.Shared.Characters.Entities;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.DataStores.Constants;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Inventory.Entities;
using Genrpg.Shared.Inventory.Messages;
using Genrpg.Shared.MapMessages.Interfaces;
using Genrpg.Shared.Stats.Entities;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.Shared.Inventory.Services
{
    public class InventoryService : IInventoryService
    {
        private IStatService _statService = null;
        public virtual async Task Setup(GameState gs, CancellationToken token)
        {
            await Task.CompletedTask;
        }
        public virtual int InventorySpaceLeft(Unit unit, Item item)
        {
            return 1000;
        }

        public int InventorySpaceLeft(GameState gs, Character ch, Item item)
        {
            return 1000;
        }

        protected virtual void AddMessage(GameState gs, Unit unit, InventoryData idata, Item item, IMapApiMessage message, EDataUpdateTypes updateType = EDataUpdateTypes.Save)
        {
        }

        protected virtual void AddMessageNear(GameState gs, Unit unit, InventoryData idata, Item item, IMapApiMessage message, EDataUpdateTypes updateType = EDataUpdateTypes.Save)
        {

        }


        public bool AddItem(GameState gs, Unit unit, Item item, bool forceAdd)
        {
            InventoryData idata = unit.Get<InventoryData>();
            if (idata == null)
            {
                return false;
            }

            if (string.IsNullOrEmpty(item.Id))
            {
                item.Id = HashUtils.NewGuid();
            }
            ItemType itype = gs.data.GetGameData<ItemSettings>().GetItemType(item.ItemTypeId);
            if (itype == null)
            {
                return false;
            }

            Item currItem = idata.GetItem(item.Id);
            if (currItem != null)
            {
                return false;
            }

            if (itype.CanStack())
            {
                Item stackItem = idata.GetMatchingStackItem(item);

                if (stackItem != null)
                {
                    stackItem.Quantity += item.Quantity;
                    AddMessage(gs, unit, idata, stackItem, new OnUpdateItem { Item = stackItem, UnitId = unit.Id });
                    return true;
                }
            }
            idata.AddInventory(item);
            item.OwnerId = unit.Id;

            AddMessage(gs, unit, idata, item, new OnAddItem() { ItemId = item.Id, UnitId = unit.Id });
            return true;
        }

        public virtual Item RemoveItemQuantity(GameState gs, Unit unit, string itemId, int quantity)
        {
            if (quantity < 1)
            {
                return null;
            }

            InventoryData idata = unit.Get<InventoryData>();

            Item item = idata.GetItem(itemId);
            if (item == null)
            {
                return null;
            }

            item.Quantity -= quantity;
            if (item.Quantity < 1)
            {
                idata.RemoveInventory(gs, item);
                AddMessage(gs, unit, idata, item, new OnRemoveItem() { ItemId = item.Id, UnitId = unit.Id }, EDataUpdateTypes.Delete);
            }
            else
            {
                AddMessage(gs, unit, idata, item, new OnUpdateItem { Item = item, UnitId = unit.Id });
            }

            return item;
        }

        public virtual Item RemoveItem(GameState gs, Unit unit, string itemId,bool deleteItem)
        {
            InventoryData idata = unit.Get<InventoryData>();

            Item item = idata.GetItem(itemId);
            if (item == null)
            {
                return null;
            }

            if (idata.GetItem(item.Id) == null)
            {
                return null;
            }

            idata.RemoveInventory(gs, item);
            AddMessage(gs, unit, idata, item, new OnRemoveItem() { ItemId = item.Id, UnitId = unit.Id },
                deleteItem ? EDataUpdateTypes.Delete : EDataUpdateTypes.Save);
            return item;
        }


        public virtual bool EquipItem(GameState gs, Unit unit, string itemId, long equipSlotId)
        {

            if (equipSlotId < 1 || equipSlotId >= EquipSlot.Max)
            {
                return false;
            }

            EquipSlot eqslot = gs.data.GetGameData<ItemSettings>().GetEquipSlot(equipSlotId);
            if (eqslot == null || !eqslot.Active)
            {
                return false;
            }
            InventoryData idata = unit.Get<InventoryData>();

            long oldEquipSlot = -1;

            Item item = idata.GetItem(itemId);
            if (item == null)
            {
                item = idata.GetEquipById(itemId);
                if (item != null)
                {
                    oldEquipSlot = item.EquipSlotId;
                    UnequipItem(gs, unit, itemId, false);
                }
                else
                {
                    return false;
                }
            }

            ItemType itype = gs.data.GetGameData<ItemSettings>().GetItemType(item.ItemTypeId);

            if (itype == null || itype.EquipSlotId < 1)
            {
                return false;
            }

            List<long> compatibleSlots = itype.GetCompatibleEquipSlots(gs);

            if (!compatibleSlots.Contains(equipSlotId))
            {
                return false;
            }


            // Get equipment out of the way.
            Item currEquip = idata.GetEquipBySlot(equipSlotId);
            if (currEquip != null)
            {
                if (itype.HasFlag(ItemType.FlagTwoHandedItem) || oldEquipSlot < 1)
                {
                    UnequipItem(gs, unit, currEquip.Id, false);
                }
                else
                {
                    ItemType currItemType = gs.data.GetGameData<ItemSettings>().GetItemType(currEquip.ItemTypeId);
                    if (currItemType == null || currItemType.HasFlag(ItemType.FlagTwoHandedItem) ||
                        currItemType.EquipSlotId == EquipSlot.OffHand)
                    {
                        UnequipItem(gs, unit, currEquip.Id, false);
                    }
                    else
                    {
                        List<long> currSlots = currItemType.GetCompatibleEquipSlots(gs);
                        if (currSlots.Contains(oldEquipSlot))
                        {
                            currEquip.EquipSlotId = oldEquipSlot;
                        }
                    }
                }
            }

            // Remove from inventory.
            RemoveItem(gs, unit, itemId, false);

            // Two handed weapons remove offhand items.
            if (FlagUtils.IsSet(itype.Flags, ItemType.FlagTwoHandedItem))
            {
                Item offhandEquip = idata.GetEquipBySlot(EquipSlot.OffHand);
                if (offhandEquip != null)
                {
                    UnequipItem(gs, unit, offhandEquip.Id, false);
                }
            }

            if (equipSlotId == EquipSlot.OffHand)
            {
                Item mainHandEquip = idata.GetEquipBySlot(EquipSlot.MainHand);
                if (mainHandEquip != null)
                {
                    ItemType mainHandItemType = gs.data.GetGameData<ItemSettings>().GetItemType(mainHandEquip.ItemTypeId);
                    if (mainHandItemType != null && FlagUtils.IsSet(mainHandItemType.Flags, ItemType.FlagTwoHandedItem))
                    {
                        UnequipItem(gs, unit, mainHandEquip.Id, false);
                    }
                }
            }

            item.EquipSlotId = equipSlotId;
            idata.AddEquipment(item);
            AddMessageNear(gs, unit, idata, item, new OnEquipItem() { Item = item, UnitId = unit.Id });

            _statService.CalcStats(gs, unit, false);
            return true;
        }

        public virtual bool UnequipItem(GameState gs, Unit unit, string itemId, bool calcStatsNow = true)
        {
            InventoryData idata = unit.Get<InventoryData>();
            Item item = idata.GetEquipById(itemId);

            if (item == null)
            {
                return false;
            }

            Item currItem = idata.GetEquipmentById(item.Id);

            if (currItem != null)
            {
                idata.RemoveEquipment(currItem.Id);
                AddMessageNear(gs, unit, idata, item, new OnUnequipItem() { ItemId = item.Id, UnitId = unit.Id });
                currItem.EquipSlotId = EquipSlot.None;
                if (calcStatsNow)
                {
                    _statService.CalcStats(gs, unit, false);
                }
            }

            AddItem(gs, unit, item, true);


            return true;
        }

        private void UpdateEffectsFromItem(GameState gs, Character ch, Item item, bool addStats)
        {
            if (item == null || item.Effects == null)
            {
                return;
            }

            int statMult = addStats ? 1 : -1;

            foreach (ItemEffect eff in item.Effects)
            {
                if (eff.EntityTypeId == EntityType.Stat)
                {
                    _statService.Add(gs, ch, eff.EntityId, StatCategory.Base, eff.Quantity * statMult);
                }
                else if (eff.EntityTypeId == EntityType.StatPct)
                {
                    _statService.Add(gs, ch, eff.EntityId, StatCategory.Pct, eff.Quantity * statMult);
                }
            }
        }

        public virtual List<Item> GetInventoryFromItemTypeId(Unit unit, int itemTypeId)
        {
            List<Item> retval = new List<Item>();
            InventoryData idata = unit.Get<InventoryData>();
            if (idata == null)
            {
                return retval;
            }

            return idata.GetItemsByItemTypeId(itemTypeId);
        }
    }
}
