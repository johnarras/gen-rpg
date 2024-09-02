using System.Collections.Generic;
using GEntity = UnityEngine.GameObject;
using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Inventory.PlayerData;
using Genrpg.Shared.Inventory.Services;

using System.Threading;
using Genrpg.Shared.Inventory.Messages;
using Genrpg.Shared.Inventory.Constants;
using Genrpg.Shared.Inventory.Settings.ItemTypes;
using Genrpg.Shared.Stats.Settings.Stats;
using Genrpg.Shared.Inventory.Settings.Slots;
using Genrpg.Shared.Units.Entities;
using UnityEngine;

public class CharacterScreen : ItemIconScreen
{
    public GText BasicInfo;
    public InventoryPanel Items;
    public List<EquipSlotIcon> EquipmentIcons;
    public List<StatInfoRow> Stats;
    public GEntity StatGridParent;

    protected virtual bool CalcStatsOnEquipUnequip() { return true; }
    protected virtual string GetStatSubdirectory() { return "Units"; }
    protected virtual bool ShowZeroStats() { return true; }

    protected IInventoryService _inventoryService;
    protected IClientMapObjectManager _objectManager;
    class StatDownloadData
    {
        public Unit currUnit = null;
        public long statTypeId = 0;
    }
    
    protected Unit _unit = null;

    public override Unit GetUnit() { return _unit; }

    protected override async Awaitable OnStartOpen(object data, CancellationToken token)
    {
        await base.OnStartOpen(data, token);
        _dispatcher.AddEvent<OnUnequipItem>(this, OnUnequip);
        _dispatcher.AddEvent<OnEquipItem>(this, OnEquip);

        if (_unit == null)
        {
            _unit = _gs.ch;
        }
        InitEquipmentIcons();

        ShowStats();

        if (Items != null)
        {
            Items.Init(InventoryGroup.Equipment, this, _unit, null, token);
        }
    }

    private void InitEquipmentIcons()
    {
        if (EquipmentIcons == null || _unit == null)
        {
            return;
        }

        InventoryData inventory = _unit.Get<InventoryData>();
        List<Item> allEquipment = inventory.GetAllEquipment();
        foreach (EquipSlotIcon icon in EquipmentIcons)
        {
            InitEquipmentIcon(icon);
        }
    }

    public void InitEquipmentIcon(EquipSlotIcon eqIcon)
    {
        if (eqIcon == null || _unit == null)
        {
            return;
        }

        if (eqIcon.Icon == null)
        {
            GEntityUtils.SetActive(eqIcon.entity(), false);
        }

        InventoryData inventory = _unit.Get<InventoryData>();
        Item currentEquipment = inventory.GetEquipBySlot(eqIcon.EquipSlotId);
        InitItemIconData iconInitData = new InitItemIconData()
        {
            Data = currentEquipment,
            Screen = this,
        };
        EquipSlot slot = _gameData.Get<EquipSlotSettings>(_unit).Get(eqIcon.EquipSlotId);
        eqIcon.Icon.Init(iconInitData, _token);
        _uIInitializable.SetText(eqIcon.Name, slot?.Name ?? "");
    }

    protected virtual void ShowStats()
    {
        if (_unit == null)
        {
            return;
        }

        if (StatGridParent == null)
        {
            return;
        }

        if (Stats == null)
        {
            Stats = new List<StatInfoRow>();
        }

        if (Stats.Count < 1)
        {
            IReadOnlyList<StatType> allstats = _gameData.Get<StatSettings>(_unit).GetData();
            if (allstats == null)
            {
                return;
            }

            int prevStat = -1;
            foreach (StatType stat in allstats)
            {
                if (stat.IdKey < 1)
                {
                    continue;
                }

                if (!ShowZeroStats() && _unit.Stats.Max(stat.IdKey) < 1)
                {
                    continue;
                }

                if (prevStat > 0 && prevStat/10 != stat.IdKey /10)
                {
                    StatDownloadData sddFill = new StatDownloadData()
                    {
                        currUnit = _unit,
                        statTypeId = -1,
                    };
                    _assetService.LoadAssetInto(StatGridParent, AssetCategoryNames.UI, 
                        "StatInfoRow", OnDownloadStat, sddFill, _token, GetStatSubdirectory());
                }
                StatDownloadData sdd = new StatDownloadData()
                {
                    currUnit = _unit,
                    statTypeId = stat.IdKey,
                };
                _assetService.LoadAssetInto(StatGridParent, AssetCategoryNames.UI,
                    "StatInfoRow", OnDownloadStat, sdd, _token, GetStatSubdirectory());
            }
        }
        else 
        {
            foreach (StatInfoRow row in Stats)
            {
                row.Init(_unit, -1, 0);
            }
        }
    }

