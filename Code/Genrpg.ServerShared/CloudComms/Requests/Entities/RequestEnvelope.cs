using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.ServerShared.CloudComms.Requests.Entities
{
    public class RequestEnvelope
    {
        [JsonProperty(TypeNameHandling = TypeNameHandling.Auto)]
        public IRequest Request { get; set; }
    }
}
