using Genrpg.MapServer.CloudMessaging.Interfaces;
using Genrpg.MapServer.Maps;
using Genrpg.MapServer.Maps.Constants;
using Genrpg.MapServer.Services.Maps;
using Genrpg.MapServer.Setup;
using Genrpg.ServerShared.CloudComms.Constants;
using Genrpg.ServerShared.CloudComms.Servers.InstanceServer.Queues;
using Genrpg.ServerShared.Core;
using Genrpg.ServerShared.MainServer;
using Genrpg.ServerShared.Maps;
using Genrpg.ServerShared.Setup;
using Genrpg.Shared.DataStores.Interfaces;
using Genrpg.Shared.GameSettings;
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

        private IMapServerService _mapServerService = null;

        public override async Task Init(object data, CancellationToken serverToken)
        {
            await base.Init(data, serverToken);

            await _mapServerService.Init(_gs, data as InitMapServerData, serverToken);
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

        protected override void SetupCustomCloudMessagingHandlers()
        {
            _cloudCommsService.SetQueueMessageHandlers(_reflectionService.SetupDictionary<Type, IMapServerCloudMessageHandler>(_gs));
        }
    }
}
