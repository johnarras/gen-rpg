using MessagePack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using Genrpg.Shared.Website.Interfaces;

namespace Genrpg.Shared.Website.Messages
{
    [MessagePackObject]
    public class WebServerCommandSet
    {
        [Key(0)] public string UserId { get; set; }
        [Key(1)] public string SessionId { get; set; }
        [JsonProperty(TypeNameHandling = TypeNameHandling.Auto)]
        [Key(2)] public List<IWebCommand> Commands { get; set; } = new List<IWebCommand>();
    }
}
