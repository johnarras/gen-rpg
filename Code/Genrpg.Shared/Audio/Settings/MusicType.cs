using MessagePack;
using Genrpg.Shared.Interfaces;
using System;
using Genrpg.Shared.DataStores.GameSettings;

namespace Genrpg.Shared.Audio.Settings
{
    [MessagePackObject]
    public class MusicType : ChildSettings, IIndexedGameItem
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string ParentId { get; set; }
        [Key(2)] public long IdKey { get; set; }
        [Key(3)] public override string Name { get; set; }
        [Key(4)] public string Desc { get; set; }
        [Key(5)] public string Icon { get; set; }
        [Key(6)] public string Art { get; set; }


        [Key(7)] public float RandomizeSeconds { get; set; }
    }
}
