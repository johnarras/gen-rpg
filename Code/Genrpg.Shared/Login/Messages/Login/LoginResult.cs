using MessagePack;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Users.Entities;
using Newtonsoft.Json;
using System.Collections.Generic;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.Shared.Login.Interfaces;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings.Interfaces;
using Genrpg.Shared.Characters.PlayerData;

namespace Genrpg.Shared.Login.Messages.Login
{
    [MessagePackObject]
    public class LoginResult : ILoginResult
    {
        [Key(0)] public User User { get; set; }
        [Key(1)] public List<CharacterStub> CharacterStubs { get; set; }
        [Key(2)] public List<MapStub> MapStubs { get; set; }
        [JsonProperty(TypeNameHandling = TypeNameHandling.Auto)]
        [Key(3)] public List<ITopLevelSettings> GameData { get; set; } = new List<ITopLevelSettings>();
    }
}
