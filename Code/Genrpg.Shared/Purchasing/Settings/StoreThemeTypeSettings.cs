using Genrpg.Shared.DataStores.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.Interfaces;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Purchasing.Settings
{
    [MessagePackObject]
    public class StoreThemeTypeSettings : ParentSettings<StoreThemeType>
    {
        [Key(0)] public override string Id { get; set; }

        public StoreThemeType GetStoreThemeType(long idkey) { return _lookup.Get<StoreThemeType>(idkey); }
    }

    [MessagePackObject]
    public class StoreThemeTypeSettingsApi : ParentSettingsApi<StoreThemeTypeSettings, StoreThemeType> { }
    [MessagePackObject]
    public class StoreThemeTypeSettingsLoader : ParentSettingsLoader<StoreThemeTypeSettings, StoreThemeType, StoreThemeTypeSettingsApi> { }


    [MessagePackObject]
    public class StoreThemeType : ChildSettings, IIndexedGameItem
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string ParentId { get; set; }
        [Key(2)] public long IdKey { get; set; }
        [Key(3)] public override string Name { get; set; }
        [Key(4)] public string Desc { get; set; }
        [Key(5)] public string Art { get; set; }
        [Key(6)] public string Icon { get; set; }
    }
}
