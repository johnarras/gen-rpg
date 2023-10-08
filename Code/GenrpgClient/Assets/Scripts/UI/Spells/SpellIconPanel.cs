using System.Collections.Generic;
using GEntity = UnityEngine.GameObject;
using Genrpg.Shared.Spells.Entities;
using System.Threading;

public class SpellIconPanel : BaseBehaviour
{
    
    public GEntity _iconParent;

    protected SpellIconScreen _screen = null;
    protected string _prefabName = "";
    protected CancellationToken _token;
    public void Init(UnityGameState gs, SpellIconScreen screen, string prefabName, CancellationToken token)
    {
        _screen = screen;
        _prefabName = prefabName;
        _token = token;
        List<Spell> spells = gs.ch.Get<SpellData>().GetData();

        if (spells == null)
        {
            return;
        }

        GEntityUtils.DestroyAllChildren(_iconParent);

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