
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Genrpg.Shared.Utils;
using Genrpg.Shared.ProcGen.Entities;
using System.Threading;
using Genrpg.Shared.Zones.Settings;
using Genrpg.Shared.ProcGen.Settings.Locations;
using Genrpg.Shared.Zones.WorldData;

public class AddRoadDips : BaseZoneGenerator
{
    public override async UniTask Generate(CancellationToken token)
    {
        await base.Generate(token);

        foreach (Zone zone in _mapProvider.GetMap().Zones)
        {
            GenerateOne(zone, _gameData.Get<ZoneTypeSettings>(_gs.ch).Get(zone.ZoneTypeId), zone.XMin, zone.ZMin, zone.XMax, zone.ZMax);
        }
    }

    public void GenerateOne(Zone zone, ZoneType zoneType, int startx, int starty, int endx, int endy)
    { 
        if (zone == null || zoneType == null || startx >= endx || starty >= endy)
        {
            return;
        }

        GenZone genZone = _md.GetGenZone(zone.IdKey);

		float dipScale = genZone.RoadDipScale*zoneType.RoadDipScale;

        int dx = endx - startx + 1;
        int dy = endy - starty + 1;

        float maxSize = Math.Max(dx, dy);
        MyRandom rand = new MyRandom(zone.Seed % 23422423);

        List<float[,]> noises = new List<float[,]>();

        int noiseTimes = 1;

        for (int i = 0; i < noiseTimes; i++)
        {
            float freq = MathUtils.FloatRange(0.03f, 0.10f, rand) * maxSize * 1.5f;
            float amp = MathUtils.FloatRange(0.1f, 0.3f, rand) * 4;
            float pers = MathUtils.FloatRange(0.2f, 0.5f, rand);
            int octaves = 2;

            float[,] noise = _noiseService.Generate(pers, freq, amp, octaves, rand.Next(), dx, dy);
            noises.Add(noise);
        }

        int zoneRad = 3;

		int maxDist = MathUtils.IntRange(6, 11, rand);

		dipScale *= 1.0f * maxDist / 6.0f;

        for (int x = startx; x < endx; x++)
		{
			for (int y = starty; y < endy; y++)
			{

                Location loc = _zoneGenService.FindMapLocation(x, y, 5);

                if (loc != null)
                {
                    continue;
                }


                float hx = 1.0f * x / _mapProvider.GetMap().GetHwid();
                float hy = 1.0f * y / _mapProvider.GetMap().GetHhgt();
           
                float wallDistScale = MathUtils.Clamp(0, _md.mountainDistPercent[x,y], 1);

                if (_md.mapZoneIds[x,y] != zone.IdKey)
                {
                    continue;
                }

                double closestOtherZoneDist = zoneRad;

                for (int xx = x-zoneRad; xx <= x+zoneRad; xx++)
                {
                    if (xx < 0 || xx >= _mapProvider.GetMap().GetHwid())
                    {
                        continue;
                    }

                    for (int yy = y - zoneRad; yy <= y+zoneRad; yy++)
                    {
                        if (yy < 0 || yy >= _mapProvider.GetMap().GetHhgt())
                        {
                            continue;
                        }
                        if (_md.mapZoneIds[xx,yy] != zone.IdKey)
                        {
                            double dist = Math.Sqrt((x - xx) * (x - xx) + (y - yy) * (y - yy));
                            if (dist < closestOtherZoneDist)
                            {
                                closestOtherZoneDist = dist;
                            }
                        }
                    }
                }

                float distToRoad = _md.roadDistances[x,y];

				if (distToRoad > maxDist)
				{
					continue;
				}
				float pct = 1;

				if (distToRoad > 0)
				{
					pct = MathUtils.Clamp(0, 1-distToRoad/maxDist, 1);
                }
                if (closestOtherZoneDist < zoneRad)
                {
                    pct *= (float)(Math.Pow(closestOtherZoneDist / zoneRad, 2));
                }

                for (int i = 0; i < noises.Count; i++)
                {
                    pct *= (1.0f + noises[i][x - startx, y - starty]);
                }

              

                float val = MapConstants.RoadDipHeight * pct * dipScale * wallDistScale;

				_md.heights[x,y] -= Math.Abs(val);
				
			}
		}
	}
}
	