    private void OnDownloadStat (object obj, object data, CancellationToken token)
    {
        GEntity go = obj as GEntity;
        if (go == null)
        {
            return;
        }

        StatDownloadData downloadData = data as StatDownloadData;

        if (downloadData == null || downloadData.currUnit == null || downloadData.statTypeId < 1)
        {
            GEntityUtils.Destroy(go);
            return;
        }


        StatInfoRow statRow = go.GetComponent<StatInfoRow>();
        if (statRow == null)
        {
            GEntityUtils.Destroy(go);
            return;
        }
        if (Stats == null)
        {
            Stats = new List<StatInfoRow>();
        }

        Stats.Add(statRow);

        statRow.Init(downloadData.currUnit, downloadData.statTypeId, 0);

    }

    // Blank
    public override void OnLeftClickIcon(ItemIcon icon) { }


    // Equip or Unequip item.
    public override void OnRightClickIcon(ItemIcon icon)
    {
        if (icon == null || icon.GetDataItem() == null)
        {
            return;
        }

    }


    public EquipSlotIcon GetIconFromSlot(long equipSlotId)
    {
        if (EquipmentIcons == null)
        {
            return null;
        }

        foreach (EquipSlotIcon icon in EquipmentIcons)
        {
            if (icon != null && icon.EquipSlotId == equipSlotId)
            {
                return icon;
            }
        }
        return null;
    }

    protected void EquipItem(ItemIcon icon, long newEquipSlotId)
    {

        if (icon == null || icon.GetDataItem()== null)
        {
            return;
        }

        ItemType itype = _gameData.Get<ItemTypeSettings>(_unit).Get(icon.GetDataItem().ItemTypeId);
        if (itype == null || itype.EquipSlotId < 1)
        {
            return;
        }

        long currEquipSlotId = icon.GetDataItem().EquipSlotId;

        Item origItem = icon.GetDataItem();

        if (currEquipSlotId == newEquipSlotId)
        {
            return;
        }

        List<long> equipSlots = itype.GetCompatibleEquipSlots(_gameData, _unit);
        List<long> relatedSlots = itype.GetRelatedEquipSlots(_gameData, _unit);

        Dictionary<long, Item> _currentRelatedItems = new Dictionary<long, Item>();

        InventoryData inventory = _unit.Get<InventoryData>();
        foreach (long relSlot in relatedSlots)
        {
            if (relSlot != newEquipSlotId && relSlot != currEquipSlotId)
            {
                Item currEq = inventory.GetEquipBySlot(relSlot);
                if (currEq != null)
                {
                    _currentRelatedItems[relSlot] = currEq;
                }
            }
        }

        if (!equipSlots.Contains(newEquipSlotId))
        {
            return;
        }

        TryEquip(origItem, newEquipSlotId);
    }

    protected virtual void TryEquip(Item origItem, long equipSlotId)
    {
        EquipItem equip = new EquipItem()
        {
            ItemId = origItem.Id,
            EquipSlot = equipSlotId,
        };

        _networkService.SendMapMessage(equip);
    }

    protected void OnEquip(OnEquipItem equip)
    {
        if (equip.Item == null)
        {
            return;
        }

        if (_unit.Id != equip.UnitId)
        {
            return;
        }

        Items.RemoveIcon(equip.Item.Id);

        InventoryData inventory = _unit.Get<InventoryData>();

        Item origItem = inventory.GetItem(equip.Item.Id);

        if (origItem == null)
        {
            return;
        }

        if (origItem.EquipSlotId == equip.Item.EquipSlotId)
        {
            return;
        }

        long currEquipSlotId = origItem.EquipSlotId;
        long newEquipSlotId = equip.Item.EquipSlotId;
        
        ItemType itype = _gameData.Get<ItemTypeSettings>(_unit).Get(equip.Item.ItemTypeId);

        List<long> equipSlots = itype.GetCompatibleEquipSlots(_gameData, _unit);
        List<long> relatedSlots = itype.GetRelatedEquipSlots(_gameData, _unit);

        EquipSlotIcon currEqIcon = GetIconFromSlot(currEquipSlotId);
        EquipSlotIcon newEqIcon = GetIconFromSlot(newEquipSlotId);

        Item existingItemInNewSlot = inventory.GetEquipBySlot(newEquipSlotId);

        Item existingEquipmentInNewSlot = inventory.GetEquipBySlot(newEquipSlotId);

        if (!_inventoryService.EquipItem(_unit, equip.Item.Id, newEquipSlotId, CalcStatsOnEquipUnequip()))
        {
            return;
        }

        InitEquipmentIcon(GetIconFromSlot(newEquipSlotId));

        InitEquipmentIcon(GetIconFromSlot(currEquipSlotId));

        foreach (long relSlot in relatedSlots)
        {
            if (relSlot != newEquipSlotId && relSlot != currEquipSlotId)
            {
                InitEquipmentIcon(GetIconFromSlot(relSlot));
            }
        }

        // Handle original icon. 
        // If there was no icon or the icon ended up not being equipped, destroy it. 
        // if it's not an equipment icon.
        if (existingEquipmentInNewSlot == null || existingEquipmentInNewSlot.EquipSlotId > 0)
        {
            EquipSlotIcon eqIcon = newEqIcon.GetComponent<EquipSlotIcon>();
            if (eqIcon == null)
            {
                GEntityUtils.Destroy(newEqIcon.entity());
            }
        }
        else // Otherwise set the icon data that we were dragging to the currently equipped item data.
        {
            newEqIcon.Icon.SetDataItem(existingEquipmentInNewSlot);
            newEqIcon.Icon.AddFlags(ItemIconFlags.ShowTooltipNow);
            newEqIcon.Icon.Init(newEqIcon.Icon.GetInitData(), _token);
        }
     
        ShowStats();
    }

