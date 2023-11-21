using Azure.Messaging.ServiceBus.Administration;
using Azure.Messaging.ServiceBus;
using Genrpg.ServerShared.Core;
using Genrpg.Shared.Reflection.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Genrpg.ServerShared.CloudComms.PubSub.Entities;

namespace Genrpg.ServerShared.CloudComms.PubSub.Topics.Core
{
    public interface IPubSubHelper
    {
        Task Init(ServerGameState gs, ServiceBusClient client, ServiceBusAdministrationClient adminClient, IReflectionService _reflectionService, string env, string serverId, CancellationToken token);
        void SendMessage(ServerGameState gs, IPubSubMessage message);
        bool IsValidMessage(IPubSubMessage message);
        void SetMessageHandlers(ServerGameState gs, IReflectionService reflectionService);

    }
}
