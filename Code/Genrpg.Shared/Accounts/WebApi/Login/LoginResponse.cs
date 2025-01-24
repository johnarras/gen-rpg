using MessagePack;
using Genrpg.Shared.GameSettings;
using Newtonsoft.Json;
using System.Collections.Generic;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings.Interfaces;
using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Website.Interfaces;
using Genrpg.Shared.DataStores.PlayerData;
using Genrpg.Shared.Users.PlayerData;

namespace Genrpg.Shared.Accounts.WebApi.Login
{
    [MessagePackObject]
    public class LoginResponse : IWebResponse
    {
        [Key(0)] public User User { get; set; }
        [Key(1)] public List<CharacterStub> CharacterStubs { get; set; } = new List<CharacterStub>();
        [Key(2)] public List<MapStub> MapStubs { get; set; } = new List<MapStub>();
        [JsonProperty(TypeNameHandling = TypeNameHandling.Auto)]
        [Key(3)] public List<ITopLevelSettings> GameData { get; set; } = new List<ITopLevelSettings>();
        [Key(4)] public string LoginToken { get; set; }
        [Key(5)] public List<IUnitData> UserData { get; set; } = new List<IUnitData>();
    }
}
