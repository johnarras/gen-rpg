﻿using Genrpg.ServerShared.Config;
using Genrpg.Shared.Analytics.Services;
using Genrpg.Shared.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.ServerShared.Analytics.Services
{
    public class ServerAnalyticsService : IAnalyticsService
    {

        public async Task Initialize(GameState gs, CancellationToken toke)
        {
            await Task.CompletedTask;
        }


        private IServerConfig _serverConfig = null;
        public ServerAnalyticsService(IServerConfig serverConfig)
        {
            _serverConfig = serverConfig;
        }


        public void Send(GameState gs, string eventId, string eventType, string eventSubtype, Dictionary<string,string> extraData = null)
        {
        }

    }
}