using Assets.Scripts.Login.Messages.Core;
using Genrpg.Shared.UserCoins.Constants;
using Genrpg.Shared.UserEnergy.WebApi;
using Genrpg.Shared.Users.PlayerData;
using System.Threading;

namespace Assets.Scripts.BoardGame.MessageHandlers
{
    public class UpdateUserEnergyMessageHandler : BaseClientLoginResultHandler<UpdateUserEnergyResponse>
    {
        protected override void InnerProcess(UpdateUserEnergyResponse result, CancellationToken token)
        {
            CoreUserData userData = _gs.ch.Get<CoreUserData>();
            userData.LastHourlyReset = result.LastHourlyReset;
            userData.Coins.Add(UserCoinTypes.Energy, result.EnergyAdded);
            _dispatcher.Dispatch(result);
        }
    }
}
