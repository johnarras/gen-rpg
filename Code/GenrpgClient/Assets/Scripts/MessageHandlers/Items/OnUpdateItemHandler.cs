using Genrpg.Shared.Inventory.PlayerData;
using Genrpg.Shared.Inventory.Messages;
using System.Threading;
using Genrpg.Shared.Inventory.Utils;

namespace Assets.Scripts.MessageHandlers.Items
{
    public class OnUpdateItemHandler : BaseClientMapMessageHandler<OnUpdateItem>
    {
        protected override void InnerProcess(UnityGameState gs, OnUpdateItem msg, CancellationToken token)
        {

            if (msg.UnitId != gs.ch.Id)
            {
                return;
            }

            InventoryData inventory = gs.ch.Get<InventoryData>();

            Item item = inventory.GetItem(msg.Item.Id);

            if (item != null)
            {
                ItemUtils.CopyStatsFrom(msg.Item, item);
            }
        }
    }
}
