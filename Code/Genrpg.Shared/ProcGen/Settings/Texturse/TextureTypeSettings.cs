using Genrpg.Shared.DataStores.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.ProcGen.Settings.Texturse
{
    [MessagePackObject]
    public class TextureTypeSettings : ParentSettings<TextureType>
    {
        [Key(0)] public override string Id { get; set; }

        public TextureType GetTextureType(long idkey) { return _lookup.Get<TextureType>(idkey); }
    }

    [MessagePackObject]
    public class TextureTypeSettingsApi : ParentSettingsApi<TextureTypeSettings, TextureType> { }
    [MessagePackObject]
    public class TextureTypeSettingsLoader : ParentSettingsLoader<TextureTypeSettings, TextureType, TextureTypeSettingsApi> { }

}
