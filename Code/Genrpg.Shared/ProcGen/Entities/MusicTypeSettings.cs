using Genrpg.Shared.Audio.Entities;
using Genrpg.Shared.DataStores.GameSettings;
using Genrpg.Shared.GameSettings.Loading;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.ProcGen.Entities
{
    [MessagePackObject]
    public class MusicTypeSettings : ParentSettings<MusicType>
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override List<MusicType> Data { get; set; } = new List<MusicType>();

        public MusicType GetMusicType(long idkey) { return _lookup.Get<MusicType>(idkey); }
    }

    [MessagePackObject]
    public class MusicTypeSettingsApi : ParentSettingsApi<MusicTypeSettings, MusicType> { }
    [MessagePackObject]
    public class MusicTypeSettingsLoader : ParentSettingsLoader<MusicTypeSettings, MusicType, MusicTypeSettingsApi> { }


}
