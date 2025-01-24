using MessagePack;
using Genrpg.Shared.GameSettings.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using Genrpg.Shared.GameSettings.PlayerData;
using Genrpg.Shared.Purchasing.PlayerData;
using Genrpg.Shared.Website.Interfaces;

namespace Genrpg.Shared.GameSettings.WebApi.RefreshGameSettings
{
    [MessagePackObject]
    public sealed class RefreshGameSettingsResponse : IWebResponse
    {
        [JsonProperty(TypeNameHandling = TypeNameHandling.Auto)]
        [Key(0)] public List<ITopLevelSettings> NewSettings { get; set; } = new List<ITopLevelSettings>();
        [Key(1)] public GameDataOverrideList DataOverrides { get; set; } = null;
    }
}
