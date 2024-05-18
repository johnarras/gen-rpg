using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using MessagePack;
using System.Linq;

namespace Genrpg.Editor.Entities.MetaData
{
    [MessagePackObject]
    public class EditorMetaDataSettings : ParentSettings<TypeMetaData>
    {
        [Key(0)] public override string Id { get; set; }


        public TypeMetaData GetMetaDataForType(string typeName)
        {
            return _data.FirstOrDefault(x => x.TypeName == typeName);
        }

    }

    [MessagePackObject]
    public class EditorMetaDataTypeSettingsApi : ParentSettingsApi<EditorMetaDataSettings, TypeMetaData> { }
    [MessagePackObject]
    public class EditorMetaDataTypeSettingsLoader : ParentSettingsLoader<EditorMetaDataSettings, TypeMetaData>
    {
    }

    [MessagePackObject]
    public class EditorMetaDataTypeSettingsMapper : ParentSettingsMapper<EditorMetaDataSettings, TypeMetaData, EditorMetaDataTypeSettingsApi>
    {
        public override bool SendToClient() { return false; }
    }

}
