using Newtonsoft.Json;

namespace Genrpg.ServerShared.CloudComms.Requests.Entities
{
    public class ResponseEnvelope
    {
        [JsonProperty(TypeNameHandling = TypeNameHandling.Auto)]
        public IResponse Response { get; set; }
        public string ErrorText { get; set; }
    }
}
