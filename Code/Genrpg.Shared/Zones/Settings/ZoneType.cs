using MessagePack;
using Genrpg.Shared.Interfaces;
using System.Collections.Generic;
using Genrpg.Shared.DataStores.GameSettings;
using Genrpg.Shared.Names.Settings;
using Genrpg.Shared.Spawns.Settings;
using Genrpg.Shared.Zones.Entities;

namespace Genrpg.Shared.Zones.Settings
{
    [MessagePackObject]
    public class ZoneType : ChildSettings, IIndexedGameItem, IMusicRegion
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string ParentId { get; set; }
        [Key(2)] public long IdKey { get; set; }
        [Key(3)] public override string Name { get; set; }
        [Key(4)] public string Desc { get; set; }
        [Key(5)] public string Icon { get; set; }

        [Key(6)] public float GenChance { get; set; }

        [Key(7)] public float GrassDensity { get; set; }
        [Key(8)] public float GrassFreq { get; set; }
        [Key(9)] public float TreeDensity { get; set; }
        [Key(10)] public float TreeFreq { get; set; }
        [Key(11)] public float BushDensity { get; set; }
        [Key(12)] public float BushFreq { get; set; }
        [Key(13)] public float RockDensity { get; set; }
        [Key(14)] public float RockFreq { get; set; }
        [Key(15)] public float DetailAmp { get; set; }
        [Key(16)] public float DetailFreq { get; set; }

        [Key(17)] public float RoadDetailScale { get; set; }
        [Key(18)] public float RoadDipScale { get; set; }
        [Key(19)] public float RoadDirtScale { get; set; }

        /// <summary>
        /// Chance this is generated when creating a new map in the list of zones.
        /// </summary>
        [Key(20)] public float ZoneListGenScale { get; set; }


        [Key(21)] public List<ZoneTextureType> Textures { get; set; }

        [Key(22)] public List<WeightedName> ZoneNames { get; set; }
        [Key(23)] public List<WeightedName> ZoneAdjectives { get; set; }

        [Key(24)] public List<ZoneBridgeType> BridgeTypes { get; set; }
        [Key(25)] public List<ZoneFenceType> FenceTypes { get; set; }
        [Key(26)] public List<ZoneRockType> RockTypes { get; set; }
        [Key(27)] public List<ZoneTreeType> TreeTypes { get; set; }

        [Key(28)] public List<WeightedName> CreatureNamePrefixes { get; set; }
        [Key(29)] public List<WeightedName> CreatureDoubleNamePrefixes { get; set; }

        [Key(30)] public List<ZoneTypeOverride> Overrides { get; set; }
        [Key(31)] public List<SpawnItem> UnitSpawns { get; set; }


        [Key(32)] public string PlantChoices { get; set; }

        [Key(33)] public float FenceChance { get; set; }


        [Key(34)] public string Art { get; set; }

        [Key(35)] public long WeatherTypeId { get; set; }


        [Key(36)] public float CreviceCountScale { get; set; }
        [Key(37)] public float CreviceDepthScale { get; set; }
        [Key(38)] public float CreviceWidthScale { get; set; }

        [Key(39)] public long MusicTypeId { get; set; }
        [Key(40)] public long AmbientMusicTypeId { get; set; }

        public ZoneType()
        {
            ClearLists();
        }


        public void SlimForClient()
        {
            ClearLists();
        }
        private void ClearLists()
        {
            Textures = new List<ZoneTextureType>();
            ZoneNames = new List<WeightedName>();
            ZoneAdjectives = new List<WeightedName>();

            BridgeTypes = new List<ZoneBridgeType>();
            FenceTypes = new List<ZoneFenceType>();
            RockTypes = new List<ZoneRockType>();

            CreatureNamePrefixes = new List<WeightedName>();
            CreatureDoubleNamePrefixes = new List<WeightedName>();

            UnitSpawns = new List<SpawnItem>();
            Overrides = new List<ZoneTypeOverride>();
        }
    }
}
