using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.MapServer.Entities.MapCache
{

    [MessagePackObject]
    public class CachedMapInstance
    {
        [Key(0)] public string InstanceId { get; set; }
        [Key(1)] public string Host { get; set; }
        [Key(2)] public long Port { get; set; }
    }
}
