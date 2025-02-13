
using System;
using System.Collections.Generic;


using Genrpg.Shared.Utils;
using Genrpg.Shared.Zones.Entities;

using System.Threading;
using Genrpg.Shared.ProcGen.Entities;
using Genrpg.Shared.Zones.Settings;
using Genrpg.Shared.Zones.WorldData;
using UnityEngine;

public class AddMountainTextures : BaseZoneGenerator
{

    public const float MinSteepness = 15f;
	public const float MaxSteepness = 70f;
	public const float MaxSteepnessPerturbDelta = 15f;


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

        if (zone == null || zoneType == null)
        {
            return;
        }

        if (startx < 0 || starty < 0 || endx > _md.awid || endy > _md.ahgt || startx >= endx || starty >= endy)
        {
            return;
        }
        int wid = endx - startx+1;
        int hgt = endy - starty+1;
        int maxSize = Math.Max(wid, hgt);
        float[,,] alphas = _md.alphas;
        int size = Math.Max(Math.Max(wid, hgt), MapConstants.DefaultHeightmapSize);

        float perlinScale = 1.0f;
        int perlinSize = MapConstants.DefaultNoiseSize;

        if (maxSize > perlinSize)
        {
            perlinScale = 1.0f * maxSize / perlinSize;
            perlinSize = maxSize;
        }

        MyRandom steepRandom = new MyRandom(zone.Seed+9934);

		// We look at 100 points near a point to determine howmany are higher than
		// the central point. If the amount is >= 100/2+this number below, then
		// we show the cleft splat, otherwise we don't
		int extraRaisedPointsPercent = 1;

		// Some zones just use the regular steepness splats along the walls.
		if (steepRandom.Next () % 20 < 2)
		{
			return;
		}



		float wh = MapConstants.MinLandHeight/MapConstants.MapHeight;
       

        float minFreqMult = 0.05f;
        float maxFreqMult = 0.4f;
      

		MyRandom edgeRand = new MyRandom(zone.Seed%1000000000+_mapProvider.GetMap().Seed%1000000000+3474342);
		MyRandom texRand = new MyRandom(zone.Seed+5423+_mapProvider.GetMap().Seed/5);
        MyRandom capRand = new MyRandom(zone.Seed % 373823332 + 23423);

        float capFreq = MathUtils.FloatRange(minFreqMult, maxFreqMult, capRand) * size;
        float capAmp = MathUtils.FloatRange(0.0f,40.0f, capRand);
        float capPers = MathUtils.FloatRange(0.2f, 0.4f, capRand);
        int capOctaves = 2;
        float[,] capSteepnessDeltas = _noiseService.Generate(capPers, capFreq, capAmp, capOctaves, capRand.Next(), size, size);


        MyRandom randRand = new MyRandom(zone.Seed % 233534543 + 34534);
        float randFreq = MathUtils.FloatRange(minFreqMult,maxFreqMult, randRand) * size;
        float randAmp = MathUtils.FloatRange(0.0f, 0.4f, randRand);
        float randPers = MathUtils.FloatRange(0.2f, 0.5f, randRand);
        float[,] randomBaseAmounts = _noiseService.Generate(randPers, randFreq, randAmp, 2, randRand.Next() % 100000000, size, size);

        randRand = new MyRandom(zone.Seed % 233534 + 34532434);
        randFreq = MathUtils.FloatRange(minFreqMult,maxFreqMult, randRand) * size;
        randAmp = MathUtils.FloatRange(0.0f, 0.4f, randRand);
        randPers = MathUtils.FloatRange(0.2f, 0.5f, randRand);
        float[,] randomCleftAmounts = _noiseService.Generate(randPers, randFreq, randAmp, 2, randRand.Next() % 100000000, size, size);

        float coreSteepnessWallPercent = MathUtils.FloatRange(0.7f, 0.99f, randRand);
        float innerWallSizePercent = MathUtils.FloatRange(0.95f, 0.98f, randRand);

        int extraAboveMidNeeded = randRand.NextDouble() < 0.3f ? 1 : 0;

        List<int> mainChoices = new List<int>();

		for (int i = 0; i < 2; i++)
		{
			mainChoices.Add (MapConstants.BaseTerrainIndex);
		}
		for (int i = 0; i < 2; i++)
		{
			mainChoices.Add (MapConstants.DirtTerrainIndex);
		}
		for (int i = 0; i < 16; i++)
		{
			mainChoices.Add (MapConstants.SteepTerrainIndex);
		}

