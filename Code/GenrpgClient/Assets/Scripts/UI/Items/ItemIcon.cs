
using Genrpg.Shared.Inventory.PlayerData;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Entities.Services;
using System.Threading;
using Genrpg.Shared.Inventory.Constants;
using Genrpg.Shared.Inventory.Settings.ItemTypes;
using Genrpg.Shared.Client.Assets.Constants;
using Genrpg.Shared.Inventory.Services;


public delegate void OnLoadItemIconHandler(InitItemIconData data);

public class ItemIconFlags
{
    public const int IsVendorItem = (1 << 0);
    public const int ShowTooltipNow = (1 << 1);
    public const int ShowTooltipOnRight = (1 << 2);
    public const int NoDrag = (1 << 3);
}

public class InitItemIconData : DragItemInitData<Item, ItemIcon, ItemIconScreen, InitItemIconData>
{
    public long EntityTypeId;
    public long EntityId;
    public long Quantity;
    public long Level;
    public long Quality;
    public ItemType ItemType;

    public ItemIcon CreatedItem;

    public OnLoadItemIconHandler Handler;

    public string IconPrefabName;

    public string SubDirectory = "Items";
};


public class ItemIcon : DragItem<Item,ItemIcon,ItemIconScreen,InitItemIconData>
{

    protected ISharedItemService _sharedItemService;
    protected IEntityService _entityService;
    protected IIconService _iconService;

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

        data.CreatedItem = this;
        _initData = data;

        string bgName = _iconService.GetBackingNameFromQuality(_gameData, 0);
        string frameName = _iconService.GetFrameNameFromLevel(_gameData, 1);

        string iconName = ItemConstants.BlankIconName;

        if (_initData.Data != null)
        {
            frameName = _iconService.GetFrameNameFromLevel(_gameData, _initData.Data.Level);
            bgName = _iconService.GetBackingNameFromQuality(_gameData, _initData.Data.QualityTypeId);
            iconName = _sharedItemService.GetIcon(_gameData, _gs.ch, _initData.Data);
        }
        else
        {
            IIndexedGameItem dataObject = (IIndexedGameItem)_entityService.Find(_gs.ch, data.EntityTypeId, data.EntityId);
            if (dataObject != null && !string.IsNullOrEmpty(dataObject.Icon))
            {
                iconName = dataObject.Icon;
            }
            if (data.Quality > 0)
            {
                bgName = _iconService.GetBackingNameFromQuality(_gameData, data.Quality);
            }

            if (data.Level > 0)
            {
                frameName = _iconService.GetFrameNameFromLevel(_gameData, data.Level);
            }
        }

        _assetService.LoadAtlasSpriteInto(AtlasNames.Icons, bgName, Background, token);
        _assetService.LoadAtlasSpriteInto(AtlasNames.Icons, frameName, Frame, token);
        _assetService.LoadAtlasSpriteInto(AtlasNames.Icons, iconName, Icon, token);

        if (_initData.Data != null)
        {
            ItemType itype = _gameData.Get<ItemTypeSettings>(_gs.ch).Get(_initData.Data.ItemTypeId);
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
            _uiService.SetText(QuantityText, data.Quantity.ToString());
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

        data.Handler?.Invoke(data);
    }


    public override void ShowTooltip()
    {
        if (_initData == null || _initData.Screen == null || _initData.Screen.ToolTip == null || _initData.Data == null ||
            _initData.Screen.GetDragItem() != null)
        {
            return;
        }

        _gameObjectService.SetActive(_initData.Screen.ToolTip, true);
        FullItemTooltipInitData fullTooltipInitData = new FullItemTooltipInitData()
        {
            unit = _initData.Screen.GetUnit(),
            screen = _initData.Screen,
            item = _initData.Data,
            flags = _initData.Flags,
        };
        _initData.Screen.ToolTip.Init(fullTooltipInitData, _token);
        UpdateTooltipPosition();
    }

    public override void HideTooltip()
    {
        if (_initData == null || _initData.Screen == null || _initData.Screen.ToolTip == null)
        {
            return;
        }
        _gameObjectService.SetActive(_initData.Screen.ToolTip, false);
    }
}
