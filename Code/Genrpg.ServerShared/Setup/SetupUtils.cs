
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
        public static async Task<GS> SetupFromConfig<GS, TSetupService>(object currentObject, string serverId, CancellationToken token, IServerConfig serverConfigIn = null) 
            where GS : ServerGameState
            where TSetupService : SetupService
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
            TSetupService setupService = (TSetupService)Activator.CreateInstance(typeof(TSetupService), new object[] { gs.loc });
            await setupService.SetupGame(token);

            IGameDataService gameDataService = gs.loc.Get<IGameDataService>();
            IGameData gameData = await gameDataService.LoadGameData(setupService.CreateMissingGameData());

            await setupService.FinalSetup();
         
            gs.loc.Resolve(currentObject);

            return gs;
        }
    }
}
