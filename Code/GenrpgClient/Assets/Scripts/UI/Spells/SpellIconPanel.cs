using System.Collections.Generic;
using UnityEngine;
using Genrpg.Shared.Spells.PlayerData.Spells;
using System.Threading;

public class SpellIconPanel : BaseBehaviour
{

    protected IIconService _iconService;
    public GameObject _iconParent;

    protected SpellIconScreen _screen = null;
    protected string _prefabName = "";
    protected CancellationToken _token;
    public void Init(SpellIconScreen screen, string prefabName, CancellationToken token)
    {
        _screen = screen;
        _prefabName = prefabName;
        _token = token;
        IReadOnlyList<Spell> spells = _gs.ch.Get<SpellData>().GetData();

        if (spells == null)
        {
            return;
        }

        _clientEntityService.DestroyAllChildren(_iconParent);

        foreach (Spell spell in spells)
        {
            InitIcon(spell, token);
        }
    }

    public void InitIcon(Spell stype, CancellationToken token)
    {
        InitSpellIconData idata = new InitSpellIconData()
        {
            Data = stype,
            Screen = _screen,
            iconPrefabName = _prefabName,
        };
        _iconService.InitSpellIcon(idata, _iconParent,_assetService, token);
    }

}