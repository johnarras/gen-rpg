using Genrpg.Shared.Spells.PlayerData.Spells;

using System.Threading;
using System.Threading.Tasks;

public class SpellIconScreen : DragItemScreen<Spell,SpellIcon,SpellIconScreen,InitSpellIconData>
{
    protected override async Task OnStartOpen(object data, CancellationToken token)
    {
        await base.OnStartOpen(data, token);
    }

    protected virtual bool LoadSpellIconsOnLoad() { return true; }

}
