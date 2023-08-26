using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.ServerShared.CloudMessaging.Messages
{
    public class CloudMessageEnvelope
    {
        public string ToServerId { get; set; }
        public string FromServerId { get; set; }
        [JsonProperty(TypeNameHandling = TypeNameHandling.Auto)]
        public ICloudMessage Message { get; set; }
    }
}
