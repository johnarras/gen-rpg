using System.Collections.Generic;
using GEntity = UnityEngine.GameObject;
using Genrpg.Shared.Characters.Entities;
using Genrpg.Shared.Inventory.Entities;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.Stats.Entities;
using Genrpg.Shared.Inventory.Services;
using Cysharp.Threading.Tasks;
using System.Threading;
using Genrpg.Shared.Inventory.Messages;

public class CharacterScreen : ItemIconScreen
{
    public GText BasicInfo;
    public InventoryPanel Items;
    public List<EquipSlotIcon> EquipmentIcons;
    public List<StatInfoRow> Stats;
    public GEntity StatGridParent;
    public GButton CloseButton;

    protected IInventoryService _inventoryService;
    protected IClientMapObjectManager _objectManager;
    class StatDownloadData
    {
        public Character currCh = null;
        public long statTypeId = 0;
    }
    
    protected Character _ch = null;

    protected override async UniTask OnStartOpen(object data, CancellationToken token)
    {
        await base.OnStartOpen(data, token);
        UIHelper.SetButton(CloseButton, GetAnalyticsName(), StartClose);
        _gs.AddEvent<OnUnequipItem>(this, OnUnequip);
        _gs.AddEvent<OnEquipItem>(this, OnEquip);
        _ch = _gs.ch;
        InitEquipmentIcons();

        ShowStats();

        if (Items != null)
        {
            Items.Init(InventoryGroup.Equipment, this, null, token);
        }
    }

    private void InitEquipmentIcons()
    {
        if (EquipmentIcons == null || _ch == null)
        {
            return;
        }

        InventoryData inventory = _gs.ch.Get<InventoryData>();
        List<Item> allEquipment = inventory.GetAllEquipment();
        foreach (EquipSlotIcon icon in EquipmentIcons)
        {
            InitEquipmentIcon(_gs, icon);
        }
    }

    public void InitEquipmentIcon(UnityGameState gs, EquipSlotIcon eqIcon)
    {
        if (eqIcon == null || _ch == null)
        {
            return;
        }

        if (eqIcon.Icon == null)
        {
            GEntityUtils.SetActive(eqIcon.entity(), false);
        }

        InventoryData inventory = gs.ch.Get<InventoryData>();
        Item currentEquipment = inventory.GetEquipBySlot(eqIcon.EquipSlotId);
        InitItemIconData iconInitData = new InitItemIconData()
        {
            Data = currentEquipment,
            Screen = this,
        };
        EquipSlot slot = gs.data.GetGameData<EquipSlotSettings>(gs.ch).GetEquipSlot(eqIcon.EquipSlotId);
        eqIcon.Icon.Init(iconInitData, _token);
        UIHelper.SetText(eqIcon.Name, slot?.Name ?? "");
    }

