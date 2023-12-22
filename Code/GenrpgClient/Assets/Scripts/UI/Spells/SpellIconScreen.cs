using Genrpg.Shared.Spells.PlayerData.Spells;
using Cysharp.Threading.Tasks;
using System.Threading;

public class SpellIconScreen : DragItemScreen<Spell,SpellIcon,SpellIconScreen,InitSpellIconData>
{
    protected override async UniTask OnStartOpen(object data, CancellationToken token)
    {
        await base.OnStartOpen(data, token);
    }

    protected virtual bool LoadSpellIconsOnLoad() { return true; }

}
