
using System;


using Genrpg.Shared.Utils;
using System.Threading;
using Genrpg.Shared.ProcGen.Entities;
using Genrpg.Shared.Zones.Settings;
using Genrpg.Shared.Zones.WorldData;
using UnityEngine;

public class AddRoadBorders : BaseZoneGenerator
{
    public override async Awaitable Generate(CancellationToken token)
    {

        await base.Generate(token);
        foreach (Zone zone in _mapProvider.GetMap().Zones)
        {
            GenerateOne(zone, _gameData.Get<ZoneTypeSettings>(_gs.ch).Get(zone.ZoneTypeId), zone.XMin, zone.ZMin, zone.XMax, zone.ZMax);
        }
    }

    public void GenerateOne (Zone zone, ZoneType zoneType, int startx, int starty, int endx, int endy)
    { 
        int dx = endx - startx+1;
        int dy = endy - starty+1;

        GenZone genZone = _md.GetGenZone(zone.IdKey);

		float dirtPercent = 
			MapConstants.RoadBorderBaseDirtPercent*
			genZone.RoadDirtScale *
			zoneType.RoadDirtScale;


		if (dirtPercent <= 0)
		{
			return;
		}

		MyRandom dirtRand = new MyRandom(_mapProvider.GetMap().Seed/19+zone.IdKey *23+zone.Seed+44663);


        int minDirtRadius = 1;

        float radpers = MathUtils.FloatRange(0.2f, 0.4f, dirtRand);
        float radfreq = MathUtils.FloatRange(0.4f, 1.5f, dirtRand) * (dx + dy) / 12.0f;
        float radamp = MathUtils.FloatRange(0.8f, 1.2f, dirtRand) * 8.0f;

        float[,] radNoise = _noiseService.Generate(radpers, radfreq, radamp, 2, zone.IdKey * 237 + zone.Seed / 13,dx,dy);

		float perturbPercent = MathUtils.FloatRange (0.3f,0.6f,dirtRand);


        float pers = MathUtils.FloatRange(0.2f, 0.4f, dirtRand);
        float freq = MathUtils.FloatRange(0.1f,0.3f,dirtRand)*(dx + dy) / 2.0f;
        float amp = MathUtils.FloatRange(0.3f, 0.8f, dirtRand);

        float[,] dirtAmount = _noiseService.Generate(pers, freq, amp, 2, zone.IdKey * 131 + zone.Seed / 7, dx, dy);


        float roadRadius = 2;

        if (dirtRand.NextDouble() < 0.2f)
        {
            roadRadius--;
        }

        for (int times = 0; times < 4; times++)
        {
            if (dirtRand.NextDouble() < 0.30f && roadRadius < 4)
            {
                roadRadius++;           
            }
            else
            {
                break;
            }
        }


		for (int x = startx; x < endx; x++)
		{
			for (int y = starty; y < endy; y++)
			{

                if (_md.mapZoneIds[x, y] != zone.IdKey)
                {
                    continue;
                }

				float roadVal = _md.alphas[x,y,MapConstants.RoadTerrainIndex];
				if (roadVal > 0)
				{
					continue;
				}

                roadRadius = (float)MathUtils.Clamp(minDirtRadius, Math.Abs(radNoise[x - startx, y - starty]), 6);

                float roadDist = _md.roadDistances[x, y];

				if (roadDist >= Math.Max(minDirtRadius,roadRadius) || roadDist <= 0f)
				{
					continue;
				}

                float roadDistScale = (float)Math.Pow(1 - roadDist / roadRadius,0.2f);

                if (roadDist <= 10)
                {
                    roadDistScale = 1.0f;
                }
                float distToRoad = _md.roadDistances[x, y];

                float posDirtPercent = 0.6f + Math.Abs(dirtAmount[x - startx, y - starty]);


                float roadPctNearby = 1 - MathUtils.GetSmoothScalePercent(0, roadRadius,distToRoad);
					

                float roadPctNearbyScale = (float)Math.Pow(roadPctNearby, 0.25f);
               
                if (distToRoad <= 1.5f)
                {
                    roadPctNearbyScale = Math.Max(roadPctNearbyScale, (2 - distToRoad) / 2);
                }
                roadPctNearbyScale = 1.0f;
				float currDirtPercent = roadDistScale*MathUtils.FloatRange(dirtPercent*(1-perturbPercent),dirtPercent*1,dirtRand);
                currDirtPercent *= roadPctNearbyScale;
				currDirtPercent = MathUtils.Clamp(0,currDirtPercent,0.75f);
                if (roadRadius > 3)
                {
                    currDirtPercent *= (1.01f * roadRadius);
                }
                currDirtPercent *= posDirtPercent;
                currDirtPercent = MathUtils.Clamp(0, currDirtPercent, 0.8f);


                for (int c = 0; c < MapConstants.MaxTerrainIndex; c++)
				{
					_md.alphas[x,y,c] *= (1-currDirtPercent);
				}
				_md.alphas[x,y,MapConstants.DirtTerrainIndex] += currDirtPercent;

			}
		}
	}
}
	
