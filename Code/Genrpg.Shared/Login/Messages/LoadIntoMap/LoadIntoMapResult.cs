using MessagePack;

using System.Collections.Generic;
using Newtonsoft.Json;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.Shared.Characters.Entities;
using Genrpg.Shared.DataStores.Interfaces;
using Genrpg.Shared.Login.Interfaces;
using Genrpg.Shared.Networking.Constants;

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
        [Key(5)] public List<IUnitData> CharData { get; set; }

        [Key(6)] public EMapApiSerializers Serializer { get; set; } = EMapApiSerializers.Json;

        public LoadIntoMapResult()
        {
            CharData = new List<IUnitData>();
        }
    }
}
