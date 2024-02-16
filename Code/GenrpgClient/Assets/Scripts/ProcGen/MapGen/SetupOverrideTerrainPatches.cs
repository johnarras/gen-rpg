
using System;
using System.Collections.Generic;
using GEntity = UnityEngine.GameObject;
using Cysharp.Threading.Tasks;

using Genrpg.Shared.Utils;
using Genrpg.Shared.MapServer.Entities;

using System.Text;
using Genrpg.Shared.Zones.Entities;
using System.Linq;
using System.Threading;
using Genrpg.Shared.MapServer.Constants;
using Genrpg.Shared.Zones.WorldData;
using Genrpg.Shared.Core.Entities;

public class SetupOverrideTerrainPatches : BaseZoneGenerator
{
    public override async UniTask Generate (UnityGameState gs, CancellationToken token)
    {
        await base.Generate(gs, token);
        int wid = gs.map.GetHwid();
		int hgt = gs.map.GetHhgt();

        MyRandom rand = new MyRandom(gs.map.Seed % 1000000000 + 132873);

        for (int times = 0; times < 3; times++)
        {
            float amp = MathUtils.FloatRange(1.2f, 1.8f, rand)*1.4f;

            float freq = wid * MathUtils.FloatRange(0.015f, 0.025f, rand) * 1.0f;

            int octaves = 3;

            float pers = MathUtils.FloatRange(0.35f, 0.45f, rand);

            int pseed = rand.Next();

            float[,] heights = _noiseService.Generate(gs, pers, freq, amp, octaves, pseed, wid, hgt);

            for (int x = 0; x < wid; x++)
            {
                for (int z = 0; z < hgt; z++)
                {
                    gs.md.subZonePercents[x, z] += heights[x, z];
                }
            }
        }

        for (int x = 0; x < wid; x++)
        {
            for (int z = 0; z < hgt; z++)
            {
                gs.md.subZonePercents[x, z] = MathUtils.Clamp(0, 2*(gs.md.subZonePercents[x, z]-0.5f), 1);
            }
        }


        List<Zone> procGenZones = gs.map.Zones.Where(x=>x.IdKey >= SharedMapConstants.MinBaseZoneId && x.IdKey <= SharedMapConstants.MaxBaseZoneId).ToList();
        for (int x = 0; x < wid; x++)
        {
            for (int z = 0; z < hgt; z++)
            {
                if (gs.md.subZonePercents[x,z] > 0 && gs.md.subZoneIds[x,z] == 0)
                {
                    Zone currZone = GetZoneAt(gs, gs.map, x, z);
                    List<Zone> okZones = procGenZones.Where(x => x.ZoneTypeId != currZone.ZoneTypeId).ToList();
                    if (okZones.Count < 1)
                    {
                        continue;
                    }
                    long subZoneId = okZones[gs.rand.Next() % okZones.Count].IdKey;

                    if (rand.NextDouble() < 0.8f) // Most subzones are not added for now
                    {
                        subZoneId = 0;
                    }

                    FloodFillRegion(gs, (int)subZoneId, x, z, 0);
                }
            }
        }
    }

    public Zone GetZoneAt(UnityGameState gs, Map map, int x, int y)
    {
        if (gs.map == null || gs.md == null ||
            gs.md.mapZoneIds == null || x < 0 || y < 0 || x >= gs.md.mapZoneIds.GetLength(0) || y >= gs.md.mapZoneIds.GetLength(1))
        {
            return null;
        }

        return map.Get<Zone>(gs.md.mapZoneIds[x, y]);
    }

    protected void FloodFillRegion(UnityGameState gs, int zoneId, int x, int z, int depth)
    {
        if (x < 0 || x >= gs.map.GetHwid() || z < 0 || z >= gs.map.GetHhgt() || depth >= 100)
        {
            return;
        }
        if (gs.md.subZonePercents[x, z] > 0 && gs.md.subZoneIds[x, z] == 0)
        {
            gs.md.subZoneIds[x, z] = zoneId;
            gs.md.subZonePercents[x, z] = 0;
        }
        else
        {
            return;
        }

        FloodFillRegion(gs, zoneId, x - 1, z, depth+1);
        FloodFillRegion(gs, zoneId, x + 1, z, depth+1);
        FloodFillRegion(gs, zoneId, x, z - 1, depth+1);
        FloodFillRegion(gs, zoneId, x, z + 1, depth+1);
    }
}
	
