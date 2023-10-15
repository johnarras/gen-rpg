using Genrpg.Shared.DataStores.GameSettings;
using Genrpg.Shared.GameSettings.Loading;
using Genrpg.Shared.Interfaces;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Inventory.Entities
{
    [MessagePackObject]
    public class QualityTypeSettings : ParentSettings<QualityType>
    {
        [Key(0)] public override string Id { get; set; }

        public QualityType GetQualityType(long idkey) { return _lookup.Get<QualityType>(idkey); }
    }

    [MessagePackObject]
    public class QualityTypeSettingsApi : ParentSettingsApi<QualityTypeSettings, QualityType> { }
    [MessagePackObject]
    public class QualityTypeSettingsLoader : ParentSettingsLoader<QualityTypeSettings, QualityType, QualityTypeSettingsApi> { }

}
