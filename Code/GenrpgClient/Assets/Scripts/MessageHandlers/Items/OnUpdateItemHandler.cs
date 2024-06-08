using Genrpg.Shared.Inventory.PlayerData;
using Genrpg.Shared.Inventory.Messages;
using System.Threading;
using Genrpg.Shared.Inventory.Utils;

namespace Assets.Scripts.MessageHandlers.Items
{
    public class OnUpdateItemHandler : BaseClientMapMessageHandler<OnUpdateItem>
    {
        protected override void InnerProcess(OnUpdateItem msg, CancellationToken token)
        {

            if (msg.UnitId != _gs.ch.Id)
            {
                return;
            }

            InventoryData inventory = _gs.ch.Get<InventoryData>();

            Item item = inventory.GetItem(msg.Item.Id);

            if (item != null)
            {
                ItemUtils.CopyStatsFrom(msg.Item, item);
            }
        }
    }
}
