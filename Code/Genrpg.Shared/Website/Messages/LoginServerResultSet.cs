using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Genrpg.Shared.Website.Interfaces;

namespace Genrpg.Shared.Website.Messages
{
    [MessagePackObject]
    public class LoginServerResultSet
    {
        [JsonProperty(TypeNameHandling = TypeNameHandling.Auto)]
        [Key(0)] public List<IWebResult> Results { get; set; } = new List<IWebResult>();
    }
}
