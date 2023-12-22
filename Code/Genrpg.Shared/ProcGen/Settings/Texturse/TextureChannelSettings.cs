using Genrpg.Shared.DataStores.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.ProcGen.Settings.Texturse
{
    [MessagePackObject]
    public class TextureChannelSettings : ParentSettings<TextureChannel>
    {
        [Key(0)] public override string Id { get; set; }

        public TextureChannel GetTextureChannel(long idkey) { return _lookup.Get<TextureChannel>(idkey); }
    }

    [MessagePackObject]
    public class TextureChannelSettingsApi : ParentSettingsApi<TextureChannelSettings, TextureChannel> { }
    [MessagePackObject]
    public class TextureChannelSettingsLoader : ParentSettingsLoader<TextureChannelSettings, TextureChannel, TextureChannelSettingsApi> { }

}
