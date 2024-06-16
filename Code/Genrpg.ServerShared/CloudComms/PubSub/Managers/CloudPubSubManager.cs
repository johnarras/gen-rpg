using Azure.Messaging.ServiceBus.Administration;
using Azure.Messaging.ServiceBus;
using Genrpg.ServerShared.Core;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Genrpg.ServerShared.CloudComms.PubSub.Topics.Core;
using Genrpg.ServerShared.CloudComms.PubSub.Topics.Admin;
using Genrpg.ServerShared.CloudComms.PubSub.Constants;
using Genrpg.ServerShared.CloudComms.PubSub.Entities;
using Genrpg.Shared.Logging.Interfaces;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Interfaces;

namespace Genrpg.ServerShared.CloudComms.PubSub.Managers
{

    internal class CloudPubSubManager
    {
        Dictionary<string, IPubSubHelper> _helpers = new Dictionary<string, IPubSubHelper>();

        private ILogService _logService;
        private IServiceLocator _loc;
        public async Task Init(IServiceLocator loc, ILogService logService, ServiceBusClient serviceBusClient, ServiceBusAdministrationClient adminClient,
            string serverId, string env, CancellationToken token)
        {
            _loc = loc;
            _logService = logService;
            _helpers[PubSubTopicNames.Admin] = (AdminPubSubHelper)(await ReflectionUtils.CreateInstanceFromType(_loc, typeof(AdminPubSubHelper), token));
                
            foreach (IPubSubHelper helper in _helpers.Values)
            {
                await helper.Init(serviceBusClient, adminClient, serverId, env, token);
            }
        }

        public void SendMessage(IPubSubMessage message)
        {
            foreach (IPubSubHelper helper in _helpers.Values)
            {
                if (helper.IsValidMessage(message))
                {
                    helper.SendMessage(message);
                    return;
                }
            }
        }
    }
}
