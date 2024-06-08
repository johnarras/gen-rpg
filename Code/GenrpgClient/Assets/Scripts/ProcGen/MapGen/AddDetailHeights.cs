
using System;
using Cysharp.Threading.Tasks;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Zones.Entities;
using System.Threading;
using Genrpg.Shared.ProcGen.Entities;
using Genrpg.Shared.Zones.Settings;
using Genrpg.Shared.Zones.WorldData;

public class AddDetailHeights : BaseZoneGenerator
{

    public override async UniTask Generate(CancellationToken token)
    {
        await base.Generate(token);
        foreach (Zone zn in _mapProvider.GetMap().Zones)
        {
            GenerateOneZone(zn, _gameData.Get<ZoneTypeSettings>(_gs.ch).Get(zn.ZoneTypeId), zn.XMin, zn.ZMin, zn.XMax, zn.ZMax);
        }
    }

    public void GenerateOneZone (Zone zone, ZoneType zoneType, int startx, int starty, int endx, int endy)
    {
        if (zone == null || zoneType == null)
        {
            return;
        }

        if (startx < 0)
        {
            startx = 0;
        }

        if (starty < 0)
        {
            starty = 0;
        }

        if (endx >= _mapProvider.GetMap().GetHwid())
        {
            endx = _mapProvider.GetMap().GetHwid()-1;
        }

        if (endy >= _mapProvider.GetMap().GetHhgt())
        {
            endy = _mapProvider.GetMap().GetHhgt()-1;
        }

        if (endx <= startx || endy <= starty)
        {
            return;
        }

        if (_zoneGenService == null)
        {
            return;
        }

        int wid = endx - startx;
        int hgt = endy - starty;

        int awid = _md.awid;
        int ahgt = _md.ahgt;


        MyRandom rand = new MyRandom(zone.Seed+_mapProvider.GetMap().Seed/7);

        GenZone genZone = _md.GetGenZone(zone.IdKey);
		float hillAmplitudeScale = 1.0f;
        hillAmplitudeScale *= genZone.DetailAmp;
        hillAmplitudeScale *= zoneType.DetailAmp;
		if (hillAmplitudeScale < 0.1f)
        {
            hillAmplitudeScale = 0.1f;
        }

        float hillFrequencyScale = 1.0f;
        hillFrequencyScale *= genZone.DetailFreq;
        hillFrequencyScale *= zoneType.DetailFreq;
		if (hillFrequencyScale < 0.1f)
        {
            hillFrequencyScale = 0.1f;
        }


        float perlinScale = 1.0f;

        int perlinSize = MapConstants.DefaultNoiseSize;

        int maxSize = Math.Max(wid, hgt);
        if (maxSize > MapConstants.DefaultNoiseSize)
        {
            perlinScale = 1.0f*maxSize / MapConstants.DefaultNoiseSize;
            perlinSize = maxSize;
        }
        
		float startFreq = perlinSize*hillFrequencyScale*0.009f;
        // Amplitude for details increases a bit as the size of the zone increases.
        float startAmp = 0.0150f * (float)(Math.Pow(perlinScale, 0.2f));

        
        float ampDelta = 0.35f;
        float freqDelta = 0.35f;

		float pers = 0.40f; float amp = startAmp; float freq = startFreq; int octaves = 2;


		float startExp = 0.0f;
		
		long pseed = zone.Seed+34543;
        freq = startFreq * MathUtils.FloatRange(1 - freqDelta, 1 + freqDelta, rand);
        amp = startAmp * MathUtils.FloatRange(1 - ampDelta, 1 + ampDelta, rand);

        float extraScale = 1.0f;

        extraScale = MathUtils.FloatRange(0.9f, 1.3f, rand);
        freq /= extraScale;
        amp *= extraScale;

		float exp = startExp* MathUtils.FloatRange(0.5f, 1.5f, rand);
       
        amp *= 0.95f;
        pers *= 1.05f;

        float[,] heightsUp = _noiseService.Generate(pers,freq,amp,octaves,pseed,perlinSize,perlinSize, exp);
		
		
        freq = startFreq * MathUtils.FloatRange(1 - freqDelta, 1 + freqDelta, rand)*0.45f;
        amp = startAmp * MathUtils.FloatRange(1 - ampDelta, 1 + ampDelta, rand) * 1.45f;
        exp = startExp * MathUtils.FloatRange(0.5f, 1.5f, rand);

        extraScale = MathUtils.FloatRange(0.9f, 1.3f, rand);
        freq /= extraScale;
        amp *= extraScale;

        float[,] heightsUp2 = _noiseService.Generate (pers,freq,amp,octaves,pseed/6+21412,perlinSize,perlinSize, exp);
		
		
		

        freq = startFreq * MathUtils.FloatRange(1 - freqDelta, 1 + freqDelta, rand)*0.9f;
        amp = startAmp * MathUtils.FloatRange(1 - ampDelta, 1 + ampDelta, rand) * 1.1f;
        exp = startExp * MathUtils.FloatRange(0.5f, 1.5f, rand);


        extraScale = MathUtils.FloatRange(0.9f, 1.3f, rand);
        freq /= extraScale;
        amp *= extraScale;

        float[,] heightsDown = _noiseService.Generate(pers,freq,amp,octaves,rand.Next(),perlinSize,perlinSize,exp);


        float effPers = MathUtils.FloatRange(0.05f, 0.2f, rand);
        float effFreq = MathUtils.FloatRange(0.05f, 0.10f, rand)*perlinSize;
        float effAmp = MathUtils.FloatRange(0.04f, 0.12f, rand);

        float[,] roadEffectPercent = _noiseService.Generate(effPers, effFreq, effAmp, 2, rand.Next(), perlinSize,perlinSize);

        float[,,] alphamaps = _md.alphas;

		float roadAffectedPercent = zoneType.RoadDetailScale*0.15f;


        float detailMult = 1.0f;


        float startRad = 40;


        float radPers = MathUtils.FloatRange(0.1f, 0.4f, rand);
        float radFreq = MathUtils.FloatRange(0.02f, 0.1f, rand) * perlinSize;
        float radAmp = MathUtils.FloatRange(0.3f, 0.6f, rand)*startRad;

        float[,] radValues = _noiseService.Generate(radPers, radFreq, radAmp, 2, rand.Next(), perlinSize, perlinSize);


        float startPower = 1.5f;

        float powerPers = MathUtils.FloatRange(0.1f, 0.4f, rand);
        float powerFreq = MathUtils.FloatRange(0.02f, 0.1f, rand) * perlinSize;
        float powerAmp = MathUtils.FloatRange(0.3f, 0.6f, rand);

        float[,] powerValues = _noiseService.Generate(powerPers, powerFreq, powerAmp, 2, rand.Next(), perlinSize, perlinSize);

        int numTries = 0;

		for (int x = 0; x < wid; x++)
		{
			for (int y = 0; y < hgt; y++)
            { 

                if (heightsUp[x,y] < 0)
                {
                    heightsUp[x,y] /= 4;
                }

                if (heightsDown[x,y] < 0)
                {
                    heightsDown[x,y] = 0;
                }

                if (heightsUp2[x,y] < 0)
                {
                    heightsUp2[x,y] /= 4;
                }

                heightsUp2[x,y] *= detailMult;

                int wx = x + startx;  int wy = y + starty;

                float roadHeightMult = 1.0f;
				
				
				if (false && alphamaps[wx,wy,MapConstants.RoadTerrainIndex] > 0.12f)
				{
					roadHeightMult = 0;
				}
				else
				{
                    float roadDist = _md.roadDistances[wx, wy];



                    float rad = MathUtils.Clamp(startRad / 2, startRad + radValues[x, y], MapConstants.MaxRoadCheckDistance);

                    if (roadDist < rad)
                    {
                        float scaleDown = roadDist / rad;
                        float currPower = MathUtils.Clamp(1.0f, startPower + powerValues[x, y], 2.0f);
                        roadHeightMult *= (float)(Math.Pow(scaleDown, currPower));

                      
                    }
				}
                if (roadHeightMult > 1)
                {
                    roadHeightMult = 1;
                }

                if (roadHeightMult < roadAffectedPercent)
                {
                    if (roadHeightMult <= 0)
                    {
                        roadHeightMult = roadAffectedPercent / 5;
                    }
                    else
                    {
                        roadHeightMult = roadAffectedPercent;
                    }
                }

                float edgeSmoothMult = 1.0f;

                int rad2 = 15;
                int totalNum = 0;
                int numOtherNearby = 0;    
                if (wx >= rad2 && wx < _mapProvider.GetMap().GetHwid()-rad2-1 && wy >= rad2 && wy <= _mapProvider.GetMap().GetHhgt()-rad2-1)
                {
                    double minDist = 100000;
                    for (int xx = wx-rad2; xx <= wx+rad2; xx++)
                    {
                        for (int yy = wy-rad2; yy <= wy+rad2; yy++)
                        {
                            totalNum++;
                            if (_md.mapZoneIds[xx,yy] != zone.IdKey)
                            {
                                numOtherNearby++;
                                double newDist = Math.Sqrt((xx - wx) * (xx - wx) + (yy - wy) * (yy - wy));
                                if (newDist < minDist)
                                {
                                    minDist = newDist;
                                }
                            }
                        }
                    }

                    if (minDist <= 0)
                    {
                        edgeSmoothMult = 0;
                    }
                    else if (minDist < rad2)
                    {
                        edgeSmoothMult = (float)(Math.Pow(minDist / rad2, 1.5f));
                    }
                }
				
				float downHeight = heightsDown[x, y];
                numTries++;
                    

				float heightDiff = hillAmplitudeScale * (heightsUp[x, y] + heightsUp2[x, y] - downHeight);

                // The idea here is we scale down mountains and valleys if a road is there,
                // but the max scaledown is currently to 20% of the actual height.
                // But if the heightdiff for this detail is <= 0.02, we scale down this diff
                // so we look at Math.min(actualHeightdiff/0.02,1) to get how much we should really
                // scale down, and add the rest of the part back into the height mult.


                float worldEdgePercent = (float)Math.Pow(_md.EdgeHeightmapAdjustPercent(_mapProvider.GetMap(), wx, wy), 0.09f);


                float finalHeightDiff = heightDiff * roadHeightMult * edgeSmoothMult * worldEdgePercent;

                _md.heights[wx, wy] += finalHeightDiff;
			}
		}

	}

	

}
	
