using Genrpg.Shared.Spells.Entities;
using System.Threading.Tasks;
using System.Threading;

public class SpellIconScreen : DragItemScreen<Spell,SpellIcon,SpellIconScreen,InitSpellIconData>
{
    protected override async Task OnStartOpen(object data, CancellationToken token)
    {
        await base.OnStartOpen(data, token);
    }

    protected virtual bool LoadSpellIconsOnLoad() { return true; }

}
