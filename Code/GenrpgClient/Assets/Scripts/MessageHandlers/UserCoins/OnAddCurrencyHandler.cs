
using Genrpg.Shared.UserCoins.Messages;
using Genrpg.Shared.UserCoins.PlayerData;
using System.Threading;

namespace Assets.Scripts.MessageHandlers.Currency
{
    public class OnAddUserCoinsHandler : BaseClientMapMessageHandler<OnAddUserCoin>
    {
        protected override void InnerProcess(OnAddUserCoin msg, CancellationToken token)
        {
            if (msg.CharId != _gs.ch.Id)
            {
                return;
            }

            UserCoinData coinData = _gs.ch.Get<UserCoinData>();
            coinData.Add(msg.UserCoinTypeId, msg.QuantityAdded);
            _dispatcher.Dispatch(msg);
        }
    }
}
