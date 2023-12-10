using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.ServerShared.CloudComms.Queues.Requests.Entities
{
    public class PendingQueueRequest
    {
        public string RequestId { get; set; } = null;
        public string ToServerId { get; set; } = null;
        public string FromServerId { get; set; } = null;
        public IRequestQueueMessage Request { get; set; } = null;
        public IResponseQueueMessage Response { get; set; } = null;
        public DateTime SendTime { get; set; } = DateTime.UtcNow;
    }
}
