using MessagePack;
using Genrpg.Shared.Login.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Genrpg.Shared.Login.Messages
{
    [MessagePackObject]
    public class LoginServerResultSet
    {
        [JsonProperty(TypeNameHandling = TypeNameHandling.Auto)]
        [Key(0)] public List<ILoginResult> Results { get; set; } = new List<ILoginResult>();
    }
}
