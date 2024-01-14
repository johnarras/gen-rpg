
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Genrpg.Shared.Pathfinding.Services;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.Utils.Data;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.Shared.Users.Entities;
using Genrpg.Shared.Players.Constants;
using Genrpg.Shared.Spawns.Entities;
using Genrpg.Shared.Characters.PlayerData;
using System.ComponentModel;
using Genrpg.ServerShared.PlayerData;
using Genrpg.ServerShared.Setup;
using Genrpg.ServerShared.Core;
using Genrpg.MapServer.Networking;
using Genrpg.MapServer.Setup;
using Genrpg.Shared.Crafting.PlayerData;
using Genrpg.Shared.MapMessages.Interfaces;
using Genrpg.MapServer.MapMessaging.Interfaces;
using Genrpg.MapServer.CloudMessaging.Interfaces;
using Genrpg.Shared.Reflection.Services;
using System.Net;
using Newtonsoft.Json.Serialization;
using Genrpg.MapServer.Networking.Listeners;
using Genrpg.Shared.Players.Messages;
using Genrpg.Shared.Pings.Messages;
using Genrpg.Shared.Errors.Messages;
using Genrpg.Shared.Stats.Messages;
using Genrpg.Shared.Networking.Constants;
using Genrpg.Shared.Networking.Entities;
using Genrpg.ServerShared.GameSettings.Services;
using Genrpg.ServerShared.CloudComms.Constants;
using Genrpg.ServerShared.CloudComms.Servers.PlayerServer.Queues;
using Genrpg.ServerShared.CloudComms.Servers.InstanceServer.Queues;
using Genrpg.ServerShared.CloudComms.Services;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Constants;
using Genrpg.Shared.GameSettings.Messages;
using Microsoft.WindowsAzure.Storage.Blob;
using Genrpg.ServerShared.MainServer;
using Genrpg.ServerShared.Maps;
using Genrpg.ServerShared.MapSpawns;
using Genrpg.Shared.Utils;
using Genrpg.Shared.DataStores.Categories;
using Genrpg.Shared.Characters.Utils;
using Newtonsoft.Json.Linq;
using Genrpg.Shared.Logs.Interfaces;
using MongoDB.Bson.Serialization.Serializers;

namespace Genrpg.MapServer.Maps
{
    public class MapInstance : BaseServer<ServerGameState,MapInstanceSetupService,IMapInstanceCloudMessageHandler>, IDisposable
    {
        private IListener _listener = null;

        private List<ServerConnectionState> _players = new List<ServerConnectionState>();

        private object _playersLock = new object();

        private IMapMessageService _messageService = null;
        private IMapObjectManager _objectManager = null;
        private IStatService _statService = null;
        private IPathfindingService _pathfindingService = null;
        private IGameDataService _gameDataService = null;
        private IPlayerDataService _playerDataService = null;
        private IMapSpawnDataService _mapSpawnDataService = null;
        private IMapDataService _mapDataService = null;
        public const double UpdateMS = 100.0f;

        private string _mapId;
        private string _instanceId;

        private string _host = null;
        private int _port = 0;
        private int _mapSize = 0;

        private CancellationTokenSource _instanceTokenSource;

        public MapInstance()
        {

        }

        public async Task Shutdown(int msDelay = 0)
        {
            _cloudCommsService.SendQueueMessage(CloudServerNames.Instance, new RemoveMapInstance() { FullInstanceId = _serverId });
            _instanceTokenSource?.CancelAfter(msDelay);
            await Task.CompletedTask;
        }

        public string GetMapId()
        {
            return _mapId;
        }

        public string GetInstanceId()
        {
            return _instanceId;
        }

        protected bool _isRunning = false;
        public bool IsRunning()
        {
            return _isRunning;
        }

        public void Dispose()
        {
            _tokenSource.Cancel();
        }

        protected virtual IListener GetListener(string host, int port, ILogSystem logger, EMapApiSerializers serializer)
        {
            return new BaseTcpListener(host, port, logger, serializer, AddConnection, ReceiveCommands, _tokenSource.Token);
        }

        public void RefreshGameData()
        {
            Task.Run(() => RefreshGameDataAsync());
        }

