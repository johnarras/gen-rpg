using System.Collections.Generic;
using GEntity = UnityEngine.GameObject;
using Genrpg.Shared.Inventory.PlayerData;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.Utils;
using System.Threading;
using UnityEngine; // Needed
using Genrpg.Shared.Inventory.Settings;
using Genrpg.Shared.Inventory.Settings.ItemTypes;

public class FullItemTooltipInitData : InitTooltipData
{
    public Item item;
    public Unit unit;
    public ItemIconScreen screen;
    public int flags;
}

public class FullItemTooltip : BaseTooltip
{
    
    public ItemTooltip _mainTooltip;
    
    public List<ItemTooltip> _equipTooltips;

    
    public GEntity _equipParent;

    protected ItemIconScreen _screen;
    private Item _mainItem;
    private Unit _unit;

    
    public int _iconWidth = 32;

    protected List<Item> _equips;
    protected bool _isVendorItem;
    protected bool _tooltipOnRight;

    public override void Init(UnityGameState gs, InitTooltipData baseData, CancellationToken token)
    {
        base.Init (gs, baseData, token);
        FullItemTooltipInitData initData = baseData as FullItemTooltipInitData;
        if (initData == null)
        {
            return;
        }

        _isVendorItem = FlagUtils.IsSet(initData.flags, ItemIconFlags.IsVendorItem);
        _tooltipOnRight = FlagUtils.IsSet(initData.flags, ItemIconFlags.ShowTooltipOnRight);

        _mainItem = initData.item;
        _equips = new List<Item>();
        _screen = initData.screen;
        _unit = initData.unit;
        if (_mainItem == null || initData.unit == null)
        {
            OnExit("No item");
            return;
        }

        if (_mainTooltip == null || _equipTooltips== null || _equipTooltips.Count < 2 || _equipTooltips[0] == null || _equipTooltips[1] == null ||
            _equipParent == null)
        {
            OnExit("Missing Tooltip objects");
            return;
        }
        ItemType itype = gs.data.Get<ItemTypeSettings>(_unit).Get(_mainItem.ItemTypeId);

        if (itype != null)
        {
            if (itype.EquipSlotId > 0)
            {
                InventoryData inventory = _unit.Get<InventoryData>();
                List<long> compatibleSlots = itype.GetCompatibleEquipSlots(gs, _unit);
                for (int i = 0; i < compatibleSlots.Count; i++)
                {
                    Item equipItem = inventory.GetEquipBySlot(compatibleSlots[i]);
                    if (equipItem != null && equipItem != initData.item)
                    {
                        _equips.Add(equipItem);
                    }
                }
            }
                
        }

        InitItemTooltipData mainInitData = new InitItemTooltipData()
        {
            mainItem = _mainItem,
            mainItemType = itype,
            isVendorItem = _isVendorItem,
            message = "",
            compareToItem = null,
            unit = _unit,
        };


        _mainTooltip.Init(gs, mainInitData, _token);
        for (int i = 0; i < _equipTooltips.Count; i++)
        {
            GEntityUtils.SetActive(_equipTooltips[i], i < _equips.Count);
            if (i < _equips.Count)
            {
                InitItemTooltipData otherInitData = new InitItemTooltipData()
                {
                    mainItem = _equips[i],
                    isVendorItem = false,
                    compareToItem = _mainItem,
                    message = "Currently Equipped:",
                    unit = _unit,
                };

                _equipTooltips[i].Init(gs, otherInitData, _token);
            }
        }

        if (_iconWidth == 0)
        {
            RectTransform mrect = _mainTooltip.GetComponent<RectTransform>();
            if (mrect == null)
            {
                _iconWidth = 150;
            }
            else
            {
                _iconWidth = (int)mrect.rect.width + 5;
            }
        }



        float iconx = 0;
        float eqiconx = -_iconWidth;

        if (_tooltipOnRight)
        {
            iconx = eqiconx;
            eqiconx = 0;
        }

        GVector3 mpos = GVector3.Create(_mainTooltip.transform().localPosition);
        _mainTooltip.transform().localPosition = GVector3.Create(iconx, mpos.y, mpos.z);

        GVector3 epos = GVector3.Create(_equipParent.transform().localPosition);
        _equipParent.transform().localPosition = GVector3.Create(eqiconx, epos.y, epos.z);

    }

    public void OnExit(string msg = "")
    {
        GEntityUtils.SetActive(entity, false);
    }

}