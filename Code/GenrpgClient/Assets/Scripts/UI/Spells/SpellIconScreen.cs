using Genrpg.Shared.Spells.PlayerData.Spells;

using System.Threading;
using UnityEngine;

public class SpellIconScreen : DragItemScreen<Spell,SpellIcon,SpellIconScreen,InitSpellIconData>
{
    protected override async Awaitable OnStartOpen(object data, CancellationToken token)
    {
        await base.OnStartOpen(data, token);
    }

    protected virtual bool LoadSpellIconsOnLoad() { return true; }

}