    protected void UnequipItem(ItemIcon icon)
    {
        if (icon == null || icon.GetDataItem() == null || EquipmentIcons == null)
        {
            return;
        }

        TryUnequip(icon.GetDataItem());
    }

    protected virtual void TryUnequip(Item item)
    {

        UnequipItem uneq = new UnequipItem()
        {
            ItemId = item.Id,
        };

        _networkService.SendMapMessage(uneq);
    }

    protected void OnUnequip(OnUnequipItem unequipItem)
    {

        if (unequipItem.UnitId != _unit.Id)
        {
            return;
        }

        if (!_inventoryService.UnequipItem(_unit, unequipItem.ItemId, CalcStatsOnEquipUnequip()))
        {
            return;
        }

        EquipSlotIcon eqSlotIcon = null;

        foreach (EquipSlotIcon eqIcon in EquipmentIcons)
        {
            if (eqIcon != null && 
                eqIcon.Icon != null &&
                eqIcon.Icon.GetInitData() != null &&
                eqIcon.Icon.GetDataItem() != null &&
                eqIcon.Icon.GetDataItem().Id == unequipItem.ItemId)
            {
                eqSlotIcon = eqIcon;
                break;
            }
        }

        if (eqSlotIcon == null)
        {
            return;
        }


        Item oldItem = eqSlotIcon.Icon.GetDataItem();

        InitEquipmentIcon(eqSlotIcon);

        if (oldItem != null)
        {
            Items.InitIcon(oldItem, _token);
        }
        ShowStats();
    }

    protected override void ShowDragTargetIconsGlow(bool visible)
    {
        if (EquipmentIcons == null)
        {
            return;
        }

        List<EquipSlotIcon> toggleList = null;

        Item item = null;

        if (!visible)
        {
            toggleList = EquipmentIcons;
        }
        else
        {
            if (_dragItem == null || _dragItem.GetDataItem() == null)
            {
                return;
            }

            item = _dragItem.GetDataItem();


            ItemType itype = _gameData.Get<ItemTypeSettings>(_unit).Get(item.ItemTypeId);
            if (itype == null)
            {
                return;
            }

            List<long> allSlots = itype.GetCompatibleEquipSlots(_gameData, _unit);

            toggleList = new List<EquipSlotIcon>();

            foreach (EquipSlotIcon eqIcon in EquipmentIcons)
            {
                if (allSlots.Contains(eqIcon.EquipSlotId))
                {
                    toggleList.Add(eqIcon);
                }
            }
        }


        UnityEngine.Color color = GColor.white;

        if (visible)
        {
            color = GColor.yellow;

            if (item != null && !_inventoryService.CanEquipItem(_unit, item))
            {
                color = GColor.red;
            }
        }

        foreach (EquipSlotIcon icon in toggleList)
        {
            if (icon.Name != null)
            {
                icon.Name.color = color;
            }
        }

    }
    protected override void HandleDragDrop(ItemIconScreen screen, ItemIcon dragItem, ItemIcon otherIconHit, GEntity finalObjectHit)
    {
        if (dragItem != _dragItem)
        {
            return;
        }

        if (_dragItem == null || _origItem == null || _origItem.GetDataItem() == null)
        {
            return;
        }

        int equipSlotId = -1;

        if (otherIconHit != null)
        {
            EquipSlotIcon equipSlotIcon = otherIconHit.GetComponent<EquipSlotIcon>();
            if (equipSlotIcon != null)
            {
                equipSlotId = equipSlotIcon.EquipSlotId;
            }
        }


        if (equipSlotId < 1)
        {
            if (_origItem.GetDataItem().EquipSlotId < 1)
            {
                // Do nothing. No target, item is not currently equipped.
            }
            else
            {
                // Item is equipped, so unequip it.
                UnequipItem(_origItem);
            }
        }
        else // EquipSlot is > 0 so try to equip.
        {
            EquipItem(_origItem, equipSlotId);
        }

    }

}

