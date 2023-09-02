using Genrpg.ServerShared.CloudMessaging.Messages;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.ServerShared.CloudMessaging.Requests
{
    public class CloudResponseEnvelope
    {
        [JsonProperty(TypeNameHandling = TypeNameHandling.Auto)]
        public ICloudResponse Response { get; set; }
        public string ErrorText { get; set; }
    }
}
