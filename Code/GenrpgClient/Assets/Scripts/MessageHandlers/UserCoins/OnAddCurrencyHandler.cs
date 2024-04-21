
using Genrpg.Shared.UserCoins.Messages;
using Genrpg.Shared.UserCoins.PlayerData;
using System.Threading;

namespace Assets.Scripts.MessageHandlers.Currency
{
    public class OnAddUserCoinsHandler : BaseClientMapMessageHandler<OnAddUserCoin>
    {
        protected override void InnerProcess(UnityGameState gs, OnAddUserCoin msg, CancellationToken token)
        {
            if (msg.CharId != gs.ch.Id)
            {
                return;
            }

            UserCoinData coinData = gs.ch.Get<UserCoinData>();
            coinData.Add(msg.UserCoinTypeId, msg.QuantityAdded);
            _dispatcher.Dispatch(gs,msg);
        }
    }
}
