using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.ProcGen.Settings.Fences;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.ProcGen.Settings.Fences
{
    [MessagePackObject]
    public class FenceType : ChildSettings, IIndexedGameItem
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string ParentId { get; set; }
        [Key(2)] public long IdKey { get; set; }
        [Key(3)] public override string Name { get; set; }
        [Key(4)] public string Desc { get; set; }
        [Key(5)] public string Icon { get; set; }
        [Key(6)] public string Art { get; set; }
        [Key(7)] public float Length { get; set; }

        public FenceType()
        {
            Art = "Fence";
            Length = 6;
        }

    }
    [MessagePackObject]
    public class FenceTypeSettings : ParentSettings<FenceType>
    {
        [Key(0)] public override string Id { get; set; }
    }

    [MessagePackObject]
    public class FenceTypeSettingsApi : ParentSettingsApi<FenceTypeSettings, FenceType> { }
    [MessagePackObject]
    public class FenceTypeSettingsLoader : ParentSettingsLoader<FenceTypeSettings, FenceType> { }

    [MessagePackObject]
    public class FenceSettingsMapper : ParentSettingsMapper<FenceTypeSettings, FenceType, FenceTypeSettingsApi> { }

}
