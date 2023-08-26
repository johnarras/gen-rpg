using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Genrpg.Shared.Core.Entities;

using Services;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Genrpg.Shared.Constants;
using Genrpg.Shared.Inventory.Entities;
using Entities;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Utils;
using Assets.Scripts.Atlas.Constants;
using Genrpg.Shared.Entities.Services;
using System.Threading;

public delegate void OnLoadItemIconHandler(UnityGameState gs, InitItemIconData data);

public class ItemIconFlags
{
    public const int IsVendorItem = (1 << 0);
    public const int ShowTooltipNow = (1 << 1);
    public const int ShowTooltipOnRight = (1 << 2);
    public const int NoDrag = (1 << 3);
}

public class InitItemIconData : DragItemInitData<Item,ItemIcon,ItemIconScreen,InitItemIconData>
{
    public long entityTypeId;
    public long entityId;
    public long quantity;
    public long level;
    public long quality;
    public ItemType itemType;

    public ItemIcon createdIcon;

    public OnLoadItemIconHandler handler;

    public string iconPrefabName;
};


public class ItemIcon : DragItem<Item,ItemIcon,ItemIconScreen,InitItemIconData>
{

    protected IEntityService _entityService;

    [SerializeField] 
    protected Image _background;
    [SerializeField] 
    protected Image _frame;
    [SerializeField] 
    protected Image _icon;
    [SerializeField] 
    protected Text _quantityText;

    
    public override void Init(InitItemIconData data, CancellationToken token)
    {
        base.Init(data, token);
        if (data == null)
        {
            return;
        }

        data.createdIcon = this;
        _initData = data;


        string bgName = IconHelper.GetBackingNameFromQuality(_gs, 0);
        string frameName = IconHelper.GetFrameNameFromLevel(_gs, 1);

        string iconName = ItemConstants.BlankIconName;

        if (_initData.Data != null)
        {
            frameName = IconHelper.GetFrameNameFromLevel(_gs, _initData.Data.Level);
            bgName = IconHelper.GetBackingNameFromQuality(_gs, _initData.Data.QualityTypeId);
            iconName = ItemUtils.GetIcon(_gs,_initData.Data);
        }
        else
        {
            IIndexedGameItem dataObject = _entityService.Find(_gs, data.entityTypeId, data.entityId);
            if (dataObject != null && !string.IsNullOrEmpty(dataObject.Icon))
            {
                iconName = dataObject.Icon;
            }
            if (data.quality > 0)
            {
                bgName = IconHelper.GetBackingNameFromQuality(_gs, data.quality);
            }

            if (data.level > 0)
            {
                frameName = IconHelper.GetFrameNameFromLevel(_gs, data.level);
            }
        }

        _assetService.LoadSpriteInto(_gs, AtlasNames.Icons, bgName, _background, token);
        _assetService.LoadSpriteInto(_gs, AtlasNames.Icons, frameName, _frame, token);
        _assetService.LoadSpriteInto(_gs, AtlasNames.Icons, iconName, _icon, token);

        if (_initData.Data != null)
        {
            ItemType itype = _gs.data.GetGameData<ItemSettings>().GetItemType(_initData.Data.ItemTypeId);
            if (itype.EquipSlotId > 0)
            {
                UIHelper.SetText(_quantityText, "");
            }
            else
            {
                UIHelper.SetText(_quantityText, _initData.Data.Quantity.ToString());
            }
        }
        else
        {
            UIHelper.SetText(_quantityText, data.quantity.ToString());
        }

        if (FlagUtils.IsSet(_initData.Flags, ItemIconFlags.ShowTooltipNow))
        {
            ShowTooltip();
            _initData.Flags &= ~ItemIconFlags.ShowTooltipNow;
        }

        if (FlagUtils.IsSet(_initData.Flags, ItemIconFlags.NoDrag))
        {
            _canDrag = false;
        }

        data.handler?.Invoke(_gs, data);
    }


    public override void ShowTooltip()
    {
        if (_initData == null || _initData.Screen == null || _initData.Screen.ToolTip == null || _initData.Data == null ||
            _initData.Screen.GetDragItem() != null)
        {
            return;
        }

        GameObjectUtils.SetActive(_initData.Screen.ToolTip, true);
        FullItemTooltipInitData fullTooltipInitData = new FullItemTooltipInitData()
        {
            unit = _gs.ch,
            screen = _initData.Screen,
            item = _initData.Data,
            flags = _initData.Flags,
        };
        _initData.Screen.ToolTip.Init(_gs, fullTooltipInitData, _token);
        UpdateTooltipPosition();
    }

    public override void HideTooltip()
    {
        if (_initData == null || _initData.Screen == null || _initData.Screen.ToolTip == null)
        {
            return;
        }

        GameObjectUtils.SetActive(_initData.Screen.ToolTip, false);
    }



}
