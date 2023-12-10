using Genrpg.ServerShared.CloudComms.Queues.Requests.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.ServerShared.CloudComms.Servers.InstanceServer.Queues
{
    public class GetInstanceQueueRequest : IInstanceQueueMessage, IRequestQueueMessage
    {
        public string RequestId { get; set; }
        public string FromServerId { get; set; }
        public string MapId { get; set; }
    }
}
