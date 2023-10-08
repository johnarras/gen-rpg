using Genrpg.Shared.DataStores.GameSettings;
using Genrpg.Shared.GameSettings.Loading;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.ProcGen.Entities
{
    [MessagePackObject]
    public class TextureTypeSettings : ParentSettings<TextureType>
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override List<TextureType> Data { get; set; } = new List<TextureType>();

        public TextureType GetTextureType(long idkey) { return _lookup.Get<TextureType>(idkey); }
    }

    [MessagePackObject]
    public class TextureTypeSettingsApi : ParentSettingsApi<TextureTypeSettings, TextureType> { }
    [MessagePackObject]
    public class TextureTypeSettingsLoader : ParentSettingsLoader<TextureTypeSettings, TextureType, TextureTypeSettingsApi> { }

}
