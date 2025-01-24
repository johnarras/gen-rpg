using Genrpg.Shared.Website.Interfaces;
using MessagePack;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Genrpg.Shared.Website.Messages
{
    [MessagePackObject]
    public class WebServerRequestSet
    {
        [Key(0)] public string UserId { get; set; }
        [Key(1)] public string SessionId { get; set; }
        [JsonProperty(TypeNameHandling = TypeNameHandling.Auto)]
        [Key(2)] public List<IWebRequest> Requests { get; set; } = new List<IWebRequest>();
    }
}
