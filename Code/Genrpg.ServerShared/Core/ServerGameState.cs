
using System;
using System.Collections;
using System.Configuration;
using System.Diagnostics;
using Genrpg.ServerShared.Config;
using Genrpg.ServerShared.Logging;
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
            loc = new ServiceLocator(logService);
            loc.Set(config);
        }

        public ServerGameState(IServerConfig config, ILogService logService)
        {
            loc = new ServiceLocator(logService);
            loc.Set(config);
        }     

        protected override GameState CreateGameStateInstance(ILogService logService = null)
        {
            if (logService == null)
            {
                return (GameState)Activator.CreateInstance(GetType(), new object[] { loc.Get<IServerConfig>() });
            }
            else
            {
                return (GameState)Activator.CreateInstance(GetType(), new object[] { loc.Get<IServerConfig>(), logService });
            }
        }

        public override GameState CreateGameStateCopy()
        {
            ServerGameState state = base.CreateGameStateCopy() as ServerGameState;
            return state;
        }


    }
}
