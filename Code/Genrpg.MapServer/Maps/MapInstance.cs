﻿
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
using Genrpg.Shared.Characters.Entities;
using System.ComponentModel;
using Genrpg.ServerShared.PlayerData;
using Genrpg.ServerShared.Setup;
using Genrpg.ServerShared.Core;
using Genrpg.MapServer.Networking;
using Genrpg.MapServer.Setup;
using Genrpg.Shared.Crafting.Entities;
using Genrpg.Shared.MapMessages.Interfaces;
using Genrpg.MapServer.MapMessaging.Interfaces;
using Genrpg.ServerShared.CloudMessaging.Services;
using Genrpg.ServerShared.CloudMessaging.Messages.InstanceServer;
using Genrpg.ServerShared.CloudMessaging.Constants;
using Genrpg.MapServer.CloudMessaging.Interfaces;
using Genrpg.Shared.Reflection.Services;
using System.Net;
using Genrpg.ServerShared.CloudMessaging.Messages.PlayerServer;
using Newtonsoft.Json.Serialization;
using Genrpg.MapServer.Networking.Listeners;
using Genrpg.Shared.Players.Messages;
using Genrpg.Shared.Pings.Messages;
using Genrpg.Shared.Errors.Messages;
using Genrpg.Shared.Stats.Messages;
using Genrpg.Shared.Networking.Constants;
using Genrpg.Shared.Networking.Entities;

namespace Genrpg.MapServer.Maps
{
    public class MapInstance : IDisposable
    {
        private IListener _listener = null;
        private ServerGameState _gs = null;

        private List<ServerConnectionState> _players = new List<ServerConnectionState>();

        private object _playersLock = new object();

        private IMapMessageService _messageService = null;
        private IMapObjectManager _objectManager = null;
        private IStatService _statService = null;
        private IPathfindingService _pathfindingService = null;
        private ICloudMessageService _cloudMessageService = null;
        private IReflectionService _reflectionService = null;
        public const double UpdateMS = 100.0f;

        private string _mapId;
        private string _mapServerId;
        private string _instanceId;
        private string _mapServerQueueId;

        public MapInstance()
        {

        }

        protected bool _isRunning = false;
        public bool IsRunning()
        {
            return _isRunning;
        }

        protected CancellationTokenSource _tokenSource = new CancellationTokenSource();
        public void Dispose()
        {
            _tokenSource.Cancel();
        }


        protected virtual IListener GetListener(string host, int port, EMapApiSerializers serializer)
        {
            return new BaseTcpListener(host, port, serializer, AddConnection, ReceiveCommands, _tokenSource.Token);
        }

        public async Task Init(InitMapInstanceData initData, CancellationToken parentToken)
        {
            _mapId = initData.MapId;
            _mapServerId = initData.MapServerId;
            _instanceId = initData.MapServerId + initData.InstanceId;
            _mapServerQueueId = initData.MapServerMessageQueueId;
            _tokenSource = CancellationTokenSource.CreateLinkedTokenSource(parentToken, _tokenSource.Token);
            _isRunning = true;
            string queueName = CloudMessageService.GetQueueNameForMap(initData.MapId,
                _instanceId);
            _gs = await SetupUtils.SetupFromConfig<ServerGameState>(this, queueName, new MapInstanceSetupService(initData.MapId), _tokenSource.Token);

            _cloudMessageService.SetMessageHandlers(_reflectionService.SetupDictionary<Type, IMapInstanceCloudMessageHandler>(_gs));

            _listener = GetListener(IPAddress.Any.ToString(), initData.Port, initData.Serializer);
            _messageService.Init(_gs, _tokenSource.Token);
            _objectManager.Init(_gs, _tokenSource.Token);

            AddMapInstance addInstance = new AddMapInstance()
            {
                MapInstanceId = queueName,
                MapId = _mapId,

            };

            _cloudMessageService.SendMessage(CloudServerNames.Instances, addInstance);
            _ = Task.Run(() => ProcessMap(), _tokenSource.Token);

            await _pathfindingService.LoadPathfinding(_gs);
        }

        public void AddConnection(ServerConnectionState connState)
        {
            lock (_playersLock)
            {
                _players.Add(connState);
            }
        }

        public void ReceiveCommands(ServerConnectionState connState, List<IMapApiMessage> commands, CancellationToken token)
        {
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
                        _ = Task.Run(() => AddPlayerHandler(connState, add));
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

                _messageService.SendMessage(_gs, connState.ch, obj);
            }
        }

        private async Task ProcessMap()
        {
            while (true)
            {
                DateTime startTime = DateTime.UtcNow;
                UpdatePlayerConnections();
                int msDelay = (int)Math.Max(0, UpdateMS - (DateTime.UtcNow - startTime).TotalMilliseconds);
                if (msDelay > 0)
                {
                    await Task.Delay(msDelay, _tokenSource.Token);
                }
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
                    _cloudMessageService.SendMessage(CloudServerNames.Player, new PlayerLeaveMap() { Id = connState.ch?.Id });
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
            PlayerDataUtils.SavePlayerData(connState.ch, _gs.repo, true);
            Character ch = connState.ch;
            if (ch != null)
            {
                _objectManager.RemoveObject(ch.Id);
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
                    Character ch = await _gs.repo.Load<Character>(add.CharacterId);

                    if (ch == null)
                    {
                        connState.conn.AddMessage(new ErrorMessage("Character does not exist"));
                        return;
                    }
                    ch.SetConn(connState.conn);
                    ch.NearbyGridsSeen = new List<PointXZ>();
                    MapObjectGridItem gridItem = _objectManager.AddObject(ch, null);

                    if (gridItem != null)
                    {
                        connState.ch = (Character)gridItem.Obj;
                        await PlayerDataUtils.LoadPlayerData(_gs, ch);
                    }

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
                    _messageService.SendMessage(_gs, connState.ch, connState.ch.GetCachedMessage<Regen>(true));
                    _messageService.SendMessage(_gs, connState.ch, connState.ch.GetCachedMessage<SaveDirty>(true));
                }

                _cloudMessageService.SendMessage(CloudServerNames.Player, new PlayerEnterMap()
                {
                    Id = connState.ch.Id,
                    Name = connState.ch.Name,
                    Level = connState.ch.Level,
                    MapId = _mapId,
                    MapInstanceId = _instanceId,
                    UserId = connState.ch.UserId,
                });

                connState.conn.AddMessage(new OnFinishLoadPlayer());
            }
            catch (Exception e)
            {
               _gs.logger.Exception(e, "AddPlayer");
            }
        }
    }
}