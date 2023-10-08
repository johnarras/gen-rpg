using Genrpg.MapServer.CloudMessaging.Interfaces;
using Genrpg.MapServer.Maps;
using Genrpg.MapServer.Setup;
using Genrpg.ServerShared.CloudMessaging.Constants;
using Genrpg.ServerShared.CloudMessaging.Requests;
using Genrpg.ServerShared.CloudMessaging.Servers.InstanceServer.Messaging;
using Genrpg.ServerShared.CloudMessaging.Servers.PlayerServer.Requests;
using Genrpg.ServerShared.CloudMessaging.Services;
using Genrpg.ServerShared.Core;
using Genrpg.ServerShared.MainServer;
using Genrpg.ServerShared.Maps;
using Genrpg.ServerShared.Setup;
using Genrpg.Shared.DataStores.Interfaces;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.Shared.Networking.Constants;
using Genrpg.Shared.Setup.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Microsoft.Azure.Amqp.Serialization.SerializableType;

namespace Genrpg.MapServer.MainServer
{
    public class MapServerMain : BaseServer
    {

        private IMapDataService _mapDataService;

        private List<MapInstance> _instances = new List<MapInstance>();

        private string _mapServerId;
        private int _nextInstanceId = 0;
        private int _mapServerIndex = -1;
        private int _mapServerCount = -1;

        public override async Task Init(object data, CancellationToken serverToken)
        {
            InitMapServerData mapData = data as InitMapServerData;
            _serverId = CloudServerNames.Map + mapData.MapServerId;
            _mapServerId = mapData.MapServerId;
            _mapServerIndex = mapData.MapServerIndex;
            _mapServerCount = mapData.MapServerCount;
            await base.Init(data, serverToken);

            string messageQueueId = _cloudMessageService.GetFullQueueName(_serverId);

            AddMapServer addServer = new AddMapServer()
            {
                ServerId = messageQueueId,
            };

            _cloudMessageService.SendMessage(CloudServerNames.Instances, addServer);

            List<MapStub> mapStubs = await _mapDataService.GetMapStubs(_gs);

            foreach (MapStub stub in mapStubs)
            {
                if (int.TryParse(stub.Id, out int mapStubId))
                {
                    if (mapStubId != 2)
                    {
                        continue;
                    }
                    if (mapStubId % _mapServerCount == _mapServerIndex)
                    {
                        MapInstance mapInstance = new MapInstance();

                        InitMapInstanceData initData = new InitMapInstanceData()
                        {
                            MapId = stub.Id,
                            MapServerId = mapData.MapServerId,
                            Port = mapData.StartPort + mapStubId,
                            InstanceId = (++_nextInstanceId).ToString(),
                            MapServerMessageQueueId = messageQueueId,
                            Serializer = EMapApiSerializers.MessagePack,
                        };

                        await mapInstance.Init(initData, _tokenSource.Token);

                        _instances.Add(mapInstance);
                    }
                }
            }
        }

        protected override string GetServerId(object data)
        {
            InitMapServerData mapData = data as InitMapServerData;
            return CloudServerNames.Map + mapData.MapServerId;
        }

        protected override SetupService GetSetupService(object data)
        {
            return new MapServerSetupService();
        }

        protected override void SetupMessagingHandlers()
        {
            _cloudMessageService.SetMessageHandlers(_reflectionService.SetupDictionary<Type, IMapServerCloudMessageHandler>(_gs));
        }

    }
}
