using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Genrpg.Shared.Core.Entities;

using Services;
using UnityEngine;
using UnityEngine.UI;
using Genrpg.Shared.Constants;
using Genrpg.Shared.Inventory.Entities;
using Entities;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.Spells.Entities;
using Genrpg.Shared.Stats.Entities;
using Assets.Scripts.Atlas.Constants;
using Genrpg.Shared.Entities.Utils;
using Newtonsoft.Json.Linq;
using System.Threading;

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

    [SerializeField] 
    private Text _message;
    [SerializeField] 
    private Text _name;
    [SerializeField] 
    private Text _basicInfo;
    [SerializeField] 
    private Image _rarityImage;
    [SerializeField] 
    private GameObject _rowParent;
    [SerializeField] 
    private Text _moneyText;
    [SerializeField] 
    private MoneyDisplay _money;

    protected List<ItemTooltipRow> Rows;


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

        UIHelper.SetText(_message, _data.message);
        UIHelper.SetText(_name, ItemUtils.GetName(gs,_data.mainItem));
        UIHelper.SetText(_basicInfo, ItemUtils.GetBasicInfo(gs, _data.mainItem));

        string bgName = IconHelper.GetBackingNameFromQuality(gs, _data.mainItem.QualityTypeId);

        _assetService.LoadSpriteInto(gs, AtlasNames.Icons, bgName, _rarityImage, token);

        ShowMoney();

        ShowEffects();
    }

    private void ShowEffects()
    {
        GameObjectUtils.DestroyAllChildren(_rowParent);
        Rows = new List<ItemTooltipRow>();

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
                    if (eff.EntityTypeId == EntityType.Stat || eff.EntityTypeId == EntityType.StatPct)
                    {
                        StatType stype = _gs.data.GetGameData<StatSettings>().GetStatType(eff.EntityId);
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
            string mainText = EntityUtils.PrintData(_gs, eff);

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

            string mainText = EntityUtils.PrintData(_gs, eff);
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

        _assetService.LoadAssetInto(gs, _rowParent, AssetCategory.UI, ItemTooltipRow, OnLoadRow, data, _token);
    }

    private void OnLoadRow(UnityGameState gs, string url, object obj, object data, CancellationToken token)
    {
        GameObject go = obj as GameObject;
        if (go == null)
        {
            return;
        }

        ItemTooltipRow row = go.GetComponent<ItemTooltipRow>();
        ItemTooltipRowData rowData = data as ItemTooltipRowData;       
        if(row == null || rowData == null)
        {
            GameObject.Destroy(go);
            return;
        }

        row.Init(gs, rowData);
        Rows.Add(row);
    }

    public void OnExit(string reason = "")
    {
        gameObject.SetActive(false);
    }

    public void ShowMoney()
    {
        if (_money != null && _data.mainItem != null)
        {
            long cost = 0;

            if (!_data.isVendorItem)
            {
                UIHelper.SetText(_moneyText, "Sell:");
                cost = ItemUtils.GetSellToVendorPrice(_gs,_data.mainItem);
            }
            else
            {
                UIHelper.SetText(_moneyText, "Price:");
                cost = ItemUtils.GetBuyFromVendorPrice(_gs, _data.mainItem);
            }

            _money.SetMoney(cost);
        }
    }
}