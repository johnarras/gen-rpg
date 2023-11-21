using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.ServerShared.CloudComms.PubSub.Entities
{
    public class PubSubMessageEnvelope
    {
        [JsonProperty(TypeNameHandling = TypeNameHandling.Auto)]
        public IPubSubMessage Message { get; set; }
    }
}
