using MessagePack;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.Shared.Spawns.WorldData;
using Genrpg.Shared.Website.Interfaces;

namespace Genrpg.Shared.MapServer.WebApi.UploadMap
{
    [MessagePackObject]
    public class UploadMapRequest : IClientUserRequest
    {
        [Key(0)] public Map Map { get; set; }
        [Key(1)] public MapSpawnData SpawnData { get; set; }
        [Key(2)] public string WorldDataEnv { get; set; }
    }
}
