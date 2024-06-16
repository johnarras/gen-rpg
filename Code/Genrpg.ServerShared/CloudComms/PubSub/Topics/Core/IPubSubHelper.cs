using Azure.Messaging.ServiceBus.Administration;
using Azure.Messaging.ServiceBus;
using Genrpg.ServerShared.Core;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Genrpg.ServerShared.CloudComms.PubSub.Entities;
using Genrpg.Shared.Interfaces;

namespace Genrpg.ServerShared.CloudComms.PubSub.Topics.Core
{
    public interface IPubSubHelper
    {
        Task Init(ServiceBusClient client, ServiceBusAdministrationClient adminClient, string env, string serverId, CancellationToken token);
        void SendMessage(IPubSubMessage message);
        bool IsValidMessage(IPubSubMessage message);

    }
}
