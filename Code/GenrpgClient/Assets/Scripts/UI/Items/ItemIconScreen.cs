using Genrpg.Shared.Inventory.Entities;
using Cysharp.Threading.Tasks;
using System.Threading;

public class ItemIconScreen : DragItemScreen<Item,ItemIcon,ItemIconScreen,InitItemIconData>
{
    protected override async UniTask OnStartOpen(object data, CancellationToken token)
    {
        await base.OnStartOpen(data, token);
        await UniTask.CompletedTask;
    }
}
