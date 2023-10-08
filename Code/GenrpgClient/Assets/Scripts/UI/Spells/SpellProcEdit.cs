using Genrpg.Shared.Spells.Entities;
using System.Threading;

public class SpellProcEdit : BaseBehaviour
{
    public GText ProcName;
    public GText Cost;
    public SpellIcon Icon;
    public SpellModDropdown Chance;
    public SpellModDropdown Scale;

    public Spell Spell { get; set; }

    public SpellProc Proc { get; set; }

    public SpellbookScreen Screen { get; set; }

    private CancellationToken _token;
    public void Init(UnityGameState gs, SpellbookScreen screen, SpellProc currentProc, CancellationToken token)
    {
        _token = token;
        SpellModifier scaleMod = gs.data.GetGameData<SpellModifierSettings>(gs.ch).GetSpellModifier(SpellModifier.Scale);
        SpellModifier chanceMod = gs.data.GetGameData<SpellModifierSettings>(gs.ch).GetSpellModifier(SpellModifier.ProcChance);

        if (Chance != null)
        {
            Chance.Init(chanceMod, OnValueChanged);
        }

        if (Scale != null)
        {
            Scale.Init(chanceMod, OnValueChanged);
        }

        if (currentProc == null)
        {
            currentProc = new SpellProc();
        }

        Proc = currentProc;

        Screen = screen;

        Spell = gs.ch.Get<SpellData>().Get(Proc.SpellId);

        if (Spell == null)
        {
            UIHelper.SetText(ProcName, "");
            UIHelper.SetText(Cost, "");
        }
        else
        {
            UIHelper.SetText(ProcName, Spell.Name);
            UIHelper.SetText(Cost, Spell.Cost.ToString());

        }

        if (Icon != null)
        {
            InitSpellIconData iconData = new InitSpellIconData()
            {
                iconPrefabName = IconHelper.DefaultSpellIconName,
                Screen = Screen,
                Data = Spell,
            };
            Icon.Init(iconData, token);
        }
        

    }

    protected void OnValueChanged(int newValue)
    {
        //if (Screen != null) Screen.OnValueChanged(newValue);
    }
}