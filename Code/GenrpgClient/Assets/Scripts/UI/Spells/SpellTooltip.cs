using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Genrpg.Shared.Core.Entities;

using Services;
using UnityEngine;
using UnityEngine.UI;
using Entities;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.Spells.Entities;
using System.Threading;

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

    [SerializeField]
    private Text _name;
    [SerializeField]
    private Text _basicInfo;
    [SerializeField]
    private GameObject _rowParent;
    [SerializeField]
    private Text _moneyText;
    [SerializeField]
    private MoneyDisplay _money;

    protected List<SpellTooltipRow> Rows;

    protected InitSpellTooltipData _data;
    public override void Init(UnityGameState gs, InitTooltipData baseData, CancellationToken token)
    {
        base.Init(gs, baseData, token);
        _data = baseData as InitSpellTooltipData;
        if (_data == null || _data.spell == null)
        {
            OnExit("No Spell");
            return;
        }

        UIHelper.SetText(_name, _data.spell.Name);
        UIHelper.SetText(_basicInfo, "");

        ShowEffects();
    }

    private void ShowEffects()
    {
        GameObjectUtils.DestroyAllChildren(_rowParent);
        Rows = new List<SpellTooltipRow>();

        if (_data.spell == null)
        {
            return;
        }
    }

    private void ShowTooltipRow(UnityGameState gs, SpellTooltipRowData data)
    {
        if (data == null)
        {
            return;
        }

        _assetService.LoadAssetInto(gs, _rowParent, AssetCategory.UI, SpellTooltipRow, OnLoadRow, data, _token);
    }

    private void OnLoadRow(UnityGameState gs, string url, object obj, object data, CancellationToken token)
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
}