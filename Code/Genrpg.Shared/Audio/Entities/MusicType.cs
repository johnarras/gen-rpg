using MessagePack;
using Genrpg.Shared.Interfaces;
using System;

namespace Genrpg.Shared.Audio.Entities
{
    [MessagePackObject]
    public class MusicType : IIndexedGameItem
    {
        
        [Key(0)] public long IdKey { get; set; }
        [Key(1)] public string Name { get; set; }
        [Key(2)] public string Desc { get; set; }
        [Key(3)] public string Icon { get; set; }
        [Key(4)] public string Art { get; set; }


        [Key(5)] public float RandomizeSeconds { get; set; }
    }
}
