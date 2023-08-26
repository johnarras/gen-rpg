
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Genrpg.Shared.Logs.Entities;
using Genrpg.Shared.Setup.Services;
using Genrpg.Shared.Core.Entities;
using Genrpg.ServerShared;
using Genrpg.Shared.Constants;
using System.Threading;
using Genrpg.ServerShared.Config;
using Genrpg.ServerShared.CloudMessaging;
using Genrpg.ServerShared.Core;
using Genrpg.ServerShared.Logging;
using Genrpg.ServerShared.GameDatas.Services;

namespace Genrpg.ServerShared.Setup
{
    public class SetupUtils
    {
        public static async Task<GS> SetupFromConfig<GS>(object parentObject, string serverId, 
            SetupService setupService, CancellationToken token, ServerConfig serverConfigIn = null) where GS : ServerGameState, new()
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
            gs.repo = new ServerRepositorySystem(gs.logger, config.Env, config.ConnectionStrings);
            await setupService.SetupGame(gs, token);

            IGameDataService gameDataService = gs.loc.Get<IGameDataService>();
            gs.data = await gameDataService.LoadGameData(gs, setupService.CreateMissingGameData());

            await setupService.FinalSetup(gs);

            gs.loc.Resolve(parentObject);

            return gs;
        }
    }
}
