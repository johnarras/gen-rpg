using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.ServerShared.CloudMessaging.Requests
{
    public class CloudRequestEnvelope
    {
        [JsonProperty(TypeNameHandling = TypeNameHandling.Auto)]
        public ICloudRequest Request { get; set; }
    }
}
