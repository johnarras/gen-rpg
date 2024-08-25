using Assets.Scripts.Login.Messages.Core;
using Genrpg.Shared.UserCoins.Constants;
using Genrpg.Shared.UserEnergy.Messages;
using Genrpg.Shared.Users.PlayerData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Assets.Scripts.BoardGame.MessageHandlers
{
    public class UpdateUserEnergyMessageHandler : BaseClientLoginResultHandler<UpdateUserEnergyResult>
    {
        protected override void InnerProcess(UpdateUserEnergyResult result, CancellationToken token)
        {
            CoreUserData userData = _gs.ch.Get<CoreUserData>();
            userData.LastHourlyReset = result.LastHourlyReset;
            userData.Coins.Add(UserCoinTypes.Energy, result.EnergyAdded);
            _dispatcher.Dispatch(result);
        }
    }
}
