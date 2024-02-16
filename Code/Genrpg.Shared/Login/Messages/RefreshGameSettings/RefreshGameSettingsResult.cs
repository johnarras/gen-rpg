using MessagePack;
using Genrpg.Shared.GameSettings.Interfaces;
using Genrpg.Shared.Login.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using Genrpg.Shared.GameSettings.PlayerData;
using Genrpg.Shared.Purchasing.PlayerData;

namespace Genrpg.Shared.Login.Messages.RefreshGameSettings
{
    [MessagePackObject]
    public sealed class RefreshGameSettingsResult : ILoginResult
    {
        [JsonProperty(TypeNameHandling = TypeNameHandling.Auto)]
        [Key(0)] public List<ITopLevelSettings> NewSettings { get; set; } = new List<ITopLevelSettings>();
        [Key(1)] public GameDataOverrideList Overrides { get; set; } = null;
    }
}
