using Genrpg.Shared.GameSettings.Interfaces;
using Genrpg.Shared.Website.Interfaces;
using MessagePack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Website.Messages.NoUserGameData
{
    [MessagePackObject]
    public class NoUserGameDataResponse : IWebResponse
    {
        [JsonProperty(TypeNameHandling = TypeNameHandling.Auto)]
        [Key(0)] public List<ITopLevelSettings> GameData { get; set; } = new List<ITopLevelSettings>();
    }
}
