using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
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
    protected IInventoryService _inventoryService;
    protected IClientMapObjectManager _objectManager;
    class StatDownloadData
    {
        public Character currCh = null;
        public long statTypeId = 0;
    }
    [SerializeField]
    private Text _basicInfo;
    [SerializeField]
    private InventoryPanel _items;
    [SerializeField]
    private List<EquipSlotIcon> _equipmentIcons;
    [SerializeField]
    private List<StatInfoRow> _stats;
    [SerializeField]
    private GameObject _statGridParent;
    [SerializeField]
    private Button _closeButton;

    protected Character _ch = null;

    protected override async UniTask OnStartOpen(object data, CancellationToken token)
    {
        await base.OnStartOpen(data, token);
        UIHelper.SetButton(_closeButton, GetAnalyticsName(), StartClose);
        _gs.AddEvent<OnUnequipItem>(this, OnUnequip);
        _gs.AddEvent<OnEquipItem>(this, OnEquip);
        _ch = _gs.ch;
        InitEquipmentIcons();

        ShowStats();

        if (_items != null)
        {
            _items.Init(InventoryGroup.Equipment, this, null, token);
        }
    }

    private void InitEquipmentIcons()
    {
        if (_equipmentIcons == null || _ch == null)
        {
            return;
        }

        InventoryData inventory = _gs.ch.Get<InventoryData>();
        List<Item> allEquipment = inventory.GetAllEquipment();
        foreach (EquipSlotIcon icon in _equipmentIcons)
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
            GameObjectUtils.SetActive(eqIcon.gameObject, false);
        }

        InventoryData inventory = gs.ch.Get<InventoryData>();
        Item currentEquipment = inventory.GetEquipBySlot(eqIcon.EquipSlotId);
        InitItemIconData iconInitData = new InitItemIconData()
        {
            Data = currentEquipment,
            Screen = this,
        };
        eqIcon.Icon.Init(iconInitData, _token);
    }

    private void ShowStats()
    {
        if (_ch == null)
        {
            return;
        }

        if (_statGridParent == null)
        {
            return;
        }

        if (_stats == null)
        {
            _stats = new List<StatInfoRow>();
        }

        if (_stats.Count < 1)
        {
            List<StatType> allstats = _gs.data.GetGameData<StatSettings>().StatTypes;
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
                    _assetService.LoadAssetInto(_gs, _statGridParent, AssetCategory.UI, "StatInfoRow", OnDownloadStat, sddFill, _token);
                }
                StatDownloadData sdd = new StatDownloadData()
                {
                    currCh = _ch,
                    statTypeId = stat.IdKey,
                };
                _assetService.LoadAssetInto(_gs, _statGridParent, AssetCategory.UI, "StatInfoRow", OnDownloadStat, sdd, _token);
            }
        }
        else 
        {
            foreach (StatInfoRow row in _stats)
            {
                row.Init(_ch, -1, 0);
            }
        }
    }

    private void OnDownloadStat (UnityGameState gs, string url, object obj, object data, CancellationToken token)
    {
        GameObject go = obj as GameObject;
        if (go == null)
        {
            return;
        }

        StatDownloadData downloadData = data as StatDownloadData;

        if (downloadData == null || downloadData.currCh == null || downloadData.statTypeId < 1)
        {
            GameObject.Destroy(go);
            return;
        }


        StatInfoRow statRow = go.GetComponent<StatInfoRow>();
        if (statRow == null)
        {
            GameObject.Destroy(go);
            return;
        }
        if (_stats == null)
        {
            _stats = new List<StatInfoRow>();
        }

        _stats.Add(statRow);

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
        if (_equipmentIcons == null)
        {
            return null;
        }

        foreach (EquipSlotIcon icon in _equipmentIcons)
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

        ItemType itype = gs.data.GetGameData<ItemSettings>().GetItemType(icon.GetDataItem().ItemTypeId);
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

        List<long> equipSlots = itype.GetCompatibleEquipSlots(gs);
        List<long> relatedSlots = itype.GetRelatedEquipSlots(gs);

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

        _items.RemoveIcon(equip.Item.Id);

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
        

        ItemType itype = gs.data.GetGameData<ItemSettings>().GetItemType(equip.Item.ItemTypeId);

        List<long> equipSlots = itype.GetCompatibleEquipSlots(gs);
        List<long> relatedSlots = itype.GetRelatedEquipSlots(gs);

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
                GameObject.Destroy(newEqIcon.gameObject);
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

        foreach (EquipSlotIcon eqIcon in _equipmentIcons)
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
            _items.InitIcon(oldItem, _token);
        }
        ShowStats();
        return null;
    }

    protected void UnequipItem(UnityGameState gs, ItemIcon icon)
    {
        if (icon == null || icon.GetDataItem() == null || _equipmentIcons == null)
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
        if (_equipmentIcons == null)
        {
            return;
        }

        List<EquipSlotIcon> toggleList = null;

        if (!visible)
        {
            toggleList = _equipmentIcons;
        }
        else
        {
            if (_dragItem == null || _dragItem.GetDataItem() == null)
            {
                return;
            }

            Item item = _dragItem.GetDataItem();


            ItemType itype = _gs.data.GetGameData<ItemSettings>().GetItemType(item.ItemTypeId);
            if (itype == null)
            {
                return;
            }

            List<long> allSlots = itype.GetCompatibleEquipSlots(_gs);

            toggleList = new List<EquipSlotIcon>();

            foreach (EquipSlotIcon eqIcon in _equipmentIcons)
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
                icon.Name.color = (visible ? Color.yellow : Color.black);
            }
        }

    }
    protected override void HandleDragDrop(ItemIconScreen screen, ItemIcon dragItem, ItemIcon otherIconHit, GameObject finalObjectHit)
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

