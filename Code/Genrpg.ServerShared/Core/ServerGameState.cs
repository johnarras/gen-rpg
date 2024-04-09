
using System;
using System.Collections;
using System.Configuration;
using System.Diagnostics;
using Genrpg.ServerShared.Analytics.Services;
using Genrpg.ServerShared.Config;
using Genrpg.ServerShared.Logging;
using Genrpg.Shared.Analytics.Services;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Logging.Interfaces;

namespace Genrpg.ServerShared.Core
{
    public class ServerGameState : GameState
    {
        public ServerGameState()
        {

        }

        public ServerGameState(IServerConfig configIn)
        {
            IServerConfig config = configIn;
            ILogService logService = new ServerLogService(configIn);
            IAnalyticsService analyticsService = new ServerAnalyticsService(configIn);
            loc = new ServiceLocator(logService, analyticsService);
            loc.Set(config);
        }

        public ServerGameState(IServerConfig config, ILogService logService, IAnalyticsService analyticsService)
        {
            loc = new ServiceLocator(logService, analyticsService);
            loc.Set(config);           
        }     

        protected override GameState CreateGameStateInstance(ILogService logService = null, IAnalyticsService analyticsService = null)
        {
            if (logService == null || analyticsService == null)
            {
                return (GameState)Activator.CreateInstance(GetType(), new object[] { loc.Get<IServerConfig>() });
            }
            else
            {
                return (GameState)Activator.CreateInstance(GetType(), new object[] { loc.Get<IServerConfig>(), logService, analyticsService });
            }
        }

        public override GameState CreateGameStateCopy()
        {
            ServerGameState state = base.CreateGameStateCopy() as ServerGameState;
            return state;
        }


    }
}
