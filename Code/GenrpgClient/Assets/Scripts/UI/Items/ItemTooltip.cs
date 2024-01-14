using System.Collections.Generic;
using System.Linq;
using GEntity = UnityEngine.GameObject;
using Genrpg.Shared.Inventory.PlayerData;
using Genrpg.Shared.Entities.Constants;
using Assets.Scripts.Atlas.Constants;
using Genrpg.Shared.Entities.Utils;
using System.Threading;
using Genrpg.Shared.Inventory.Utils;
using Genrpg.Shared.Inventory.Settings.ItemTypes;
using Genrpg.Shared.Stats.Settings.Stats;

public class InitItemTooltipData : InitTooltipData
{
    public Item mainItem;
    public ItemType mainItemType;
    public bool isVendorItem;
    public Item compareToItem;
    public string message;
}

public class ItemTooltipRowData
{
    public string text;
    public bool isCurrent;
    public long change;
    public int starsToShow;
}


public class ItemTooltip : BaseTooltip
{
    public const string ItemTooltipRow = "ItemTooltipRow";

    public const int StarBaseAmount = 25;
    public const int StarIncrementAmount = 25;

     
    public GText Message;
    public GText ItemName;
    public GText BasicInfo;
    public GImage RarityImage;
    public GEntity RowParent;
    public GText MoneyText;
    public MoneyDisplay Money;

    protected List<ItemTooltipRow> _rows;

    protected InitItemTooltipData _data;
    public override void Init(UnityGameState gs, InitTooltipData baseData, CancellationToken token)
    {
        base.Init(gs, baseData, token);
        InitItemTooltipData data = baseData as InitItemTooltipData;
        _data = data;
        if (_data == null || _data.mainItem == null)
        {
            OnExit("No item");
            return;
        }

        _uiService.SetText(Message, _data.message);
        _uiService.SetText(ItemName, ItemUtils.GetName(gs, gs.ch,_data.mainItem));
        _uiService.SetText(BasicInfo, ItemUtils.GetBasicInfo(gs, gs.ch, _data.mainItem));

        string bgName = IconHelper.GetBackingNameFromQuality(gs, _data.mainItem.QualityTypeId);

        _assetService.LoadSpriteInto(gs, AtlasNames.Icons, bgName, RarityImage, token);

        ShowMoney();

        ShowEffects();
    }

    private void ShowEffects()
    {
        GEntityUtils.DestroyAllChildren(RowParent);
        _rows = new List<ItemTooltipRow>();

        List<ItemEffect> otherEffects = new List<ItemEffect>();
        if (_data.compareToItem != null && _data.compareToItem.Effects != null)
        {
            otherEffects = _data.compareToItem.Effects;
        }

        if (_data.mainItem == null)
        {
            return;
        }

        if (_data.mainItem.Effects == null || _data.mainItem.Effects.Count < 1)
        {
            if (_data.mainItemType != null && _data.mainItemType.Effects != null)
            {
                foreach (ItemEffect eff in _data.mainItemType.Effects)
                {
                    if (eff.EntityTypeId == EntityTypes.Stat || eff.EntityTypeId == EntityTypes.StatPct)
                    {
                        StatType stype = _gs.data.GetGameData<StatSettings>(_gs.ch).GetStatType(eff.EntityId);
                        if (stype == null)
                        {
                            continue;
                        }

                        int starsToShow = (int)((eff.Quantity - StarBaseAmount) / StarIncrementAmount + 1);

                        ItemTooltipRowData rowData = new ItemTooltipRowData()
                        {
                            text = stype.Name,
                            isCurrent = false,
                            change = 0,
                            starsToShow = starsToShow,
                        };
                        ShowTooltipRow(_gs, rowData);

                    }
                }
            }
           
        }

        foreach (ItemEffect eff in _data.mainItem.Effects)
        {
            string mainText = EntityUtils.PrintData(_gs, _gs.ch, eff);

            if (string.IsNullOrEmpty(mainText))
            {
                continue;
            }

            long change = (_data.compareToItem != null ? -eff.Quantity : 0);
            ItemEffect otherEffect = otherEffects.FirstOrDefault(x => x.EntityTypeId == eff.EntityTypeId && x.EntityId == eff.EntityId);
            
            if (otherEffect != null)
            {
                change = otherEffect.Quantity - eff.Quantity;
            }

            ItemTooltipRowData rowData = new ItemTooltipRowData()
            {
                text = mainText,
                isCurrent = true,
                change = change,
                starsToShow = 0,
            };
            ShowTooltipRow(_gs, rowData);
        }

        foreach (ItemEffect eff in otherEffects)
        {
            ItemEffect mainEffect = _data.mainItem.Effects.FirstOrDefault(x => x.EntityTypeId == eff.EntityTypeId &&
            x.EntityId == eff.EntityId);
            if (mainEffect != null)
            {
                continue;
            }

            string mainText = EntityUtils.PrintData(_gs, _gs.ch, eff);
            long change = eff.Quantity;
            ItemTooltipRowData rowData = new ItemTooltipRowData()
            {
                text = mainText,
                isCurrent = false,
                change = change,
                starsToShow = 0,
            };
            ShowTooltipRow(_gs, rowData);
        }
    }

    private void ShowTooltipRow(UnityGameState gs, ItemTooltipRowData data)
    {
        if (data == null)
        {
            return;
        }

        _assetService.LoadAssetInto(gs, RowParent, AssetCategoryNames.UI, 
            ItemTooltipRow, OnLoadRow, data, _token, "Items" );
    }

    private void OnLoadRow(UnityGameState gs, string url, object obj, object data, CancellationToken token)
    {
        GEntity go = obj as GEntity;
        if (go == null)
        {
            return;
        }

        ItemTooltipRow row = go.GetComponent<ItemTooltipRow>();
        ItemTooltipRowData rowData = data as ItemTooltipRowData;       
        if(row == null || rowData == null)
        {
            GEntityUtils.Destroy(go);
            return;
        }

        row.Init(gs, rowData);
        _rows.Add(row);
    }

    public void OnExit(string reason = "")
    {
        entity.SetActive(false);
    }

    public void ShowMoney()
    {
        if (Money != null && _data.mainItem != null)
        {
            long cost = 0;

            if (!_data.isVendorItem)
            {
                _uiService.SetText(MoneyText, "Sell:");
                cost = ItemUtils.GetSellToVendorPrice(_gs, _gs.ch, _data.mainItem);
            }
            else
            {
                _uiService.SetText(MoneyText, "Price:");
                cost = ItemUtils.GetBuyFromVendorPrice(_gs, _gs.ch, _data.mainItem);
            }

            Money.SetMoney(cost);
        }
    }
}