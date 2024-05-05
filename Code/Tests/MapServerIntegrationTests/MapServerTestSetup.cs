using Genrpg.MapServer.MainServer;
using Genrpg.ServerShared.Config;
using Genrpg.ServerShared.Core;
using Genrpg.ServerShared.Setup;
using Genrpg.Shared.Utils;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace Tests.MapServerTests
{
    public static class MapServerTestSetup
    {
        private static object _setupLock = new object();
        private static MapServerMain? _cachedMap = null;
        public static async Task<MapServerMain?> GetMapServer(string serverId, object parentObject)
        {
            CancellationTokenSource? newSource = new CancellationTokenSource();
            MapServerMain mapServer = new MapServerMain();

            InitMapServerData initServerData = new InitMapServerData()
            {
                MapServerCount = 1,
                MapServerIndex = 1,
                MapServerId = HashUtils.NewGuid(),
                StartPort = 4000,
                MapIds = new List<string>(),
            };

            await mapServer.Init(initServerData, parentObject, newSource.Token);

            return mapServer;
        }
    }
}
