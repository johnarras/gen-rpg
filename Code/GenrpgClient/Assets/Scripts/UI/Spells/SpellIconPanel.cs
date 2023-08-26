using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Genrpg.Shared.Core.Entities;

using Services;
using UnityEngine;
using Genrpg.Shared.Characters.Entities;
using Entities;
using Genrpg.Shared.Spells.Entities;
using System.Threading;

public class SpellIconPanel : BaseBehaviour
{
    [SerializeField]
    private GameObject _iconParent;

    protected SpellIconScreen _screen = null;
    protected string _prefabName = "";
    protected CancellationToken _token;
    public void Init(UnityGameState gs, SpellIconScreen screen, string prefabName, CancellationToken token)
    {
        _screen = screen;
        _prefabName = prefabName;
        _token = token;
        List<Spell> spells = gs.ch.Get<SpellData>().GetAll();

        if (spells == null)
        {
            return;
        }

        GameObjectUtils.DestroyAllChildren(_iconParent);

        foreach (Spell spell in spells)
        {
            InitIcon(gs, spell, token);
        }
    }

    public void InitIcon(UnityGameState gs, Spell stype, CancellationToken token)
    {
        InitSpellIconData idata = new InitSpellIconData()
        {
            Data = stype,
            Screen = _screen,
            iconPrefabName = _prefabName,
        };
        IconHelper.InitSpellIcon(gs,idata, _iconParent,_assetService, token);
    }

}