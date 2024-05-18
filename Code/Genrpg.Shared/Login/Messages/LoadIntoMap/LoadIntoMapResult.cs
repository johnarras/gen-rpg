using MessagePack;

using System.Collections.Generic;
using Newtonsoft.Json;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.Shared.Login.Interfaces;
using Genrpg.Shared.Networking.Constants;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings.Interfaces;
using Genrpg.Shared.DataStores.PlayerData;
using Genrpg.Shared.GameSettings.PlayerData;
using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Purchasing.PlayerData;

namespace Genrpg.Shared.Login.Messages.LoadIntoMap
{
    [MessagePackObject]
    public class LoadIntoMapResult : ILoginResult
    {
        [Key(0)] public Map Map { get; set; }
        [Key(1)] public CoreCharacter Char { get; set; }
        [Key(2)] public bool Generating { get; set; }
        [Key(3)] public string Host { get; set; }
        [Key(4)] public long Port { get; set; }
        [JsonProperty(TypeNameHandling = TypeNameHandling.Auto)]
        [Key(5)] public List<IUnitData> CharData { get; set; } = new List<IUnitData>();
        [JsonProperty(TypeNameHandling = TypeNameHandling.Auto)]
        [Key(6)] public List<ITopLevelSettings> GameData { get; set; } = new List<ITopLevelSettings>();

        [Key(7)] public EMapApiSerializers Serializer { get; set; } = EMapApiSerializers.Json;

        [Key(8)] public GameDataOverrideList OverrideList { get; set; }

        [Key(9)] public string WorldDataEnv { get; set; }

        [Key(10)] public PlayerStoreOfferData Stores { get; set; }

        public LoadIntoMapResult()
        {
        }
    }
}
