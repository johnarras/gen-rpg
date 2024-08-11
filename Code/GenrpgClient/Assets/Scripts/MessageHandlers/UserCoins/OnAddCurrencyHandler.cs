
using Genrpg.Shared.UserCoins.Messages;
using Genrpg.Shared.Users.PlayerData;
using System.Threading;
using UnityEngine;

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

            CoreUserData coreUserData = _gs.ch.Get<CoreUserData>();
            coreUserData.Coins.Add(msg.UserCoinTypeId, msg.QuantityAdded);
            _dispatcher.Dispatch(msg);
        }
    }
}
