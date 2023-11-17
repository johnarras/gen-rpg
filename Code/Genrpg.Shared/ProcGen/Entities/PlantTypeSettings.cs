using Genrpg.Shared.DataStores.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.ProcGen.Entities
{
    [MessagePackObject]
    public class PlantTypeSettings : ParentSettings<PlantType>
    {
        [Key(0)] public override string Id { get; set; }

        public PlantType GetPlantType(long idkey) { return _lookup.Get<PlantType>(idkey); }
    }

    [MessagePackObject]
    public class PlantTypeSettingsApi : ParentSettingsApi<PlantTypeSettings, PlantType> { }
    [MessagePackObject]
    public class PlantTypeSettingsLoader : ParentSettingsLoader<PlantTypeSettings, PlantType, PlantTypeSettingsApi> { }

}
