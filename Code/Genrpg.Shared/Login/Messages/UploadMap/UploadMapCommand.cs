using MessagePack;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.Shared.Spawns.Entities;
using Genrpg.Shared.Login.Interfaces;

namespace Genrpg.Shared.Login.Messages.UploadMap
{
    [MessagePackObject]
    public class UploadMapCommand : ILoginCommand
    {
        [Key(0)] public Map Map { get; set; }
        [Key(1)] public MapSpawnData SpawnData { get; set; }
    }
}
