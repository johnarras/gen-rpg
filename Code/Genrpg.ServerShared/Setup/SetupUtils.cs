
using System;
using System.Threading.Tasks;
using Genrpg.Shared.Setup.Services;
using Genrpg.Shared.Core.Entities;
using System.Threading;
using Genrpg.ServerShared.Config;
using Genrpg.ServerShared.Core;
using Genrpg.ServerShared.Logging;
using Genrpg.ServerShared.GameSettings.Services;
using Genrpg.ServerShared.DataStores;
using Genrpg.ServerShared.GameSettings.Interfaces;

namespace Genrpg.ServerShared.Setup
{
    public class SetupUtils
    {
        public static async Task<GS> SetupFromConfig<GS>(object parentObject, string serverId, 
            SetupService setupService, IGameDataContainer gameDataContainer, CancellationToken token, ServerConfig serverConfigIn = null) where GS : ServerGameState, new()
        {
            if (string.IsNullOrEmpty(serverId))
            {
                throw new Exception("Missing ServerId in setup code!");
            }

            ServerConfig config = serverConfigIn;

            if (config == null)
            {
                config = await ConfigUtils.SetupServerConfig(token, serverId);
            }

            GS gs = new GS();
            gs.config = config;
            gs.logger = new ServerLogger(config);
            gs.loc = new ServiceLocator(gs.logger);
            gs.repo = new ServerRepositorySystem(gs.logger, config.Env, config.ConnectionStrings, token);
            await setupService.SetupGame(gs, token);

            IGameDataService gameDataService = gs.loc.Get<IGameDataService>();
            gs.data = await gameDataService.LoadGameData(gs, setupService.CreateMissingGameData());
            gameDataService.AddGameDataContainer(gameDataContainer);

            await setupService.FinalSetup(gs);

            gs.loc.Resolve(parentObject);

            return gs;
        }
    }
}
