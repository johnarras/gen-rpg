
using System;
using System.Collections.Generic;

using Genrpg.Shared.Utils;
using Genrpg.Shared.Utils.Data;
using System.Threading;
using Genrpg.Shared.ProcGen.Settings.Fences;
using Genrpg.Shared.ProcGen.Settings.Locations;
using Genrpg.Shared.Zones.Settings;
using Genrpg.Shared.Zones.WorldData;
using UnityEngine;

public class AddFences : BaseZoneGenerator
{
    public const int NumCentersAveraged = 8;

    const float MaxFenceHeightAngle = 20;

    public override async Awaitable Generate(CancellationToken token)
    {

        await base.Generate(token);
        foreach (Zone zone in _mapProvider.GetMap().Zones)
        {
            GenerateOne(zone, _gameData.Get<ZoneTypeSettings>(_gs.ch).Get(zone.ZoneTypeId), zone.XMin, zone.ZMin, zone.XMax, zone.ZMax);
        }
        
    }

    public void GenerateOne(Zone zone, ZoneType zoneType, int startx, int starty, int endx, int endy)
    {
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
            endx = _mapProvider.GetMap().GetHwid();
        }

        if (endy >= _mapProvider.GetMap().GetHhgt())
        {
            endy = _mapProvider.GetMap().GetHhgt();
        }

        List<MyPointF> fences = new List<MyPointF>();
        if (zoneType.FenceTypes == null || zoneType.FenceTypes.Count < 1)
        {
            return;
        }

        if (_gameData.Get<FenceTypeSettings>(_gs.ch).GetData() == null || _gameData.Get<FenceTypeSettings>(_gs.ch).GetData().Count < 1)
        {
            return;
        }

        MyRandom chanceRand = new MyRandom(zone.Seed % 100000000 + 234323);

        MyRandom choiceRand = new MyRandom(zone.Seed % 100000000 + 9972367);

        MyRandom placeRand = new MyRandom(zone.Seed % 82348732 + 33234);

        int distFromEnd = 6;

        List<MyPointF> currFences = new List<MyPointF>();

        float minDistToFence = 2.5f;

        int xsize = endx - startx + 1;
        int ysize = endy - starty + 1;

        float amp = MathUtils.FloatRange(0.0f, 0.3f, chanceRand);
        float freq = MathUtils.FloatRange(0.02f, 0.1f, chanceRand) * (xsize+ysize) / 2;
        float pers = MathUtils.FloatRange(0.2f, 0.45f, chanceRand);
        int octaves = 2;

        float[,] fenceChances = _noiseService.Generate(pers, freq, amp, octaves, chanceRand.Next(), xsize,ysize);



        float maxHeightAboveCenter = 1.0f;

