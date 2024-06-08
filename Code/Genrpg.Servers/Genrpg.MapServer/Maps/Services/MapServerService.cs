using Genrpg.MapServer.MainServer;
using Genrpg.MapServer.Maps.Constants;
using Genrpg.MapServer.Maps;
using Genrpg.ServerShared.CloudComms.Constants;
using Genrpg.ServerShared.CloudComms.Servers.InstanceServer.Queues;
using Genrpg.ServerShared.Maps;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.Shared.Networking.Constants;
using Microsoft.Identity.Client;
using MongoDB.Driver.Core.Servers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Genrpg.ServerShared.CloudComms.Services;
using Genrpg.ServerShared.Core;
using Genrpg.Shared.Utils;
using System.Collections.Concurrent;
using System.Formats.Asn1;

namespace Genrpg.MapServer.Maps.Services
{
    public class MapServerService : IMapServerService
    {
        private IMapDataService _mapDataService = null;
        private ICloudCommsService _cloudCommsService = null;

        public async Task Initialize(IGameState gs, CancellationToken token)
        {
            await Task.CompletedTask;
        }

        private ConcurrentDictionary<string, MapInstance> _instances = new ConcurrentDictionary<string, MapInstance>();

        private string _mapServerId;
        private int _mapServerIndex = -1;
        private int _mapServerCount = -1;
        private string _serverId = null;
        private string _messageQueueId = null;
        private int _currentPort = 0; // Need better way to do this, list of ints we pick from in concurrent bag?
        private object _currentPortLock = new object();
        private CancellationToken _serverToken;
        public async Task Init(ServerGameState gs, InitMapServerData mapData, CancellationToken serverToken)
        {
            _serverToken = serverToken;
            _currentPort = mapData.StartPort;
            _serverId = CloudServerNames.Map + mapData.MapServerId;
            _mapServerId = mapData.MapServerId;
            _mapServerIndex = mapData.MapServerIndex;
            _mapServerCount = mapData.MapServerCount;

            _messageQueueId = _cloudCommsService.GetFullServerName(_serverId);

            SendAddMapServerMessage();

            List<MapStub> mapStubs = await _mapDataService.GetMapStubs();

            foreach (MapStub stub in mapStubs)
            {
                if (int.TryParse(stub.Id, out int mapStubId))
                {
                    if (MapInstanceConstants.ServerTestMode)
                    {
                        if (mapStubId != 1)
                        {
                            continue;
                        }
                    }
                    if (mapStubId % _mapServerCount == _mapServerIndex)
                    {
                        await CreateMapInstance(stub.Id, serverToken);
                    }
                }
            }
        }

        protected async Task<MapInstance> CreateMapInstance(string mapId, CancellationToken serverToken)
        {
            MapInstance mapInstance = new MapInstance();

            int nextPortNumber = 0;

            lock (_currentPortLock)
            {
                nextPortNumber = ++_currentPort;
            }
            InitMapInstanceData initData = new InitMapInstanceData()
            {
                MapId = mapId,
                Port = nextPortNumber,
                Serializer = EMapApiSerializers.MessagePack,
            };

            await mapInstance.Init(initData, null, serverToken);

            _instances[mapInstance.GetInstanceId()] = mapInstance;

            return mapInstance;
        }

        public void SendAddMapServerMessage()
        {
            AddMapServer addServer = new AddMapServer()
            {
                ServerId = _messageQueueId,
            };

            _cloudCommsService.SendQueueMessage(CloudServerNames.Instance, addServer);

        }

        public IReadOnlyList<MapInstance> GetMapInstances()
        {
            return _instances.Values.ToList();
        }

        private MapInstance GetInstance(string instanceId)
        {
            if (_instances.TryGetValue(instanceId, out MapInstance mapInstance))
            {
                return mapInstance;
            }
            return null;
        }

        public async Task ShutdownInstance(string instanceId)
        {
            MapInstance mapInstance = GetInstance(instanceId);

            if (mapInstance == null)
            {
                return;
            }

            _instances.TryRemove(instanceId, out MapInstance removedInstance);

            await mapInstance.Shutdown();

        }

        public async Task RestartMapsWithId(string mapId)
        {
            IReadOnlyList<MapInstance> restartInstances = GetMapInstances();

            foreach (MapInstance restartInstance in restartInstances)
            {
                if (restartInstance.GetMapId() != mapId)
                {
                    continue;
                }

                await ShutdownInstance(restartInstance.GetInstanceId());

                await CreateMapInstance(restartInstance.GetMapId(), _serverToken);
            }
        }
    }
}
