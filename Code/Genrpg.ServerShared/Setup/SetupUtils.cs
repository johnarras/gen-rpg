
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
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.Logging.Interfaces;
using Genrpg.Shared.GameSettings;

namespace Genrpg.ServerShared.Setup
{
    public class SetupUtils
    {
        public static async Task<GS> SetupFromConfig<GS>(object parentObject, string serverId, 
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

            gs.loc.Resolve(parentObject);

            return gs;
        }
    }
}
