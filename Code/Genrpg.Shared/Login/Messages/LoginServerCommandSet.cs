using MessagePack;
using Genrpg.Shared.Login.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Login.Messages
{
    [MessagePackObject]
    public class LoginServerCommandSet
    {
        [Key(0)] public string UserId { get; set; }
        [Key(1)] public string SessionId { get; set; }
        [JsonProperty(TypeNameHandling = TypeNameHandling.Auto)]
        [Key(2)] public List<ILoginCommand> Commands { get; set; } = new List<ILoginCommand>();
    }
}
