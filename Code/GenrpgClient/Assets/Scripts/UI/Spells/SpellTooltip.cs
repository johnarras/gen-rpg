using System.Collections.Generic;
using UnityEngine;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.Spells.PlayerData.Spells;
using System.Threading;
using Genrpg.Shared.Client.Assets.Constants;

public class InitSpellTooltipData : InitTooltipData
{
    public Spell spell;
}

public class SpellTooltipRowData
{
    public string text;
}


public class SpellTooltip : BaseTooltip
{
    public const string SpellTooltipRow = "SpellTooltipRow";

    public const int StarBaseAmount = 25;
    public const int StarIncrementAmount = 25;

    public GText SpellName;
    public GText BasicInfo;
    public GameObject RowParent;
    public GText MoneyText;
    public MoneyDisplay Money;

    protected List<SpellTooltipRow> Rows;

    protected InitSpellTooltipData _data;
    public override void Init(InitTooltipData baseData, CancellationToken token)
    {
        base.Init(baseData, token);
        _data = baseData as InitSpellTooltipData;
        if (_data == null || _data.spell == null)
        {
            OnExit("No Spell");
            return;
        }

        _uiService.SetText(SpellName, _data.spell.Name);
        _uiService.SetText(BasicInfo, "");

        ShowEffects();
    }

    private void ShowEffects()
    {
        _clientEntityService.DestroyAllChildren(RowParent);
        Rows = new List<SpellTooltipRow>();

        if (_data.spell == null)
        {
            return;
        }
    }

    private void ShowTooltipRow(SpellTooltipRowData data)
    {
        if (data == null)
        {
            return;
        }

        _assetService.LoadAssetInto(RowParent, AssetCategoryNames.UI, 
            SpellTooltipRow, OnLoadRow, data, _token, "Spells");
    }

    private void OnLoadRow(object obj, object data, CancellationToken token)
    {
        GameObject go = obj as GameObject;
        if (go == null)
        {
            return;
        }

        SpellTooltipRow row = go.GetComponent<SpellTooltipRow>();
        SpellTooltipRowData rowData = data as SpellTooltipRowData;
        if (row == null || rowData == null)
        {
            _clientEntityService.Destroy(go);
            return;
        }

        row.Init(rowData);
        Rows.Add(row);
    }

    public void OnExit(string reason = "")
    {
        entity.SetActive(false);
    }
}