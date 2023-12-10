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

namespace Genrpg.MapServer.Services.Maps
{
    public class MapServerService : IMapServerService
    {
        private IMapDataService _mapDataService = null;
        private ICloudCommsService _cloudCommsService = null;

        private List<MapInstance> _instances = new List<MapInstance>();

        private string _mapServerId;
        private int _nextInstanceId = 0;
        private int _mapServerIndex = -1;
        private int _mapServerCount = -1;
        private string _serverId = null;
        private string _messageQueueId = null;
        public async Task Init(ServerGameState gs, InitMapServerData mapData, CancellationToken serverToken)
        {
            _serverId = CloudServerNames.Map + mapData.MapServerId;
            _mapServerId = mapData.MapServerId;
            _mapServerIndex = mapData.MapServerIndex;
            _mapServerCount = mapData.MapServerCount;

            _messageQueueId = _cloudCommsService.GetFullServerName(_serverId);

            SendAddMapServerMessage();

            List<MapStub> mapStubs = await _mapDataService.GetMapStubs(gs);

            foreach (MapStub stub in mapStubs)
            {
                if (int.TryParse(stub.Id, out int mapStubId))
                {
                    if (MapInstanceConstants.ServerTestMode && mapStubId != 2)
                    {
                        continue;
                    }
                    if (mapStubId % _mapServerCount == _mapServerIndex)
                    {
                        MapInstance mapInstance = new MapInstance();

                        InitMapInstanceData initData = new InitMapInstanceData()
                        {
                            MapId = stub.Id,
                            Port = mapData.StartPort + mapStubId,
                            Serializer = EMapApiSerializers.MessagePack,
                        };

                        await mapInstance.Init(initData, serverToken);

                        _instances.Add(mapInstance);
                    }
                }
            }
        }

        public void SendAddMapServerMessage()
        {
            AddMapServer addServer = new AddMapServer()
            {
                ServerId = _messageQueueId,
            };

            _cloudCommsService.SendQueueMessage(CloudServerNames.Instance, addServer);

        }

        public List<MapInstance> GetMapInstances() { return _instances.ToList(); }
    }
}
