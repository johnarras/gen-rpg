using MessagePack;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.MapServer.Constants;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Utils;
using System.Reflection.Emit;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.DataStores.Categories.WorldData;
using Genrpg.Shared.ProcGen.Constants;
using Genrpg.Shared.ProcGen.Settings.Locations;

namespace Genrpg.Shared.Zones.WorldData
{

    [MessagePackObject]
    public class Zone : BaseWorldData, IIndexedGameItem, IStringOwnerId
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public string OwnerId { get; set; }

        [Key(2)] public long IdKey { get; set; }
        [Key(3)] public long ZoneTypeId { get; set; }


        [Key(4)] public string Name { get; set; }
        [Key(5)] public string Desc { get; set; }
        [Key(6)] public string Icon { get; set; }
        [Key(7)] public long Seed { get; set; }


        [Key(8)] public long BaseTextureTypeId { get; set; }
        [Key(9)] public long DirtTextureTypeId { get; set; }
        [Key(10)] public long RockTextureTypeId { get; set; }
        [Key(11)] public long RoadTextureTypeId { get; set; }

        [Key(12)] public string MapId { get; set; }

        [Key(13)] public long Level { get; set; }

        [Key(14)] public string Art { get; set; }

        [Key(15)] public int XMin { get; set; }
        [Key(16)] public int ZMin { get; set; }
        [Key(17)] public int XMax { get; set; }
        [Key(18)] public int ZMax { get; set; }

        [Key(19)] public List<Location> Locations { get; set; }
        [Key(20)] public List<ZoneUnitStatus> Units { get; set; }
        [Key(21)] public List<ZonePlantType> PlantTypes { get; set; }

        public override void Delete(IRepositorySystem repoSystem) { repoSystem.Delete(this); }
        public Zone()
        {
            Locations = new List<Location>();
            BaseTextureTypeId = TextureTypes.Grass;
            DirtTextureTypeId = TextureTypes.Dirt;
            RockTextureTypeId = TextureTypes.Rock;
            RoadTextureTypeId = TextureTypes.Road;

            CleanData();
        }

        public void CleanForClient()
        {
            CleanData();
        }

        private void CleanData()
        {
            Units = new List<ZoneUnitStatus>();
            foreach (Location loc in Locations)
            {
                loc.CleanForClient();
            }

            
        }

        public Location GetLocation(int id)
        {
            if (Locations == null)
            {
                return null;
            }

            for (int l = 0; l < Locations.Count; l++)
            {
                if (Locations[l].Id == id)
                {
                    return Locations[l];
                }
            }
            return null;
        }

        public ZoneUnitStatus GetUnit(long unitTypeId)
        {
            if (Units == null)
            {
                Units = new List<ZoneUnitStatus>();
            }

            ZoneUnitStatus unit = Units.FirstOrDefault(x => x.UnitTypeId == unitTypeId);
            if (unit != null)
            {
                return unit;
            }
            ZoneUnitStatus zc = new ZoneUnitStatus();
            zc.UnitTypeId = unitTypeId;
            Units.Add(zc);
            return zc;

        }

        public ZonePlantType GetPlant(int plantTypeId)
        {
            if (PlantTypes == null)
            {
                return null;
            }

            for (int p = 0; p < PlantTypes.Count; p++)
            {
                if (PlantTypes[p].PlantTypeId == plantTypeId)
                {
                    return PlantTypes[p];
                }
            }
            return null;
        }

        public string GetTitle(GameState gs)
        {
            return Name + " [#" + IdKey + "]";
        }

        public long GetFinalUnitLevel(GameState gs, float x, float z, long startLevel)
        {
            float dmaxx = XMax - x;
            float dmaxz = ZMax - z;
            float dminx = XMin - x;
            float dminz = ZMin - z;

            float distFromMax = dmaxx * dmaxx + dmaxz * dmaxz;
            float distFromMin = dminx * dminx + dminz * dminz;

            float totalDist = distFromMax + distFromMin;

            if (totalDist > 1)
            {
                float minPct = distFromMin / totalDist;
                float maxPct = 1 - minPct;

                long levelOffset = (int)(4 * (maxPct - minPct) + gs.rand.Next(-1, 1));
                return MathUtils.Clamp(1, startLevel + levelOffset, gs.map.MaxLevel);
            }
            return 1;
        }


        public long GetTerrainTextureByIndex(int index)
        {
            if (index == 1)
            {
                return DirtTextureTypeId;
            }
            else if (index == 2)
            {
                return RoadTextureTypeId;
            }
            else if (index == 3)
            {
                return RockTextureTypeId;
            }
            return BaseTextureTypeId;
        }
    }
}
