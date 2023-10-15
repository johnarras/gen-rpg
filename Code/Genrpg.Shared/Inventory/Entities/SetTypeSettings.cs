using Genrpg.Shared.DataStores.GameSettings;
using Genrpg.Shared.GameSettings.Loading;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Inventory.Entities
{
    [MessagePackObject]
    public class SetTypeSettings : ParentSettings<SetType>
    {
        [Key(0)] public override string Id { get; set; }

        public SetType GetSetType(long idkey) { return _lookup.Get<SetType>(idkey); }
    }

    [MessagePackObject]
    public class SetTypeSettingsApi : ParentSettingsApi<SetTypeSettings, SetType> { }
    [MessagePackObject]
    public class SetTypeSettingsLoader : ParentSettingsLoader<SetTypeSettings, SetType, SetTypeSettingsApi> { }

}
