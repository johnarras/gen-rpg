using Genrpg.Shared.Currencies.Settings;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Inventory.Settings.ItemSets
{
    [MessagePackObject]
    public class SetTypeSettings : ParentSettings<SetType>
    {
        [Key(0)] public override string Id { get; set; }
    }

    [MessagePackObject]
    public class SetTypeSettingsApi : ParentSettingsApi<SetTypeSettings, SetType> { }
    [MessagePackObject]
    public class SetTypeSettingsLoader : ParentSettingsLoader<SetTypeSettings, SetType> { }

    [MessagePackObject]
    public class SetSettingsMapper : ParentSettingsMapper<SetTypeSettings, SetType, SetTypeSettingsApi> { }


}
