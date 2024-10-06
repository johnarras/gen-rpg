
using Genrpg.Shared.Inventory.PlayerData;
using Genrpg.Shared.Units.Entities;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class ItemIconScreen : DragItemScreen<Item,ItemIcon,ItemIconScreen,InitItemIconData>
{
    protected override async Task OnStartOpen(object data, CancellationToken token)
    {
        await base.OnStartOpen(data, token);
        
    }

    virtual public Unit GetUnit() { return _gs.ch; }
}
