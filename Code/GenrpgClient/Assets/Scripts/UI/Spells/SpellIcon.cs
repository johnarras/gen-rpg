
using Genrpg.Shared.Inventory.Entities;
using Genrpg.Shared.Spells.Entities;
using Assets.Scripts.Atlas.Constants;
using System.Threading;

public delegate void OnLoadSpellIconHandler(UnityGameState gs, InitSpellIconData data);

public class InitSpellIconData : DragItemInitData<Spell,SpellIcon,SpellIconScreen,InitSpellIconData>
{ 
    public OnLoadSpellIconHandler handler;
    public SpellIcon createdIcon;
    public string iconPrefabName;
};

public class SpellIcon : DragItem<Spell, SpellIcon, SpellIconScreen, InitSpellIconData>
{
    public GImage Icon;

    public override void Init(InitSpellIconData data, CancellationToken token)
    {
        base.Init(data, token);
        if (data == null)
        {
            return;
        }

        data.createdIcon = this;
        _initData = data;

        string iconName = ItemConstants.BlankIconName;

        if (data.Data != null)
        {
            iconName = data.Data.Icon;
        }

        _assetService.LoadSpriteInto(_gs, AtlasNames.SkillIcons, iconName, Icon, _token);

    }

    public Spell GetSpell()
    {
        if (_initData != null)
        {
            return _initData.Data;
        }

        return null;
    }

    public long GetSpellId()
    {
        if (GetSpell() != null)
        {
            return GetSpell().IdKey;
        }

        return 0;
    }


    protected override bool ShowTooltipOnLeft()
    {
        return true;
    }

    public override void ShowTooltip()
    {
        if (_initData == null || _initData.Screen == null || _initData.Screen.ToolTip == null || 
            _initData.Data == null || _initData.Screen.GetDragItem() != null)
        {
            return;
        }

        GEntityUtils.SetActive(_initData.Screen.ToolTip, true);
        InitSpellTooltipData tooltipData = new InitSpellTooltipData() { spell = _initData.Data };
        _initData.Screen.ToolTip.Init(_gs, tooltipData, _token);
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