        private async Task RefreshGameDataAsync()
        {
            await _gameDataService.ReloadGameData(_gs);
            _messageService.UpdateGameData(_gs.data);
            UpdatePlayerClientData();
        }

        public static string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("No network adapters with an IPv4 address in the system!");
        }

        public override async Task Init(object data, CancellationToken parentToken)
        {
            InitMapInstanceData initData = data as InitMapInstanceData;
            _mapId = initData.MapId;
            _instanceId = HashUtils.NewGuid();
            _serverId = GetServerId(null);
            _isRunning = true;
            
            _instanceTokenSource = CancellationTokenSource.CreateLinkedTokenSource(parentToken, _tokenSource.Token);

            // Step 1: basic setup
            await base.Init(data, _instanceTokenSource.Token);

            // Step 2: Load map before setting up messaging and object manager
            _gs.map = await _mapDataService.LoadMap(_gs, _mapId);
            _gs.spawns = await _mapSpawnDataService.LoadMapSpawnData(_gs, _gs.map.Id, _gs.map.MapVersion);

            // Step 3: Setup messaging and object systems
            _messageService.Init(_gs, _tokenSource.Token);
            _objectManager.Init(_gs, _tokenSource.Token);
            _port = initData.Port;
            _host = "127.0.0.1";
            _mapSize = _gs.map.BlockCount;
            
            // Step 4: Setup listener
            _listener = GetListener(IPAddress.Any.ToString(), initData.Port, _gs.logger, initData.Serializer);

            if (_gs.config.Env.IndexOf("Test") >= 0 || _gs.config.Env.IndexOf("Prod") >= 0)
            {
                _host = GetLocalIPAddress();
            }

            SendAddInstanceMessage();

            _ = Task.Run(() => ProcessMap(_tokenSource.Token), _tokenSource.Token);

            await _pathfindingService.LoadPathfinding(_gs,
                _gs.config.ContentRoot + 
                _gs.config.DataEnvs[DataCategoryTypes.WorldData] + "/");
        }

        public void SendAddInstanceMessage()
        {
            AddMapInstance addInstance = new AddMapInstance()
            {
                ServerId = _serverId,
                MapId = _mapId,
                InstanceId = _serverId,
                Port = _port,
                Host = _host,
                Size = _mapSize,
            };

            _cloudCommsService.SendQueueMessage(CloudServerNames.Instance, addInstance);
        }

        public void AddConnection(ServerConnectionState connState)
        {
            lock (_playersLock)
            {
                _players.Add(connState);
            }
        }        

        public void ReceiveCommands(List<IMapApiMessage> commands, CancellationToken token, object connStateObject )
        {
            ServerConnectionState connState = connStateObject as ServerConnectionState;

            if (token.IsCancellationRequested)
            {
                return;
            }

            if (connState.ch == null)
            {
                foreach (IMapApiMessage obj in commands)
                {
                    if (obj is AddPlayer add)
                    {
                        Task.Run(() => AddPlayerHandler(connState, add));
                    }
                }
                return;
            }
            foreach (IMapApiMessage obj in commands)
            {
                if (obj is Ping || !(obj is IPlayerCommand))
                {
                    continue;
                }

                _messageService.SendMessage(connState.ch, obj);
            }
        }

        private async Task ProcessMap(CancellationToken token)
        {

            try
            {
                using (PeriodicTimer timer = new PeriodicTimer(TimeSpan.FromMilliseconds(UpdateMS)))
                {
                    while (true)
                    {
                        UpdatePlayerConnections();
                        await timer.WaitForNextTickAsync(token).ConfigureAwait(false);
                    }
                }            
            }
            catch (OperationCanceledException ce)
            {
                _gs.logger.Info("Shutdown MapInstance.ProcessPlayerConnections");
            }
        }

        private void UpdatePlayerConnections()
        {
            List<ServerConnectionState> removePlayers = new List<ServerConnectionState>();
            lock (_playersLock)
            {
                removePlayers = _players.Where(x => x.conn.RemoveMe()).ToList();
                foreach (ServerConnectionState connState in removePlayers)
                {
                    _cloudCommsService.SendQueueMessage(CloudServerNames.Player, new PlayerLeaveMap() { Id = connState.ch?.Id });
                }
                _players = _players.Where(x => !x.conn.RemoveMe()).ToList();
            }
            foreach (ServerConnectionState playerConn in removePlayers)
            {
                ShutdownConnection(playerConn);
            }
        }