		int choice = texRand.Next() % mainChoices.Count;

		int mainTex = mainChoices[choice];

		List<int> cleftChoices = new List<int>();

		if (mainTex != MapConstants.BaseTerrainIndex)
		{
			for (int i = 0; i < 2; i++)
			{
				cleftChoices.Add (MapConstants.BaseTerrainIndex);
			}
		}
		if (mainTex != MapConstants.DirtTerrainIndex)
		{
			for (int i = 0; i < 16; i++)
			{
				cleftChoices.Add (MapConstants.DirtTerrainIndex);
			}
		}
		if (mainTex != MapConstants.SteepTerrainIndex)
		{
			for (int i = 0; i < 4; i++)
			{
				cleftChoices.Add (MapConstants.SteepTerrainIndex);
			}
		}


		int cleftChoice = texRand.Next () % cleftChoices.Count;
		
		int cleftTex = cleftChoices[cleftChoice];

		// Now get the opposite texture that shows up every so often.

		List<int> oppChoices = new List<int>();
		int[] allOppChoices = new int[] 
		{ 
			MapConstants.BaseTerrainIndex,
			MapConstants.DirtTerrainIndex,
			MapConstants.SteepTerrainIndex,
		};

		foreach (int opp in allOppChoices)
		{
			if (opp != mainTex && opp != cleftTex)
			{
				oppChoices.Add (opp);
			}
		}

		int oppChoice = texRand.Next () % oppChoices.Count;

		int oppTex = oppChoices[oppChoice];

        // Do we cap mountains with base terrain
        bool useCaps = texRand.NextDouble () < 0.9f;

        if (useCaps)
		{
			if (mainTex == MapConstants.BaseTerrainIndex)
			{
				mainTex = MapConstants.SteepTerrainIndex;
				cleftTex = MapConstants.BaseTerrainIndex;
			}
			else
			{
				cleftTex = MapConstants.BaseTerrainIndex;
			}

			oppTex = MapConstants.BaseTerrainIndex;
		}



		// How steep can things be and still have cap?
		float startMinSteepnessForCap = MathUtils.FloatRange(MinSteepness,MaxSteepness, texRand);

        startMinSteepnessForCap = 10;

        // How long it takes the textures to transition from wall texture to
        // flat cap texture.
        float capTransitionRange = MathUtils.FloatRange (10,20,texRand);

		// These are the current values for the main/cleft and opp textures.
		float currMainPercent = 0;
		float currCleftPercent = 0;
		float currOppPercent = 0;

		float minCoreSteepness = 0;
		float minEdgeSteepness = 30;

