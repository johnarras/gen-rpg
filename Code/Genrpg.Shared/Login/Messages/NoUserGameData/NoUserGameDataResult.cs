using Genrpg.Shared.GameSettings.Interfaces;
using Genrpg.Shared.Login.Interfaces;
using MessagePack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Login.Messages.NoUserGameData
{
    [MessagePackObject]
    public class NoUserGameDataResult : ILoginResult
    {
        [JsonProperty(TypeNameHandling = TypeNameHandling.Auto)]
        [Key(0)] public List<ITopLevelSettings> GameData { get; set; } = new List<ITopLevelSettings>();
    }
}
