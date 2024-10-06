using Genrpg.MapServer.MainServer;
using Genrpg.ServerShared.MainServer;
using Genrpg.InstanceServer;
using Genrpg.PlayerServer;
using Genrpg.MonsterServer;
using Genrpg.ServerShared.Config;
using Genrpg.ServerShared.Logging;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Logging.Interfaces;

namespace Genrpg.GameServer
{
    /// <summary>
    /// This server exists to allow devs to spin up an entire stack on a custom MessagingEnv
    /// so they can develop locally in a sandbox. Really, the microservices may end up being
    /// stateless, and the map instance servers should all be spun up separately.
    /// </summary>
    public class GameServerMain
    {
        static async Task Main(string[] args)
        {
            await new GameServer().RunGame();
        }

    }

    public class GameServer
    { 
        private List<IBaseServer> _servers = new List<IBaseServer>();
        private CancellationTokenSource _serverTokenSource = new CancellationTokenSource();
        public async Task RunGame()
        {
            IServerConfig serverConfig = null;
            ILogService serverLogger = null;
            try
            {
                serverConfig = await new ConfigSetup().SetupServerConfig(_serverTokenSource.Token, "GameServer");

                InstanceServerMain instanceServer = new InstanceServerMain();
                await instanceServer.Init(null, null, _serverTokenSource.Token);
                _servers.Add(instanceServer);

                PlayerServerMain playerServer = new PlayerServerMain();
                await playerServer.Init(null, null, _serverTokenSource.Token);
                _servers.Add(playerServer);

                MonsterServerMain monsterServer = new MonsterServerMain();
                await monsterServer.Init(null, null, _serverTokenSource.Token);
                _servers.Add(monsterServer);

                int serverCount = 2;

                for (int i = 0; i < serverCount; i++)
                {
                    InitMapServerData initServerData = new InitMapServerData()
                    {
                        MapServerCount = serverCount,
                        MapServerIndex = i,
                        MapServerId = HashUtils.NewGuid(),
                        StartPort = 4000 + 100*i,
                        MapIds = new List<string>(),
                    }; 
                    
                    MapServerMain mapServer = new MapServerMain();

                    await mapServer.Init(initServerData, null, _serverTokenSource.Token);

                    _servers.Add(mapServer);

                }

                while (true)
                {
                    await Task.Delay(2000, _serverTokenSource.Token);
                }
            }
            catch (Exception ex)
            {
                serverLogger?.Exception(ex, "RunGame");
            }
        }
    }
}
