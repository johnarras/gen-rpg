using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.ServerShared.CloudComms.Queues.Entities
{
    public class QueueMessageEnvelope
    {
        public string ToServerId { get; set; }
        public string FromServerId { get; set; }
        [JsonProperty(TypeNameHandling = TypeNameHandling.Auto)]
        public IQueueMessage Message { get; set; }
    }
}
