using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Crawler.Roles.Settings;
using Genrpg.Shared.DataStores.Constants;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Inventory.Constants;
using Genrpg.Shared.Inventory.Messages;
using Genrpg.Shared.Inventory.PlayerData;
using Genrpg.Shared.Inventory.Settings.ItemTypes;
using Genrpg.Shared.Inventory.Settings.Slots;
using Genrpg.Shared.MapMessages.Interfaces;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.Stats.Constants;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Genrpg.Shared.Inventory.Services
{
    public class InventoryService : IInventoryService
    {
        private IStatService _statService = null;
        protected IGameData _gameData;

        public virtual int InventorySpaceLeft(MapObject unit, Item item)
        {
            return 1000;
        }

        public int InventorySpaceLeft(Character ch, Item item)
        {
            return 1000;
        }

        protected virtual void AddMessage(MapObject unit, InventoryData idata, Item item, IMapApiMessage message, EDataUpdateTypes updateType = EDataUpdateTypes.Save)
        {
        }

        protected virtual void AddMessageNear(MapObject unit, InventoryData idata, Item item, IMapApiMessage message, EDataUpdateTypes updateType = EDataUpdateTypes.Save)
        {

        }

        public virtual bool AddItem(MapObject unit, Item item, bool forceAdd)
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
            ItemType itype = _gameData.Get<ItemTypeSettings>(unit).Get(item.ItemTypeId);
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
                    AddMessage(unit, idata, stackItem, new OnUpdateItem { Item = stackItem, UnitId = unit.Id });
                    return true;
                }
            }
            idata.AddInventory(item);
            item.OwnerId = unit.Id;

            AddMessage(unit, idata, item, new OnAddItem() { ItemId = item.Id, UnitId = unit.Id });
            return true;
        }

        public virtual Item RemoveItemQuantity(MapObject unit, string itemId, int quantity)
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
                idata.RemoveInventory(item);
                AddMessage(unit, idata, item, new OnRemoveItem() { ItemId = item.Id, UnitId = unit.Id }, EDataUpdateTypes.Delete);
            }
            else
            {
                AddMessage(unit, idata, item, new OnUpdateItem { Item = item, UnitId = unit.Id });
            }

            return item;
        }

        public virtual Item RemoveItem(MapObject unit, string itemId,bool deleteItem)
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

            idata.RemoveInventory(item);
            AddMessage(unit, idata, item, new OnRemoveItem() { ItemId = item.Id, UnitId = unit.Id },
                deleteItem ? EDataUpdateTypes.Delete : EDataUpdateTypes.Save);
            return item;
        }


        public virtual bool EquipItem(MapObject obj, string itemId, long equipSlotId, bool calcStatsNow = true)
        {

            if (equipSlotId < 1 || equipSlotId >= EquipSlots.Max)
            {
                return false;
            }

            EquipSlot eqslot = _gameData.Get<EquipSlotSettings>(obj).Get(equipSlotId);
            if (eqslot == null || !eqslot.Active)
            {
                return false;
            }
            InventoryData idata = obj.Get<InventoryData>();

            long oldEquipSlot = -1;

            Item item = idata.GetItem(itemId);
            if (item == null)
            {
                item = idata.GetEquipById(itemId);
                if (item != null)
                {
                    oldEquipSlot = item.EquipSlotId;
                    UnequipItem(obj, itemId, false);
                }
                else
                {
                    return false;
                }
            }

            if (!CanEquipItem(obj, item))
            {
                return false;
            }

            ItemType itype = _gameData.Get<ItemTypeSettings>(obj).Get(item.ItemTypeId);

            if (itype == null || itype.EquipSlotId < 1)
            {
                return false;
            }

            List<long> compatibleSlots = itype.GetCompatibleEquipSlots(_gameData, obj);

            if (!compatibleSlots.Contains(equipSlotId))
            {
                return false;
            }


            // Get equipment out of the way.
            Item currEquip = idata.GetEquipBySlot(equipSlotId);
            if (currEquip != null)
            {
                if (itype.HasFlag(ItemFlags.FlagTwoHandedItem) || oldEquipSlot < 1)
                {
                    UnequipItem(obj, currEquip.Id, false);
                }
                else
                {
                    ItemType currItemType = _gameData.Get<ItemTypeSettings>(obj).Get(currEquip.ItemTypeId);
                    if (currItemType == null || currItemType.HasFlag(ItemFlags.FlagTwoHandedItem) ||
                        currItemType.EquipSlotId == EquipSlots.OffHand)
                    {
                        UnequipItem(obj, currEquip.Id, false);
                    }
                    else
                    {
                        List<long> currSlots = currItemType.GetCompatibleEquipSlots(_gameData, obj);
                        if (currSlots.Contains(oldEquipSlot))
                        {
                            currEquip.EquipSlotId = oldEquipSlot;
                        }
                    }
                }
            }

            // Remove from inventory.
            RemoveItem(obj, itemId, false);

            // Two handed weapons remove offhand items.
            if (FlagUtils.IsSet(itype.Flags, ItemFlags.FlagTwoHandedItem))
            {
                Item offhandEquip = idata.GetEquipBySlot(EquipSlots.OffHand);
                if (offhandEquip != null)
                {
                    UnequipItem(obj, offhandEquip.Id, false);
                }
            }

            if (equipSlotId == EquipSlots.OffHand)
            {
                Item mainHandEquip = idata.GetEquipBySlot(EquipSlots.MainHand);
                if (mainHandEquip != null)
                {
                    ItemType mainHandItemType = _gameData.Get<ItemTypeSettings>(obj).Get(mainHandEquip.ItemTypeId);
                    if (mainHandItemType != null && FlagUtils.IsSet(mainHandItemType.Flags, ItemFlags.FlagTwoHandedItem))
                    {
                        UnequipItem(obj, mainHandEquip.Id, false);
                    }
                }
            }

            item.EquipSlotId = equipSlotId;
            idata.AddEquipment(item);
            AddMessageNear(obj, idata, item, new OnEquipItem() { Item = item, UnitId = obj.Id });

            if (calcStatsNow && obj is Unit unit)
            {
                _statService.CalcStats(unit, false);
            }
            return true;
        }

        public virtual bool UnequipItem(MapObject obj, string itemId, bool calcStatsNow = true)
        {
            InventoryData idata = obj.Get<InventoryData>();
            Item item = idata.GetEquipById(itemId);

            if (item == null)
            {
                return false;
            }

            Item currItem = idata.GetEquipmentById(item.Id);

            if (currItem != null)
            {
                idata.RemoveEquipment(currItem.Id);
                AddMessageNear(obj, idata, item, new OnUnequipItem() { ItemId = item.Id, UnitId = obj.Id });
                currItem.EquipSlotId = EquipSlots.None;
                if (calcStatsNow && obj is Unit unit)
                {
                    _statService.CalcStats(unit, false);
                }
            }

            AddItem(obj, item, true);


            return true;
        }

        private void UpdateEffectsFromItem(Character ch, Item item, bool addStats)
        {
            if (item == null || item.Effects == null)
            {
                return;
            }

            int statMult = addStats ? 1 : -1;

            foreach (ItemEffect eff in item.Effects)
            {
                if (eff.EntityTypeId == EntityTypes.Stat)
                {
                    _statService.Add(ch, eff.EntityId, StatCategories.Base, eff.Quantity * statMult);
                }
                else if (eff.EntityTypeId == EntityTypes.StatPct)
                {
                    _statService.Add(ch, eff.EntityId, StatCategories.Pct, eff.Quantity * statMult);
                }
            }
        }

        public virtual List<Item> GetInventoryFromItemTypeId(MapObject unit, int itemTypeId)
        {
            List<Item> retval = new List<Item>();
            InventoryData idata = unit.Get<InventoryData>();
            if (idata == null)
            {
                return retval;
            }

            return idata.GetItemsByItemTypeId(itemTypeId);
        }

        public bool CanEquipItem (MapObject unit, Item item)
        {
            ItemType itype = _gameData.Get<ItemTypeSettings>(unit).Get(item.ItemTypeId);

            if (itype.EquipSlotId < 1)
            {
                return false;
            }

            EquipSlot slot = _gameData.Get<EquipSlotSettings>(unit).Get(itype.EquipSlotId);

            if (slot != null)
            {
                if (!slot.Active)
                {
                    return false;
                }

                List<Role> roles = _gameData.Get<RoleSettings>(unit).GetRoles(unit.Roles);

                if (roles.Count < 1)
                {
                    return true;
                }

                if ((slot.IdKey == EquipSlots.MainHand || slot.IdKey == EquipSlots.Ranged))
                {
                    if (roles.Any(x => x.Bonuses.Any(x => x.EntityTypeId == EntityTypes.Item && x.EntityId == itype.IdKey)))
                    {
                        return true;
                    }
                }
                else if (roles.Any(x => x.MaxArmorScalingTypeId >= item.ScalingTypeId))
                {
                    return true;
                }

                return false;
            }

            return true;
        }
    }
}
