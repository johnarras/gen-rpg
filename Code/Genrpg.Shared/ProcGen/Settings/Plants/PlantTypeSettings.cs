using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.ProcGen.Settings.Plants;
using Genrpg.Shared.Utils.Data;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.ProcGen.Settings.Plants
{

    public class PlantFlags
    {
        public const int SmallPatches = 1 << 0;
        public const int UsePrefab = 1 << 1;
    }


    /// <summary>
    /// Plants found on the ground used in Unity's grass terrain generator
    /// </summary>

    [MessagePackObject]
    public class PlantType : ChildSettings, IIndexedGameItem
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string ParentId { get; set; }
        [Key(2)] public long IdKey { get; set; }
        [Key(3)] public override string Name { get; set; }
        [Key(4)] public string Desc { get; set; }
        [Key(5)] public string Icon { get; set; }

        [Key(6)] public string Art { get; set; }

        [Key(7)] public float MinScale { get; set; }
        [Key(8)] public float MaxScale { get; set; }

        [Key(9)] public MyColorF BaseColor { get; set; }

        [Key(10)] public int Flags { get; set; }
        public bool HasFlag(int flagBits) { return (Flags & flagBits) != 0; }
        public void AddFlags(int flagBits) { Flags |= flagBits; }
        public void RemoveFlags(int flagBits) { Flags &= ~flagBits; }

        public PlantType()
        {
            MinScale = 1.0f;
            MaxScale = 1.0f;

            BaseColor = new MyColorF();
        }

    }
    [MessagePackObject]
    public class PlantTypeSettings : ParentSettings<PlantType>
    {
        [Key(0)] public override string Id { get; set; }
    }

    [MessagePackObject]
    public class PlantTypeSettingsApi : ParentSettingsApi<PlantTypeSettings, PlantType> { }
    [MessagePackObject]
    public class PlantTypeSettingsLoader : ParentSettingsLoader<PlantTypeSettings, PlantType> { }

    [MessagePackObject]
    public class PlantSettingsMapper : ParentSettingsMapper<PlantTypeSettings, PlantType, PlantTypeSettingsApi> { }


}
