using Amazon.Runtime.Internal.Util;
using Genrpg.RequestServer.Core;
using Genrpg.Shared.BoardGame.Constants;
using Genrpg.Shared.PlayMultiplier.Messages;
using Genrpg.Shared.PlayMultiplier.Services;
using Genrpg.Shared.PlayMultiplier.Settings;
using Genrpg.Shared.UserCoins.Constants;
using Genrpg.Shared.Users.PlayerData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.RequestServer.PlayMultiplier.Services
{
    public class ServerPlayMultService : IServerPlayMultService
    {
        private ISharedPlayMultService _sharedPlayMultService;
        public async Task SetPlayMult(WebContext context, long newPlayMult)
        {
            CoreUserData userData = await context.GetAsync<CoreUserData>();

            long level = context.user.Level;

            long energy = userData.Coins.Get(UserCoinTypes.Energy);

            List<PlayMult> validMults = _sharedPlayMultService.GetValidMults(context.user, level, energy);

            bool isOkMult = validMults.Any(x => x.Mult == newPlayMult);

            if (isOkMult == true)
            {
                userData.PlayMult = newPlayMult;
                context.Results.Add(new SetPlayMultResult() { Success = true, NewPlayMult = newPlayMult });
            }

            PlayMult okMult = validMults.LastOrDefault(x => x.Mult < newPlayMult);

            context.Results.Add(new SetPlayMultResult() { Success = false, NewPlayMult = okMult?.Mult ?? BoardGameConstants.MinPlayMult });

        }
    }
}
