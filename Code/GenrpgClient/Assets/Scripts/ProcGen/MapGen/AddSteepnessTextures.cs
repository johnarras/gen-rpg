
using System;

using Genrpg.Shared.Utils;
using System.Threading;
using Genrpg.Shared.Zones.Settings;
using Genrpg.Shared.Zones.WorldData;
using UnityEngine;

public class AddSteepnessTextures : BaseZoneGenerator
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
        
        if (startx >= endx || starty >= endy)
        {
            return;
        }
        float[,,] alphas = _md.alphas;

        startx = MathUtils.Clamp(0, startx, _mapProvider.GetMap().GetHwid() - 1);
        endx = MathUtils.Clamp(0, endx, _mapProvider.GetMap().GetHwid() - 1);
        starty = MathUtils.Clamp(0, starty, _mapProvider.GetMap().GetHhgt() - 1);
        endy = MathUtils.Clamp(0, endy, _mapProvider.GetMap().GetHhgt() - 1);


        int maxLen = Math.Max(endx - startx, endy - starty);

        int width = endx - startx + 1;
        int height = endy - starty + 1;

        MyRandom steepRandom = new MyRandom(zone.Seed+zone.IdKey +99434);
        bool useCleftDirt = false;
		if (steepRandom.Next () % 10 != 0)
		{
            useCleftDirt = true;
		}


        // We look at 100 points near a point to determine howmany are higher than
        // the central point. If the amount is >= 100/2+this number below, then
        // we show the cleft splat, otherwise we don't
        int extraRaisedPointsAmount = 1 + steepRandom.NextDouble() < 0.1f ? 1 : 0;

		float randomDirtChance = MathUtils.FloatRange(0.04f,0.20f,steepRandom);
		float minRandomDirtPercent = MathUtils.FloatRange (0.1f,0.5f,steepRandom);
		float maxRandomDirtPercent = MathUtils.FloatRange (0.6f,0.9f,steepRandom);


        MyRandom detailRand = new MyRandom(zone.Seed/5+582934);
       
        float ssaoFreq = MathUtils.FloatRange(0.3f, 0.7f, detailRand) * maxLen;
        float ssaoAmp = MathUtils.FloatRange(0.05f, 0.4f, detailRand);
        float ssaoPers = MathUtils.FloatRange(0.3f, 0.6f, detailRand);
        int ssaoOctaves = 3;

        float[,] cleftDirtNoise = _noiseService.Generate(ssaoPers, ssaoFreq, ssaoAmp, ssaoOctaves, detailRand.Next(), width, height);


        float midFreq = MathUtils.FloatRange(0.3f, 0.7f, detailRand) * maxLen;
        float midAmp = MathUtils.FloatRange(1.0f, 1.5f, detailRand);
        float midPers = MathUtils.FloatRange(0.2f, 0.3f, detailRand);
        int midOCtaves = 2;

        float[,] midNoise = _noiseService.Generate(midPers, midFreq, midAmp, midOCtaves, detailRand.Next(), width, height);


        int numCheck = 0;
        int numBadZone = 0;
        int numGoodZone = 0;
		for (int x = startx; x <= endx; x++)
		{
			for (int y = starty; y <= endy; y++)
			{
                numCheck++;
                if (_md.mapZoneIds[x, y] != zone.IdKey)
                {
                    numBadZone++;
                    continue;
                }
                numGoodZone++;

                float edgePct = _md.EdgeHeightmapAdjustPercent(_mapProvider.GetMap(), x, y);

				float steep = _terrainManager.GetSteepness(y,x);
				float roadPercent= alphas[x,y,MapConstants.RoadTerrainIndex];

				if (roadPercent > 0) // && steep < _md.MinSteepnessForTexture*1.5f)
				{
					continue;
				}

                steep *= (float)(Math.Pow(edgePct, 0.08f));
				
				if (steep >= MapConstants.MinSteepnessForTexture)
				{
					float currSteep = steep*MathUtils.FloatRange(0.8f,1.0f,steepRandom);
					// Grass starts at 20 and drops.
					float groundPercent = MathUtils.Clamp(0,1.0f-Math.Abs (currSteep-MapConstants.MinSteepnessForTexture)*0.05f,1);
					// Dirt is a triangle that maxes at 0.35
					float dirtPercent = MathUtils.Clamp (0,1.0f-Math.Abs(currSteep-(MapConstants.MinSteepnessForTexture+15))*0.15f,1);
					// Rock starts at 0 and slowly rises.
					float steepPercent = MathUtils.Clamp (0,0.03f*Math.Abs (currSteep-MapConstants.MinSteepnessForTexture),1);

					if (steepPercent > 0)
					{
						if (steepRandom.NextDouble () < 0.20f)
						{
							dirtPercent += MathUtils.FloatRange (0.0f,0.6f,steepRandom);
						}
					}

					float percentPerturb = 0.2f;
					groundPercent*=MathUtils.FloatRange(1-percentPerturb,1+percentPerturb,steepRandom);
					dirtPercent*=MathUtils.FloatRange(1-percentPerturb,1+percentPerturb,steepRandom);
					steepPercent*=MathUtils.FloatRange(1-percentPerturb,1+percentPerturb,steepRandom);


					if (useCleftDirt)
					{

                        float midhgt = _terrainManager.SampleHeight(y, x);
							
						int numAngles = 40;
						int innerMinNumAboveMid = numAngles/2+extraRaisedPointsAmount;
						int innerNumAboveMid = 0;
						float innerExtraHeight = 0.1f/MapConstants.MapHeight;
						float innerrad = 1.3f/_md.awid;
							
						for (int i = 0; i < numAngles; i++)
						{
							float cosx = (float)Math.Cos (Math.PI*2.0f*i/numAngles);
							float sinx = (float)Math.Sin (Math.PI*2.0f*i/numAngles);

							float xx = x+innerrad*cosx;
							float yy = y+innerrad*sinx;
                            float xyhgt = _terrainManager.SampleHeight(yy, xx);
							if (xyhgt > midhgt+innerExtraHeight)
							{
								innerNumAboveMid++;
							}
								
						}


                        int currInnerMinNumAboveMid = innerMinNumAboveMid;

                        float currMidPerturb = midNoise[x - startx, y - starty];
						
						// Adjust how many above/below are needed.
						if (currMidPerturb <= -1)
                        {
                            currInnerMinNumAboveMid--;
                        }

                        if (currMidPerturb >= 1)
                        {
                            currInnerMinNumAboveMid++;
                        }

                        if (innerNumAboveMid >= currInnerMinNumAboveMid)
						{
                            float currDirtNoise = cleftDirtNoise[x - startx, y - starty];


                            float newDirtAmount = MathUtils.FloatRange(0.4, (1 - dirtPercent) * 0.9f, steepRandom);
							newDirtAmount *= (1 + currDirtNoise);
							newDirtAmount += (innerNumAboveMid - currInnerMinNumAboveMid) * MathUtils.FloatRange(0, 0.2f, steepRandom);
							dirtPercent += newDirtAmount;
                            if (dirtPercent > 1)
                            {
                                dirtPercent = 1;
                            }
                        }
					}

					if (steepRandom.NextDouble () < randomDirtChance)
					{
						dirtPercent += MathUtils.FloatRange (minRandomDirtPercent,maxRandomDirtPercent,steepRandom);
						groundPercent /= 2;
						steepPercent /= 2;
					}
                    roadPercent /= 2;
					float total = groundPercent+dirtPercent+steepPercent+roadPercent;
					if (total > 0)
					{
						groundPercent/= total;
						dirtPercent /= total;
						steepPercent /= total;
						roadPercent /= total;
					}
                    else
                    {
                        groundPercent = 1.0f;
                        dirtPercent = 0;
                        steepPercent = 0;
                        roadPercent = 0;
                    }
					// Add some mixture of dirt and whatnot in there.

					_md.ClearAlphasAt(x,y);
					alphas[x,y,MapConstants.BaseTerrainIndex] = groundPercent;
					alphas[x,y,MapConstants.SteepTerrainIndex] = steepPercent;
					alphas[x,y,MapConstants.RoadTerrainIndex] = roadPercent;
					alphas[x,y,MapConstants.DirtTerrainIndex] = dirtPercent;
				}
			}
		}
	}
}
	
