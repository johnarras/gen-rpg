
using System;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Entities;
using Genrpg.Shared.Utils;
using Genrpg.Shared.MapServer.Entities;
using Services.ProcGen;
using System.Text;
using Genrpg.Shared.Zones.Entities;
using System.Linq;
using System.Threading;
using Genrpg.Shared.MapServer.Constants;

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
                    gs.md.overrideZonePercents[x, z] += heights[x, z];
                }
            }
        }

        for (int x = 0; x < wid; x++)
        {
            for (int z = 0; z < hgt; z++)
            {
                gs.md.overrideZonePercents[x, z] = MathUtils.Clamp(0, 2*(gs.md.overrideZonePercents[x, z]-0.5f), 1);
            }
        }


        List<Zone> procGenZones = gs.map.Zones.Where(x=>x.IdKey >= SharedMapConstants.MinBaseZoneId && x.IdKey <= SharedMapConstants.MaxBaseZoneId).ToList();
        for (int x = 0; x < wid; x++)
        {
            for (int z = 0; z < hgt; z++)
            {
                if (gs.md.overrideZonePercents[x,z] > 0 && gs.md.overrideZoneIds[x,z] == 0)
                {
                    Zone currZone = gs.md.GetZoneAt(gs, gs.map, x, z);
                    List<Zone> okZones = procGenZones.Where(x => x.ZoneTypeId != currZone.ZoneTypeId).ToList();
                    if (okZones.Count < 1)
                    {
                        continue;
                    }
                    long overrideZoneId = okZones[gs.rand.Next() % okZones.Count].IdKey;
                    FloodFillRegion(gs, (int)overrideZoneId, x, z, 0);
                }
            }
        }
    }

    protected void FloodFillRegion(UnityGameState gs, int zoneId, int x, int z, int depth)
    {
        if (x < 0 || x >= gs.map.GetHwid() || z < 0 || z >= gs.map.GetHhgt() || depth >= 100)
        {
            return;
        }
        if (gs.md.overrideZonePercents[x, z] > 0 && gs.md.overrideZoneIds[x, z] == 0)
        {
            gs.md.overrideZoneIds[x, z] = zoneId;
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
	
