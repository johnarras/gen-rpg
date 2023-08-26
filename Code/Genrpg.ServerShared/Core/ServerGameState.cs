
using System;
using System.Collections;
using System.Diagnostics;
using Genrpg.ServerShared.Config;
using Genrpg.Shared.Core.Entities;

namespace Genrpg.ServerShared.Core
{
    public class ServerGameState : GameState
    {
        public ServerConfig config;

        public override GameState CopySharedData()
        {
            ServerGameState state = base.CopySharedData() as ServerGameState;
            if (state != null)
            {
                state.config = config;
            }
            return state;
        }


    }
}
