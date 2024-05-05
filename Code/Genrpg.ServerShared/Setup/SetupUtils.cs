
using Genrpg.ServerShared.Config;
using Genrpg.ServerShared.Core;
using Genrpg.ServerShared.GameSettings.Services;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Setup.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.ServerShared.Setup
{
    public class SetupUtils
    {
        public static async Task<GS> SetupFromConfig<GS>(object currentObject, string serverId, 
            SetupService setupService, CancellationToken token, IServerConfig serverConfigIn = null) where GS : ServerGameState
        {
            if (string.IsNullOrEmpty(serverId))
            {
                throw new Exception("Missing ServerId in setup code!");
            }

            IServerConfig config = serverConfigIn;

            if (config == null)
            {
                config = await ConfigUtils.SetupServerConfig(token, serverId);
            }

            GS gs = (GS)Activator.CreateInstance(typeof(GS), new object[] { config });
            await setupService.SetupGame(gs, token);

            IGameDataService gameDataService = gs.loc.Get<IGameDataService>();
            IGameData gameData = await gameDataService.LoadGameData(gs, setupService.CreateMissingGameData());

            await setupService.FinalSetup(gs);
         
            gs.loc.Resolve(currentObject);

            return gs;
        }
    }
}
