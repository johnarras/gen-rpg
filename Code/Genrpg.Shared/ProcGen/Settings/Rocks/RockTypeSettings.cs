using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.ProcGen.Settings.Rocks;
using Genrpg.Shared.Utils.Data;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.ProcGen.Settings.Rocks
{
    /// <summary>
    /// Plants found on the ground used in Unity's grass terrain generator
    /// </summary>

    [MessagePackObject]
    public class RockType : ChildSettings, IIndexedGameItem
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string ParentId { get; set; }
        [Key(2)] public long IdKey { get; set; }
        [Key(3)] public override string Name { get; set; }
        [Key(4)] public string Desc { get; set; }
        [Key(5)] public string Icon { get; set; }

        [Key(6)] public string Art { get; set; }

        [Key(7)] public float ChanceScale { get; set; }

        [Key(8)] public int MaxPerZone { get; set; }

        [Key(9)] public MyColorF BaseColor { get; set; }

        [Key(10)] public int MaxIndex { get; set; }

        public RockType()
        {
            ChanceScale = 1.0f;

            MaxPerZone = 0;
            BaseColor = new MyColorF();

            MaxIndex = 1;
        }

    }
    [MessagePackObject]
    public class RockTypeSettings : ParentSettings<RockType>
    {
        [Key(0)] public override string Id { get; set; }
    }

    [MessagePackObject]
    public class RockTypeSettingsApi : ParentSettingsApi<RockTypeSettings, RockType> { }
    [MessagePackObject]
    public class RockTypeSettingsLoader : ParentSettingsLoader<RockTypeSettings, RockType> { }

    [MessagePackObject]
    public class RockSettingsMapper : ParentSettingsMapper<RockTypeSettings, RockType, RockTypeSettingsApi> { }


}
