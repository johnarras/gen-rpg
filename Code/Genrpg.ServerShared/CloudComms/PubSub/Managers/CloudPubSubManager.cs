using Azure.Messaging.ServiceBus.Administration;
using Azure.Messaging.ServiceBus;
using Genrpg.ServerShared.Core;
using Genrpg.ServerShared.DataStores.Constants;
using Genrpg.Shared.Logs.Entities;
using Genrpg.Shared.Utils;
using MongoDB.Driver.Core.Servers;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Genrpg.ServerShared.CloudComms.Queues.Entities;
using Genrpg.ServerShared.CloudComms.PubSub.Topics.Core;
using Genrpg.ServerShared.CloudComms.PubSub.Topics.Admin;
using Genrpg.Shared.Reflection.Services;
using Genrpg.ServerShared.CloudComms.PubSub.Constants;
using MongoDB.Bson.Serialization.Conventions;
using Genrpg.ServerShared.CloudComms.PubSub.Entities;

namespace Genrpg.ServerShared.CloudComms.PubSub.Managers
{

    internal class CloudPubSubManager
    {
        Dictionary<string, IPubSubHelper> _helpers = new Dictionary<string, IPubSubHelper>();

        public async Task Init(ServerGameState gs, ServiceBusClient serviceBusClient, ServiceBusAdministrationClient adminClient,
            IReflectionService reflectionService,
            string serverId, string env, CancellationToken token)
        {
            _helpers[PubSubTopicNames.Admin] = new AdminPubSubHelper();

            foreach (IPubSubHelper helper in _helpers.Values)
            {
                await helper.Init(gs, serviceBusClient, adminClient, reflectionService, serverId, env, token);
            }
        }

        public void SendMessage(ServerGameState gs, IPubSubMessage message)
        {
            foreach (IPubSubHelper helper in _helpers.Values)
            {
                if (helper.IsValidMessage(message))
                {
                    helper.SendMessage(gs, message);
                    return;
                }
            }
        }

        public void SetupPubSubMessageHandlers(ServerGameState gs, IReflectionService reflectionService)
        {
            foreach (IPubSubHelper helper in _helpers.Values)
            {
                helper.SetMessageHandlers(gs, reflectionService);
            }
        }
    }
}
