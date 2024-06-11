
using System.Collections.Generic;
using Genrpg.Shared.Utils;
using Genrpg.Shared.MapServer.Entities;

using System.Linq;
using System.Threading;
using Genrpg.Shared.MapServer.Constants;
using Genrpg.Shared.Zones.WorldData;
using UnityEngine;

public class SetupOverrideTerrainPatches : BaseZoneGenerator
{
    public override async Awaitable Generate (CancellationToken token)
    {
        await base.Generate(token);
        int wid = _mapProvider.GetMap().GetHwid();
		int hgt = _mapProvider.GetMap().GetHhgt();

        MyRandom rand = new MyRandom(_mapProvider.GetMap().Seed % 1000000000 + 132873);

        for (int times = 0; times < 3; times++)
        {
            float amp = MathUtils.FloatRange(1.2f, 1.8f, rand)*1.4f;

            float freq = wid * MathUtils.FloatRange(0.015f, 0.025f, rand) * 1.0f;

            int octaves = 3;

            float pers = MathUtils.FloatRange(0.35f, 0.45f, rand);

            int pseed = rand.Next();

            float[,] heights = _noiseService.Generate(pers, freq, amp, octaves, pseed, wid, hgt);

            for (int x = 0; x < wid; x++)
            {
                for (int z = 0; z < hgt; z++)
                {
                    _md.subZonePercents[x, z] += heights[x, z];
                }
            }
        }

        for (int x = 0; x < wid; x++)
        {
            for (int z = 0; z < hgt; z++)
            {
                _md.subZonePercents[x, z] = MathUtils.Clamp(0, 2*(_md.subZonePercents[x, z]-0.5f), 1);
            }
        }


        List<Zone> procGenZones = _mapProvider.GetMap().Zones.Where(x=>x.IdKey >= SharedMapConstants.MinBaseZoneId && x.IdKey <= SharedMapConstants.MaxBaseZoneId).ToList();
        for (int x = 0; x < wid; x++)
        {
            for (int z = 0; z < hgt; z++)
            {
                if (_md.subZonePercents[x,z] > 0 && _md.subZoneIds[x,z] == 0)
                {
                    Zone currZone = GetZoneAt(_mapProvider.GetMap(), x, z);
                    List<Zone> okZones = procGenZones.Where(x => x.ZoneTypeId != currZone.ZoneTypeId).ToList();
                    if (okZones.Count < 1)
                    {
                        continue;
                    }
                    long subZoneId = okZones[_rand.Next() % okZones.Count].IdKey;

                    if (rand.NextDouble() < 0.8f) // Most subzones are not added for now
                    {
                        subZoneId = 0;
                    }

                    FloodFillRegion((int)subZoneId, x, z, 0);
                }
            }
        }
    }

    public Zone GetZoneAt(Map map, int x, int y)
    {
        if (_mapProvider.GetMap() == null || _md == null ||
            _md.mapZoneIds == null || x < 0 || y < 0 || x >= _md.mapZoneIds.GetLength(0) || y >= _md.mapZoneIds.GetLength(1))
        {
            return null;
        }

        return map.Get<Zone>(_md.mapZoneIds[x, y]);
    }

    protected void FloodFillRegion(int zoneId, int x, int z, int depth)
    {
        if (x < 0 || x >= _mapProvider.GetMap().GetHwid() || z < 0 || z >= _mapProvider.GetMap().GetHhgt() || depth >= 100)
        {
            return;
        }
        if (_md.subZonePercents[x, z] > 0 && _md.subZoneIds[x, z] == 0)
        {
            _md.subZoneIds[x, z] = zoneId;
            _md.subZonePercents[x, z] = 0;
        }
        else
        {
            return;
        }

        FloodFillRegion(zoneId, x - 1, z, depth+1);
        FloodFillRegion(zoneId, x + 1, z, depth+1);
        FloodFillRegion(zoneId, x, z - 1, depth+1);
        FloodFillRegion(zoneId, x, z + 1, depth+1);
    }
}
	