    private void ShowStats()
    {
        if (_ch == null)
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
            List<StatType> allstats = _gs.data.GetGameData<StatSettings>(_gs.ch).GetData();
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

                if (prevStat > 0 && prevStat/10 != stat.IdKey /10)
                {
                    StatDownloadData sddFill = new StatDownloadData()
                    {
                        currCh = _ch,
                        statTypeId = -1,
                    };
                    _assetService.LoadAssetInto(_gs, StatGridParent, AssetCategoryNames.UI, "StatInfoRow", OnDownloadStat, sddFill, _token);
                }
                StatDownloadData sdd = new StatDownloadData()
                {
                    currCh = _ch,
                    statTypeId = stat.IdKey,
                };
                _assetService.LoadAssetInto(_gs, StatGridParent, AssetCategoryNames.UI, "StatInfoRow", OnDownloadStat, sdd, _token);
            }
        }
        else 
        {
            foreach (StatInfoRow row in Stats)
            {
                row.Init(_ch, -1, 0);
            }
        }
    }

    private void OnDownloadStat (UnityGameState gs, string url, object obj, object data, CancellationToken token)
    {
        GEntity go = obj as GEntity;
        if (go == null)
        {
            return;
        }

        StatDownloadData downloadData = data as StatDownloadData;

        if (downloadData == null || downloadData.currCh == null || downloadData.statTypeId < 1)
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

        statRow.Init(downloadData.currCh, downloadData.statTypeId, 0);

    }

    // Blank
    public override void OnLeftClickIcon(UnityGameState gs, ItemIcon icon) { }


    // Equip or Unequip item.
    public override void OnRightClickIcon(UnityGameState gs, ItemIcon icon)
    {
        if (icon == null || icon.GetDataItem() == null)
        {
            return;
        }

    }


    public EquipSlotIcon GetIconFromSlot(UnityGameState gs, long equipSlotId)
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

    protected void EquipItem(UnityGameState gs, ItemIcon icon, long newEquipSlotId)
    {

        if (icon == null || icon.GetDataItem()== null)
        {
            return;
        }

        ItemType itype = gs.data.GetGameData<ItemTypeSettings>(gs.ch).GetItemType(icon.GetDataItem().ItemTypeId);
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

        List<long> equipSlots = itype.GetCompatibleEquipSlots(gs, gs.ch);
        List<long> relatedSlots = itype.GetRelatedEquipSlots(gs, gs.ch);

        Dictionary<long, Item> _currentRelatedItems = new Dictionary<long, Item>();

        InventoryData inventory = gs.ch.Get<InventoryData>();
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

        EquipItem equip = new EquipItem()
        {
            ItemId = icon.GetDataItem().Id,
            EquipSlot = newEquipSlotId,
        };

        _networkService.SendMapMessage(equip);
    }

    protected OnEquipItem OnEquip(UnityGameState gs, OnEquipItem equip)
    {
        if (equip.Item == null)
        {
            return null;
        }

        Items.RemoveIcon(equip.Item.Id);

        InventoryData inventory = gs.ch.Get<InventoryData>();

        Item origItem = inventory.GetItem(equip.Item.Id);

        if (origItem == null)
        {
            return null;
        }

        if (origItem.EquipSlotId == equip.Item.EquipSlotId)
        {
            return null;
        }

        long currEquipSlotId = origItem.EquipSlotId;
        long newEquipSlotId = equip.Item.EquipSlotId;
        

        ItemType itype = gs.data.GetGameData<ItemTypeSettings>(gs.ch).GetItemType(equip.Item.ItemTypeId);

        List<long> equipSlots = itype.GetCompatibleEquipSlots(gs, gs.ch);
        List<long> relatedSlots = itype.GetRelatedEquipSlots(gs, gs.ch);

        EquipSlotIcon currEqIcon = GetIconFromSlot(gs, currEquipSlotId);
        EquipSlotIcon newEqIcon = GetIconFromSlot(gs, newEquipSlotId);


        Item existingItemInNewSlot = inventory.GetEquipBySlot(newEquipSlotId);

        Item existingEquipmentInNewSlot = inventory.GetEquipBySlot(newEquipSlotId);

        if (!_inventoryService.EquipItem(gs, gs.ch, equip.Item.Id, newEquipSlotId))
        {
            return null;
        }

        InitEquipmentIcon(gs, GetIconFromSlot(gs, newEquipSlotId));

        InitEquipmentIcon(gs, GetIconFromSlot(gs, currEquipSlotId));

        foreach (long relSlot in relatedSlots)
        {
            if (relSlot != newEquipSlotId && relSlot != currEquipSlotId)
            {
                InitEquipmentIcon(gs, GetIconFromSlot(gs, relSlot));
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

        return null;
    }

    protected OnUnequipItem OnUnequip(UnityGameState gs, OnUnequipItem unequipItem)
    {
        if (!_objectManager.GetChar(unequipItem.UnitId, out Character ch))
        {
            return null;
        }
        if (!_inventoryService.UnequipItem(gs, ch, unequipItem.ItemId, true))
        {
            return null;
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
            return null;
        }


        Item oldItem = eqSlotIcon.Icon.GetDataItem();

        InitEquipmentIcon(gs, eqSlotIcon);

        if (oldItem != null)
        {
            Items.InitIcon(oldItem, _token);
        }
        ShowStats();
        return null;
    }

    protected void UnequipItem(UnityGameState gs, ItemIcon icon)
    {
        if (icon == null || icon.GetDataItem() == null || EquipmentIcons == null)
        {
            return;
        }
        UnequipItem uneq = new UnequipItem()
        {
            ItemId = icon.GetDataItem().Id,
        };

        _networkService.SendMapMessage(uneq);
    }

    protected override void ShowDragTargetIconsGlow(bool visible)
    {
        if (EquipmentIcons == null)
        {
            return;
        }

        List<EquipSlotIcon> toggleList = null;

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

            Item item = _dragItem.GetDataItem();


            ItemType itype = _gs.data.GetGameData<ItemTypeSettings>(_gs.ch).GetItemType(item.ItemTypeId);
            if (itype == null)
            {
                return;
            }

            List<long> allSlots = itype.GetCompatibleEquipSlots(_gs, _gs.ch);

            toggleList = new List<EquipSlotIcon>();

            foreach (EquipSlotIcon eqIcon in EquipmentIcons)
            {
                if (allSlots.Contains(eqIcon.EquipSlotId))
                {
                    toggleList.Add(eqIcon);
                }
            }
        }

        foreach (EquipSlotIcon icon in toggleList)
        {
            if (icon.Name != null)
            {
                icon.Name.color = (visible ? GColor.yellow : GColor.black);
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
                UnequipItem(_gs, _origItem);
            }
        }
        else // EquipSlot is > 0 so try to equip.
        {
            EquipItem(_gs, _origItem, equipSlotId);
        }

    }

}

