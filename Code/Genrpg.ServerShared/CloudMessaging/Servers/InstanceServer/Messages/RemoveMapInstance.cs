using Genrpg.ServerShared.CloudMessaging.Servers.InstanceServer.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.ServerShared.CloudMessaging.Servers.InstanceServer.Messaging
{
    public class RemoveMapInstance : IInstanceCloudMessage
    {
        public string MapInstanceId { get; set; }
    }
}
