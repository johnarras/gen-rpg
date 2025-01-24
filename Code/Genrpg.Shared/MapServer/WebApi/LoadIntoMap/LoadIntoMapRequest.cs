using MessagePack;
using Genrpg.Shared.Website.Interfaces;

namespace Genrpg.Shared.MapServer.WebApi.LoadIntoMap
{
    [MessagePackObject]
    public class LoadIntoMapRequest : IClientUserRequest
    {
        [Key(0)] public string Env { get; set; }
        [Key(1)] public string MapId { get; set; }
        [Key(2)] public string InstanceId { get; set; }
        [Key(3)] public string CharId { get; set; }
        [Key(4)] public bool GenerateMap { get; set; }
        [Key(5)] public string WorldDataEnv { get; set; }
    }
}