        for (int x = startx + distFromEnd; x < endx - distFromEnd; x++)
        {
            int ddx = x - startx;
            for (int y = starty + distFromEnd; y < endy - distFromEnd; y++)
            {
                int ddy = y - starty;
                if (chanceRand.NextDouble() > fenceChances[ddx,ddy])
                {
                    Location currLoc = _zoneGenService.FindMapLocation(x, y, 3);

                    if (currLoc == null ||
                        FlagUtils.IsSet(_md.flags[x,y], MapGenFlags.IsLocationPatch))
                    {
                        continue;
                    }
                }

                if (_md.mapZoneIds[x, y] != zone.IdKey) // zoneobject
                {
                    continue;
                }
                float startRoadDist = _md.roadDistances[x,y];
                if (startRoadDist < 1.5f || startRoadDist > 2.5f)
                {
                    continue;
                }

                if (FlagUtils.IsSet(_md.flags[x, y], MapGenFlags.BelowWater))
                {
                    continue;
                }

                FenceType fenceType = GetFenceType(zoneType, choiceRand);
                if (fenceType == null)
                {
                    continue;
                }

                bool closeToFence = false;
                foreach (MyPointF item in currFences)
                {
                    float dx2 = item.X - x;
                    float dy2 = item.Y - y;
                    if (Math.Sqrt(dx2*dx2+dy2*dy2) < minDistToFence)
                    {
                        closeToFence = true;
                        continue;
                    }
                }

                if (closeToFence)
                {
                    continue;
                }

                List<MyPointF> potentialEndPoints = new List<MyPointF>();

                float extraLengthMult = 2.0f;

                float checkLength = fenceType.Length*extraLengthMult;

                int radToCheck = (int)Math.Ceiling(checkLength);

                for (int xx = x - radToCheck; xx <= x + radToCheck; xx++)
                {
                    if (xx < startx || xx >= endx)
                    {
                        continue;
                    }
                    for (int yy = y - radToCheck; yy <= y + radToCheck; yy++)
                    {
                        if (yy < starty || yy >= endy)
                        {
                            continue;
                        }

                        float dist = (float)Math.Sqrt((double)((xx - x) * (xx - x) + (yy - y) * (yy - y)));
                        if (dist < radToCheck - 0.5f || dist > radToCheck + 0.5f)
                        {
                            continue;
                        }
                        float newDist = _md.roadDistances[xx, yy];
                        if (newDist < startRoadDist-0.5f || newDist > startRoadDist+0.5f)
                        {
                            continue;
                        }

                        int sx = Math.Min((int)x, xx)+1;
                        int ex = Math.Max((int)x, xx)-1;
                        int sy = Math.Min(y, yy)+1;
                        int ey = Math.Max(y, yy)-1;

                        bool tooCloseToRoad = false;

                        for (int vx = sx; vx <= ex; vx++)
                        {
                            for (int vy = sy; vy <= sy; vy++)
                            {
                                if (_md.roadDistances[vx, vy] < 0.5f)
                                {
                                    tooCloseToRoad = true;
                                    break;
                                }
                            }
                        }
                        
                        if (tooCloseToRoad)
                        {
                            continue;
                        }

                        float enx = (x + (xx - x) / extraLengthMult);
                        float eny = (y + (yy - y) / extraLengthMult);

                        int inx = (int)(enx);
                        int iny = (int)(eny);

                        float ird = _md.roadDistances[inx, iny];

                        if (ird < startRoadDist-0.75f || ird > startRoadDist + 0.75f)
                        {
                            continue;
                        }

                        if (_md.bridgeDistances[xx, yy] < 15)
                        {
                            continue;
                        }

                        potentialEndPoints.Add(new MyPointF((float)enx, eny));
                    }
                }

                if (potentialEndPoints.Count < 1)
                {
                    continue;
                }

                MyPointF chosenEndPt = potentialEndPoints[choiceRand.Next() % potentialEndPoints.Count];

                float slope = _terrainManager.GetSteepness(x, y);

                if (slope > 30)
                {
                    continue;
                }

                int wdx = x / (MapConstants.TerrainPatchSize - 1);
                int wdy = y / (MapConstants.TerrainPatchSize - 1);

                float cx = (x + chosenEndPt.X) / 2.0f;
                float cy = (y + chosenEndPt.Y) / 2.0f;

                float dy = chosenEndPt.Y - y;
                float dx = chosenEndPt.X - x;
                float angle = (float)(Math.Atan2(dy, dx) * 180f / Math.PI + 90);

                //float hgt = _md.SampleHeight(x+wdx, 2000, y+wdy);
                float hgt = _terrainManager.SampleHeight(y + wdy,  x + wdx);

                //var centerHeight = _md.SampleHeight(cx+wdx, 2000, cy+wdy);
                float centerHeight = _terrainManager.SampleHeight(cy + wdy, cx + wdx);

                if (Math.Abs(hgt - centerHeight) > maxHeightAboveCenter)
                {
                    continue;
                }

                float dhx = (float)Math.Sqrt((cx - x) * (cx - x) + (cy - y) * (cy - y));
                float dhy = centerHeight - hgt;

                float hangle = (float)(Math.Atan2(-dhy, dhx)*180/Math.PI);

                // Don't allow fences that are too slanted. Looks bad.
                if (Math.Abs(hangle) >= MaxFenceHeightAngle)
                {
                    continue;
                }

                if (x >= 0 && y >= 0 && x < _mapProvider.GetMap().GetHwid()  && y < _mapProvider.GetMap().GetHhgt() &&
                    _md.mapObjects[x,y] == 0)
                {
                    _md.mapObjects[x, y] = MapConstants.FenceObjectOffset + (int)(fenceType.IdKey);
                    ushort nextVal = (byte)((angle + 360) / 4);
                    int hangle2 = (byte)((hangle + 360) / 4);

                    nextVal += (ushort)(hangle2 << 8);
                    _md.mapObjects[x, y] |= nextVal << 16;
                    currFences.Add(new MyPointF((float)x, y, 0));
                }

            }
        }
    }

    public float GetHeightAngle (FenceType fenceType, float angle, float newx, float newz, float hgt)
    {
        if (_gs == null || fenceType == null)
        {
            return 0.0f;
        }

        float fenceLength = fenceType.Length;

        float endx = (float)(newx + Math.Cos((angle - 90) * Math.PI / 180) * fenceLength);
        float endz = (float)(newz + Math.Sin((angle - 90) * Math.PI / 180) * fenceLength);


        int eax = (int)((endx / _mapProvider.GetMap().GetHwid()) * _md.awid);
        int eay = (int)((endz / _mapProvider.GetMap().GetHhgt()) * _md.ahgt);


        if (_md.roadDistances[(int)endx, (int)endz] <= 1)
        {
            return 0.0f;
        }


        float endhgt = _terrainManager.SampleHeight(endx, endz);





        float hdy = -(endhgt - hgt);
        float hdx = fenceLength;

        float hangle = (float)(Math.Atan2(hdy, hdx) * 180 / Math.PI);

        return hangle;
    }

	private FenceType GetFenceType (ZoneType ztype, MyRandom choiceRand)
	{
		if (choiceRand == null || ztype == null)
		{
			return null;
		}

        ZoneFenceType zoneFenceType = RandomUtils.GetRandomElement(ztype.FenceTypes, choiceRand);
		if (zoneFenceType == null)
		{
			return null;
		}
		
		FenceType fenceType = _gameData.Get<FenceTypeSettings>(_gs.ch).Get(zoneFenceType.FenceTypeId);
		
		if (fenceType == null || string.IsNullOrEmpty(fenceType.Art))
		{
			return null;
		}
		
		return fenceType;
	}

}
	
