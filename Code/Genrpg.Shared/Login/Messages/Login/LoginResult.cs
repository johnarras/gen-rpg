using MessagePack;
using Genrpg.Shared.Characters.Entities;
using Genrpg.Shared.DataStores.Categories;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Users.Entities;
using Newtonsoft.Json;
using System.Collections.Generic;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.Shared.Login.Interfaces;

namespace Genrpg.Shared.Login.Messages.Login
{
    [MessagePackObject]
    public class LoginResult : ILoginResult
    {
        [Key(0)] public User User { get; set; }
        [Key(1)] public List<CharacterStub> CharacterStubs { get; set; }
        [Key(2)] public List<MapStub> MapStubs { get; set; }
        [JsonProperty(TypeNameHandling = TypeNameHandling.Auto)]
        [Key(3)] public List<BaseGameData> Data { get; set; } = new List<BaseGameData>();
    }
}
