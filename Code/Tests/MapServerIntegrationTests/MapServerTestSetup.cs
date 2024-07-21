using Genrpg.MapServer.MainServer;
using Genrpg.Shared.Utils;

namespace Tests.MapServerTests
{
    public static class MapServerTestSetup
    {
        private static object _setupLock = new object();
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
