using Cysharp.Threading.Tasks;
using Genrpg.Shared.Inventory.PlayerData;
using Genrpg.Shared.Units.Entities;
using System.Threading;

public class ItemIconScreen : DragItemScreen<Item,ItemIcon,ItemIconScreen,InitItemIconData>
{
    protected override async UniTask OnStartOpen(object data, CancellationToken token)
    {
        await base.OnStartOpen(data, token);
        await UniTask.CompletedTask;
    }

    virtual public Unit GetUnit() { return _gs.ch; }
}
