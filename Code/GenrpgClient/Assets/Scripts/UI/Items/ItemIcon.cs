
using Genrpg.Shared.Inventory.PlayerData;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Utils;
using Assets.Scripts.Atlas.Constants;
using Genrpg.Shared.Entities.Services;
using System.Threading;
using Genrpg.Shared.Inventory.Settings;
using Genrpg.Shared.Inventory.Constants;
using Genrpg.Shared.Inventory.Utils;
using Genrpg.Shared.Inventory.Settings.ItemTypes;

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

    public GImage Background;
    public GImage Frame;
    public GImage Icon;
    public GText QuantityText;
    
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
            iconName = ItemUtils.GetIcon(_gs, _gs.ch, _initData.Data);
        }
        else
        {
            IIndexedGameItem dataObject = _entityService.Find(_gs, _gs.ch, data.entityTypeId, data.entityId);
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

        _assetService.LoadSpriteInto(_gs, AtlasNames.Icons, bgName, Background, token);
        _assetService.LoadSpriteInto(_gs, AtlasNames.Icons, frameName, Frame, token);
        _assetService.LoadSpriteInto(_gs, AtlasNames.Icons, iconName, Icon, token);

        if (_initData.Data != null)
        {
            ItemType itype = _gs.data.GetGameData<ItemTypeSettings>(_gs.ch).GetItemType(_initData.Data.ItemTypeId);
            if (itype.EquipSlotId > 0)
            {
                _uiService.SetText(QuantityText, "");
            }
            else
            {
                _uiService.SetText(QuantityText, _initData.Data.Quantity.ToString());
            }
        }
        else
        {
            _uiService.SetText(QuantityText, data.quantity.ToString());
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

        GEntityUtils.SetActive(_initData.Screen.ToolTip, true);
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

        GEntityUtils.SetActive(_initData.Screen.ToolTip, false);
    }



}
