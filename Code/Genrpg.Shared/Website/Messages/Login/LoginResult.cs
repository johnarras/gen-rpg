using MessagePack;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Users.Entities;
using Newtonsoft.Json;
using System.Collections.Generic;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings.Interfaces;
using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Website.Interfaces;

namespace Genrpg.Shared.Website.Messages.Login
{
    [MessagePackObject]
    public class LoginResult : IWebResult
    {
        [Key(0)] public User User { get; set; }
        [Key(1)] public List<CharacterStub> CharacterStubs { get; set; } = new List<CharacterStub>();
        [Key(2)] public List<MapStub> MapStubs { get; set; } = new List<MapStub>();
        [JsonProperty(TypeNameHandling = TypeNameHandling.Auto)]
        [Key(3)] public List<ITopLevelSettings> GameData { get; set; } = new List<ITopLevelSettings>();
    }
}