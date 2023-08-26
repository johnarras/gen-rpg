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
using Genrpg.Shared.Spells.Entities;
using System.Threading;

public class SpellProcEdit : BaseBehaviour
{
    [SerializeField]
    private Text _name;
    [SerializeField]
    private Text _cost;
    [SerializeField]
    private SpellIcon _icon;
    [SerializeField]
    private SpellModDropdown _chance;
    [SerializeField]
    private SpellModDropdown _scale;

    public Spell Spell { get; set; }

    public SpellProc Proc { get; set; }

    public SpellbookScreen Screen { get; set; }

    private CancellationToken _token;
    public void Init(UnityGameState gs, SpellbookScreen screen, SpellProc currentProc, CancellationToken token)
    {
        _token = token;
        SpellModifier scaleMod = gs.data.GetGameData<SpellSettings>().GetSpellModifier(SpellModifier.Scale);
        SpellModifier chanceMod = gs.data.GetGameData<SpellSettings>().GetSpellModifier(SpellModifier.ProcChance);

        if (_chance != null)
        {
            _chance.Init(chanceMod, OnValueChanged);
        }

        if (_scale != null)
        {
            _scale.Init(chanceMod, OnValueChanged);
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
            UIHelper.SetText(_name, "");
            UIHelper.SetText(_cost, "");
        }
        else
        {
            UIHelper.SetText(_name, Spell.Name);
            UIHelper.SetText(_cost, Spell.Cost.ToString());

        }

        if (_icon != null)
        {
            InitSpellIconData iconData = new InitSpellIconData()
            {
                iconPrefabName = IconHelper.DefaultSpellIconName,
                Screen = Screen,
                Data = Spell,
            };
            _icon.Init(iconData, token);
        }
        

    }

    protected void OnValueChanged(int newValue)
    {
        //if (Screen != null) Screen.OnValueChanged(newValue);
    }
}