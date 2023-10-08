using Genrpg.Shared.Inventory.Entities;
using System.Threading.Tasks;
using System.Threading;

public class ItemIconScreen : DragItemScreen<Item,ItemIcon,ItemIconScreen,InitItemIconData>
{
    protected override async Task OnStartOpen(object data, CancellationToken token)
    {
        await base.OnStartOpen(data, token);
        await Task.CompletedTask;
    }
}