		for (int x = startx; x < endx; x++)
		{
			for (int y = starty; y < endy; y++)
			{
                float wallDist = 0.0f;
                if (_md.mountainHeights == null ||
                    _md.mountainDistPercent[x,y] >= 1.0f ||
                    _md.mapZoneIds[x,y] != zone.IdKey)
                {
                    continue;
                }

                wallDist = _md.mountainDistPercent[x, y];

                float minSteepnessForCap = MathUtils.Clamp(MinSteepness - MaxSteepnessPerturbDelta,
					startMinSteepnessForCap + capSteepnessDeltas[x-startx, y-starty],
					MaxSteepness + MaxSteepnessPerturbDelta);

				if (_md.heights[x,y] < wh)
				{
					continue;
				}

				float steep = _terrainManager.GetSteepness(y,x);

				// Steepness works in 2 stages.
				// Within the "Core" wall, not the extra blended edge, the steepness
				// has to be at least minCoreSteepness (Very small, currently 0.
				// However, along the edge, the steepness has to be greater so we don't
				// get long pits of this stuff.
				if ((wallDist < coreSteepnessWallPercent && steep < minCoreSteepness) ||
				    (wallDist >= coreSteepnessWallPercent && steep < minEdgeSteepness))
				{
					//continue;
				}

				int rad = 6;

				
				if (alphas[x,y,MapConstants.RoadTerrainIndex] > 0.0f)
				{
					continue;
				}
				float roadPct = 1 - MathUtils.GetSmoothScalePercent(rad / 3, rad, _md.roadDistances[x, y]);


                if (_zoneGenService.FindMapLocation(x,y,2) != null)
                {
                    continue;
                }
                

				currMainPercent = 1;
				currCleftPercent = 0;
				currOppPercent = 0;

                if (useCaps && steep < minSteepnessForCap)
                {
                    float pct = MathUtils.Clamp(0, (minSteepnessForCap - steep) / capTransitionRange,
                                               MathUtils.FloatRange(0.4f, 1.0f, steepRandom));
                    currCleftPercent = pct;
                    currMainPercent = 0;
                    currOppPercent = 1 - pct;
                }
                else
                {
                    float midhgt = _terrainManager.GetInterpolatedHeight(y, x);

                    int numAngles = 100;
                    int innerMinNumAboveMid = numAngles / 2 + extraRaisedPointsPercent;
                    int innerNumAboveMid = 0;
                    int outerNumAboveMid = 0;
                    float innerExtraHeight = 0.2f;
                    float innerrad = 1.3f;
                    float outerrad = 3.0f;

                    for (int i = 0; i < numAngles; i++)
                    {
                        float cosx = (float)Math.Cos(Math.PI * 2.0f * i / numAngles);
                        float sinx = (float)Math.Sin(Math.PI * 2.0f * i / numAngles);

                        float xx = x + innerrad * cosx;
                        float yy = y + innerrad * sinx;
                        float xyhgt = _terrainManager.GetInterpolatedHeight(yy, xx);
                        if (xyhgt > midhgt + innerExtraHeight)
                        {
                            innerNumAboveMid++;
                        }

                        xx = x + outerrad * cosx;
                        yy = y + outerrad * sinx;
                        xyhgt = _terrainManager.GetInterpolatedHeight(yy, xx);
                        if (xyhgt > midhgt + innerExtraHeight)
                        {
                            outerNumAboveMid++;
                        }
                    }


                    int extraNumMid1 = innerNumAboveMid - innerMinNumAboveMid;
                    int extraNumMid2 = outerNumAboveMid - innerMinNumAboveMid;
                    int extraNumMid = Math.Max(extraNumMid1, extraNumMid2);
                    if (extraNumMid >= extraAboveMidNeeded)
                    {

                        int ahx = Math.Abs(x - (startx+wid/2));
                        int ahy = Math.Abs(y - (starty+hgt/2));

                        //if (Math.Abs(ahx - ahy) > 3)
                        {
                            float pct = MathUtils.FloatRange(0.2f, 1.0f, steepRandom) * extraNumMid * 0.50f;
                            pct = Math.Min(0.90f, pct);
                            if (pct > 0.6f)
                            {
                                pct = MathUtils.FloatRange(1 - pct, pct, steepRandom);
                            }
                            currOppPercent = 0;
                            currMainPercent = 1 - pct;
                            currCleftPercent = pct;
                        }
                    }
                    else
                    {
                        if (false && useCaps && extraNumMid1 < 0)
                        {
                            currMainPercent = 1;
                            currOppPercent = 0;
                            currCleftPercent = 0;
                           

                        }
                        else
                        {

                            currOppPercent = Math.Abs(randomBaseAmounts[x - startx, y - starty]);
                            currCleftPercent = Math.Abs(randomBaseAmounts[x - startx, y - starty]);
                            if (currOppPercent + currCleftPercent > 1)
                            {
                                currOppPercent /= (currOppPercent + currCleftPercent);
                                currCleftPercent /= (currOppPercent + currCleftPercent);
                                currMainPercent = 0;
                            }
                            else
                            {
                                currMainPercent = 1 - currOppPercent - currCleftPercent;
                            }
                        }
                    }
                }

                float wallpercent = _md.mountainDistPercent[x, y];

				// Near edge blend things.
				float origPercent = 0.0f;
				if (wallDist > innerWallSizePercent)
				{
					float edgePercent = (wallDist-innerWallSizePercent)/(1.0f-innerWallSizePercent);

					origPercent = MathUtils.FloatRange (0,(float)(edgePercent*1.2f),edgeRand);
					origPercent = MathUtils.Clamp(0,origPercent,1);
				}

				if (roadPct > 0)
				{
                    float mult = 1.0f;
					origPercent = Math.Min (1,Math.Max (origPercent,roadPct*mult));
				}
                // Rescale existing splats.
                for (int c = 0; c < MapConstants.MaxTerrainIndex; c++)
                {
                    _md.alphas[x, y, c] *= origPercent;
                }

                // Now rescale new alphas.
                currMainPercent *= (1 - origPercent);
                currCleftPercent *= (1 - origPercent);
                currOppPercent *= (1 - origPercent);


                _md.alphas[x, y, mainTex] += currMainPercent;
                _md.alphas[x, y, cleftTex] += currCleftPercent;
                _md.alphas[x, y, oppTex] += currOppPercent;
            }
		}
	}
}
	
