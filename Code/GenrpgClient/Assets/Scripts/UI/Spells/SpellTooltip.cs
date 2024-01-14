using System.Collections.Generic;
using GEntity = UnityEngine.GameObject;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.Spells.PlayerData.Spells;
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

    public GText SpellName;
    public GText BasicInfo;
    public GEntity RowParent;
    public GText MoneyText;
    public MoneyDisplay Money;

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

        _uiService.SetText(SpellName, _data.spell.Name);
        _uiService.SetText(BasicInfo, "");

        ShowEffects();
    }

    private void ShowEffects()
    {
        GEntityUtils.DestroyAllChildren(RowParent);
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

        _assetService.LoadAssetInto(gs, RowParent, AssetCategoryNames.UI, 
            SpellTooltipRow, OnLoadRow, data, _token, "Spells");
    }

    private void OnLoadRow(UnityGameState gs, string url, object obj, object data, CancellationToken token)
    {
        GEntity go = obj as GEntity;
        if (go == null)
        {
            return;
        }

        SpellTooltipRow row = go.GetComponent<SpellTooltipRow>();
        SpellTooltipRowData rowData = data as SpellTooltipRowData;
        if (row == null || rowData == null)
        {
            GEntityUtils.Destroy(go);
            return;
        }

        row.Init(gs, rowData);
        Rows.Add(row);
    }

    public void OnExit(string reason = "")
    {
        entity.SetActive(false);
    }
}