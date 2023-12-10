using Genrpg.ServerShared.CloudComms.Queues.Requests.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.ServerShared.CloudComms.Servers.PlayerServer.Queues
{
    public class WhoListRequest : IPlayerQueueMessage, IRequestQueueMessage
    {
        public string Args { get; set; }
        public string RequestId { get; set; }
        public string FromServerId { get; set; }
    }
}
