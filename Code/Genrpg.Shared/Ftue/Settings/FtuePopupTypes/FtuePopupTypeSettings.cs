using Genrpg.Shared.DataStores.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.Interfaces;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Ftue.Settings.FtuePopupTypes
{

    [MessagePackObject]
    public class FtuePopupType : ChildSettings, IIndexedGameItem
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string ParentId { get; set; }
        [Key(2)] public long IdKey { get; set; }
        [Key(3)] public override string Name { get; set; }
        [Key(4)] public string Desc { get; set; }
        [Key(5)] public string Icon { get; set; }
        [Key(6)] public string Art { get; set; }
    }

    [MessagePackObject]
    public class FtuePopupTypeSettings : ParentSettings<FtuePopupType>
    {
        [Key(0)] public override string Id { get; set; }
    }

    [MessagePackObject]
    public class FtuePopupTypeSettingsApi : ParentSettingsApi<FtuePopupTypeSettings, FtuePopupType> { }
    [MessagePackObject]
    public class FtuePopupTypeSettingsLoader : ParentSettingsLoader<FtuePopupTypeSettings, FtuePopupType, FtuePopupTypeSettingsApi> { }

}
