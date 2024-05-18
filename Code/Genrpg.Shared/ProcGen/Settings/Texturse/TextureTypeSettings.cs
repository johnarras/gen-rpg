using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.Interfaces;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.ProcGen.Settings.Texturse
{
    [MessagePackObject]
    public class TextureType : ChildSettings, IIndexedGameItem
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string ParentId { get; set; }
        [Key(2)] public long IdKey { get; set; }
        [Key(3)] public override string Name { get; set; }
        [Key(4)] public string Desc { get; set; }
        [Key(5)] public string Icon { get; set; }

        [Key(6)] public string Art { get; set; }

        [Key(7)] public float Size { get; set; }

        [Key(8)] public long ParentTextureTypeId { get; set; }


        public TextureType()
        {
        }
    }
    [MessagePackObject]
    public class TextureTypeSettings : ParentSettings<TextureType>
    {
        [Key(0)] public override string Id { get; set; }
    }

    [MessagePackObject]
    public class TextureTypeSettingsApi : ParentSettingsApi<TextureTypeSettings, TextureType> { }
    [MessagePackObject]
    public class TextureTypeSettingsLoader : ParentSettingsLoader<TextureTypeSettings, TextureType> { }

    [MessagePackObject]
    public class TextureSettingsMapper : ParentSettingsMapper<TextureTypeSettings, TextureType, TextureTypeSettingsApi> { }


}
