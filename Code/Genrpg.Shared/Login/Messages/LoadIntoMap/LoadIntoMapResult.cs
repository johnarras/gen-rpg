using MessagePack;

using System.Collections.Generic;
using Newtonsoft.Json;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.Shared.Characters.Entities;
using Genrpg.Shared.DataStores.Interfaces;
using Genrpg.Shared.Login.Interfaces;
using Genrpg.Shared.Networking.Constants;
using Genrpg.Shared.GameSettings.Entities;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings.Interfaces;

namespace Genrpg.Shared.Login.Messages.LoadIntoMap
{
    [MessagePackObject]
    public class LoadIntoMapResult : ILoginResult
    {
        [Key(0)] public Map Map { get; set; }
        [Key(1)] public Character Char { get; set; }
        [Key(2)] public bool Generating { get; set; }
        [Key(3)] public string Host { get; set; }
        [Key(4)] public long Port { get; set; }
        [JsonProperty(TypeNameHandling = TypeNameHandling.Auto)]
        [Key(5)] public List<IUnitData> CharData { get; set; } = new List<IUnitData>();
        [Key(6)] public List<IGameSettings> GameData { get; set; } = new List<IGameSettings>();

        [Key(7)] public EMapApiSerializers Serializer { get; set; } = EMapApiSerializers.Json;

        [Key(8)] public GameDataOverrideList OverrideList { get; set; }

        public LoadIntoMapResult()
        {
        }
    }
}
