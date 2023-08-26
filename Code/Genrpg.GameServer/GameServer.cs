using Genrpg.MapServer.Maps;
using Genrpg.MapServer.MainServer;
using Genrpg.ServerShared.MainServer;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Genrpg.InstanceServer;
using Genrpg.PlayerServer;
using Genrpg.MonsterServer;
using Genrpg.Shared.MapMessages;
using Genrpg.Shared.MapObjects.Messages;
using MessagePack;
using System.Threading;
using Genrpg.Shared.Constants.TempDev;
using Microsoft.Extensions.Azure;
using Genrpg.Shared.DataStores.Interfaces;
using Genrpg.ServerShared.Config;
using Genrpg.ServerShared.Logging;
using Genrpg.Shared.Logs.Entities;

namespace Genrpg.GameServer
{
    public class GameServer
    {
        static void Main(string[] args)
        {
            RunGame().Wait();
        }


        private static List<BaseServer> _servers = new List<BaseServer>();
        private static CancellationTokenSource _serverTokenSource = new CancellationTokenSource();
        private static async Task RunGame()
        {
            ServerConfig serverConfig = null;
            ILogSystem serverLogger = null;
            try
            {
                serverConfig = await ConfigUtils.SetupServerConfig(_serverTokenSource.Token, "GameServer");
                serverLogger = new ServerLogger(serverConfig);

                InstanceServerMain instanceServer = new InstanceServerMain();
                await instanceServer.Init(null, _serverTokenSource.Token);
                _servers.Add(instanceServer);

                PlayerServerMain playerServer = new PlayerServerMain();
                await playerServer.Init(null, _serverTokenSource.Token);
                _servers.Add(playerServer);

                MonsterServerMain monsterServer = new MonsterServerMain();
                await monsterServer.Init(null, _serverTokenSource.Token);
                _servers.Add(monsterServer);

                int serverCount = 2;

                for (int i = 0; i < serverCount; i++)
                {
                    InitMapServerData initServerData = new InitMapServerData()
                    {
                        MapServerCount = serverCount,
                        MapServerIndex = i,
                        MapServerId = i.ToString(),
                        StartPort = TempDevConstants.StartPort,
                        MapIds = new List<string>(),
                    }; 
                    
                    MapServerMain mapServer = new MapServerMain();

                    await mapServer.Init(initServerData, _serverTokenSource.Token);

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