        private void ShutdownConnection(ServerConnectionState connState)
        {
            if (!connState.conn.RemoveMe())
            {
                connState.conn.Shutdown(null, "ShutdownConnMapManager");
            }

            if (connState.ch == null)
            {
                return;
            }
            _playerDataService.SavePlayerData(connState.ch, _gs.repo, true);
            Character ch = connState.ch;
            if (ch != null)
            {
                _objectManager.RemoveObject(_gs, ch.Id);
            }
            connState.ch = null;
        }

        public async Task AddPlayerHandler(ServerConnectionState connState, AddPlayer add)
        {
            try
            {
                if (connState.ch != null)
                {
                    connState.conn.AddMessage(new ErrorMessage("Player already loaded"));
                    return;
                }

                User user = await _gs.repo.Load<User>(add.UserId);

                if (user == null)
                {
                    connState.conn.AddMessage(new ErrorMessage("User does not exist"));
                    return;
                }
                if (user.SessionId != add.SessionId)
                {
                    connState.conn.AddMessage(new ErrorMessage("Invalid session id"));
                    return;
                }
                bool didLoad = false;
                if (!_objectManager.GetObject(add.CharacterId, out MapObject mapObj))
                {
                    CoreCharacter coreCh = await _gs.repo.Load<CoreCharacter>(add.CharacterId);

                    if (coreCh == null)
                    {
                        connState.conn.AddMessage(new ErrorMessage("Character does not exist"));
                        return;
                    }
                    Character ch = new Character();

                    CharacterUtils.CopyDataFromTo(coreCh, ch);
                    ch.SetConn(connState.conn);
                    ch.NearbyGridsSeen = new List<PointXZ>();
                    MapObjectGridItem gridItem = _objectManager.AddObject(_gs, ch, null);

                    if (gridItem != null)
                    {
                        connState.ch = (Character)gridItem.Obj;
                        await _playerDataService.LoadPlayerData(_gs, ch);
                    }

                    _gameDataService.SetGameDataOverrides(_gs, ch, true);

                    didLoad = true;
                }
                else
                {
                    Character ch = mapObj as Character;
                    ch.SetConn(connState.conn);
                    connState.ch = ch;
                    ch.NearbyGridsSeen = new List<PointXZ>();
                }

                if (connState.ch == null)
                {
                    connState.conn.ForceClose();
                    return;
                }

                _statService.CalcStats(_gs, connState.ch, true);
                if (didLoad)
                {
                    _messageService.SendMessage(connState.ch, connState.ch.GetCachedMessage<Regen>(true));
                    _messageService.SendMessage(connState.ch, connState.ch.GetCachedMessage<SaveDirty>(true));
                    SendPlayerEnterMapMessage(connState.ch);
                }
               
                connState.conn.AddMessage(new OnFinishLoadPlayer());
            }
            catch (Exception e)
            {
               _gs.logger.Exception(e, "AddPlayer");
            }
        }

        protected void SendPlayerEnterMapMessage(Character ch)
        {
            PlayerEnterMap playerEnterMessage = new PlayerEnterMap()
            {
                Id = ch.Id,
                Name = ch.Name,
                Level = ch.Level,
                MapId = _mapId,
                InstanceId = _serverId,
                UserId = ch.UserId,
            };
            _cloudCommsService.SendQueueMessage(CloudServerNames.Player, playerEnterMessage);
        }

        public void SendAllPlayerEnterMapMessages()
        {
            List<Character> characters = _objectManager.GetAllCharacters();
            foreach (Character ch in characters)
            {
                SendPlayerEnterMapMessage(ch);
            }
        }

        private void UpdatePlayerClientData()
        {
            _messageService.SendMessageToAllPlayers(new OnRefreshGameSettings());

            List<Character> allCharacters = _objectManager.GetAllCharacters();

            foreach (Character ch in allCharacters)
            {
                _gameDataService.SetGameDataOverrides(_gs, ch, true);
            }
        }

        protected override string GetServerId(object data)
        {
            return "minst" + _mapId + "." + _instanceId;
        }
    }
}
