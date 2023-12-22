using MessagePack;
using Genrpg.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Genrpg.Shared.Zones.Entities;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.MapServer.Constants;
using System.Linq;
using Genrpg.Shared.Zones.WorldData;
using Genrpg.Shared.Zones.Settings;

namespace Genrpg.Shared.ProcGen.Entities
{
    [MessagePackObject]
    public class GenZone
    {
        [Key(0)] public long IdKey { get; set; }
        [Key(1)] public float RoadDipScale { get; set; }

        [Key(2)] public float RoadDirtScale { get; set; }

        [Key(3)] public float GrassDensity { get; set; }

        [Key(4)] public float GrassFreq { get; set; }

        [Key(5)] public float TreeDensity { get; set; }

        [Key(6)] public float TreeFreq { get; set; }

        [Key(7)] public float BushDensity { get; set; }

        [Key(8)] public float BushFreq { get; set; }

        [Key(9)] public float RockDensity { get; set; }

        [Key(10)] public float RockFreq { get; set; }

        [Key(11)] public float DetailFreq { get; set; }

        [Key(12)] public float DetailAmp { get; set; }

        [Key(13)] public float SpreadChance { get; set; }

        [Key(14)] public List<ZoneRockType> RockTypes { get; set; } = new List<ZoneRockType>();
        [Key(15)] public List<ZoneTreeType> TreeTypes { get; set; } = new List<ZoneTreeType>();

        [Key(16)] public List<ZoneRelation> ZonesNearLevel { get; set; } = new List<ZoneRelation>();
        [Key(17)] public List<ZoneRelation> ZonesNearPos { get; set; } = new List<ZoneRelation>();


        public ZoneTreeType GetTree(int treeTypeId)
        {
            if (TreeTypes == null)
            {
                return null;
            }

            for (int t = 0; t < TreeTypes.Count; t++)
            {
                if (TreeTypes[t].TreeTypeId == treeTypeId)
                {
                    return TreeTypes[t];
                }
            }
            return null;
        }

        public void AddNearbyZone(GameState gs, Zone zone, float distance)
        {

            if (zone == null || zone.ZoneTypeId < SharedMapConstants.MapZoneStartId)
            {
                return;
            }

            if (ZonesNearPos == null)
            {
                ZonesNearPos = new List<ZoneRelation>();
            }

            ZoneRelation currNearby = ZonesNearPos.FirstOrDefault(x => x.ZoneId == zone.ZoneTypeId);

            if (currNearby != null)
            {
                return;
            }


            ZonesNearPos.Add(new ZoneRelation() { ZoneId = zone.IdKey, Offset = distance });
        }
    }
}
