using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using GEntity = UnityEngine.GameObject;


using Genrpg.Shared.Core.Entities;



using Cysharp.Threading.Tasks;

using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Utils.Data;
using Genrpg.Shared.Zones.Entities;
using Genrpg.Shared.MapServer.Entities;

using System.Threading;
using Genrpg.Shared.Zones.WorldData;

public class ZoneHeightCellData
{

    public int zoneId;
    public int x;
    public int y;
    public int wx;
    public int wy;
    public int heightOffset;
}

public class RaiseOrLowerZones : BaseZoneGenerator
{

    public const int StartDist = -1000;
    public override async UniTask Generate(CancellationToken token)
    {
        await base.Generate(token);
        MyRandom rand = new MyRandom(_mapProvider.GetMap().Seed % 23432432 + 31234);

        int minEdgeDistance = _mapProvider.GetMap().BlockCount/5 * MapConstants.TerrainPatchSize;

        if (minEdgeDistance > 5 * MapConstants.TerrainPatchSize)
        {
            minEdgeDistance = 5 * MapConstants.TerrainPatchSize;
        }

        if (minEdgeDistance < 2*MapConstants.TerrainPatchSize)
        {
           // return;
        }

        int minpos = minEdgeDistance;
        int maxPos = _mapProvider.GetMap().GetHwid() - minEdgeDistance;
        foreach (Zone zone in _mapProvider.GetMap().Zones)
        {

            if (zone.XMin >= zone.XMax || zone.ZMin > zone.ZMax)
            {
               continue;
            }

            if (zone.XMin < minpos || zone.ZMin < minpos || zone.XMax > maxPos || zone.ZMax > maxPos)
            {
                continue;
            }


            RaiseOrLowerZone(zone, rand.Next());
        }

    }

    private void RaiseOrLowerZone(Zone zone, int seed)
    {
        if (_mapProvider.GetMap() == null || zone == null)
        {
            return;
        }

        int roadCheckRad = 5;

        int extraWidth = (int)_md.GetMountainDefaultSize(_mapProvider.GetMap());

        MyRandom rand = new MyRandom(seed);


        extraWidth = MathUtils.IntRange(extraWidth * 4 / 5, extraWidth * 5 / 4, rand);


        float heightOffset = MathUtils.FloatRange(0.7f,0.9f,rand)*extraWidth / MapConstants.MapHeight;


        float waterScaledHeight = 1.0f * MapConstants.MinLandHeight / MapConstants.MapHeight;

        int midx = (zone.XMin + zone.XMax) / 2;
        int midy = (zone.ZMin + zone.ZMax) / 2;

        float midHeight = _md.heights[midx, midy];

        float centerSpread = 0.3f;

        float powerSpread = 0.3f;

        float minCenter = 0.5f - centerSpread;
        float maxCenter = 0.5f + centerSpread;
        float minPower = 1 - powerSpread;
        float maxPower = 1 + powerSpread;

        int minx = (int)Math.Max(0, zone.XMin - extraWidth);
        int maxx = (int)Math.Min(_mapProvider.GetMap().GetHwid() - 1, zone.XMax + extraWidth);
        int miny = (int)Math.Max(0, zone.ZMin - extraWidth);
        int maxy = (int)Math.Min(_mapProvider.GetMap().GetHhgt() - 1, zone.ZMax + extraWidth);

     

        int closeCheckEdgeSize = 8;

        bool tooLowAlready = false;


        bool tooCloseToRaisedOrLowered = false;
        for (int x = minx+closeCheckEdgeSize; x < maxx-closeCheckEdgeSize; x++)
        {
            for (int y = miny+closeCheckEdgeSize; y < maxy-closeCheckEdgeSize; y++)
            {
                if (FlagUtils.IsSet(_md.flags[x, y], MapGenFlags.IsRaisedOrLowered))
                {
                    tooCloseToRaisedOrLowered = true;
                    break;
                }

                if (x >= minx && x < maxx && y >= miny && y < maxy)
                {
                    if (_md.heights[x, y] - heightOffset < waterScaledHeight)
                    {
                       tooLowAlready = true;
                    }
                }
            }
            if (tooCloseToRaisedOrLowered)
            {
               break;
            }
        }

        if (tooCloseToRaisedOrLowered)
        {
            return;
        }


        int distx = maxx - minx+1;
        int disty = maxy - miny+1;

        int size = (distx + disty) / 2;

        if (distx < 100 || disty < 100)
        {
            return;
        }

        int noLowEdgeSize = MapConstants.TerrainPatchSize * 10;

        
        if (midx < noLowEdgeSize ||
            midy < noLowEdgeSize ||
            midx > _mapProvider.GetMap().GetHwid()-noLowEdgeSize ||
            midy > _mapProvider.GetMap().GetHhgt()-noLowEdgeSize)
        {
            tooLowAlready = true;
        }


        if (rand.NextDouble() < 0.5 && !tooLowAlready)
        {
            heightOffset = -heightOffset;
        }

        float centerAmp = MathUtils.FloatRange(powerSpread,powerSpread*2, rand);
        float centerFreq = MathUtils.FloatRange(size / 40, size / 10, rand);
        float centerPers = MathUtils.FloatRange(0.1f, 0.4f, rand);
        int centerOctaves = 2;

        float[,] centers = _noiseService.Generate(centerPers, centerFreq, centerAmp, centerOctaves, rand.Next(), distx, disty);

        float powerAmp = MathUtils.FloatRange(centerSpread,centerSpread*2, rand);
        float powerFreq = MathUtils.FloatRange(size / 40, size / 10, rand);
        float powerPers = MathUtils.FloatRange(0.1f, 0.3f, rand);
        int powerOctaves = 2;

        float[,] powers = _noiseService.Generate(powerPers, powerFreq, powerAmp,powerOctaves, rand.Next(), distx, disty);

        int xsize = maxx - minx + 1;
        int ysize = maxy - miny + 1;

        int[,] distances = new int[xsize, ysize];

        for (int x = 0; x < xsize; x++)
        {
            for (int y = 0; y < ysize; y++)
            {
                distances[x, y] = StartDist;
            }
        }

        // Now get the 0 dist zones.

        Queue<ZoneHeightCellData> cellQueue = new Queue<ZoneHeightCellData>();

        List<MyPoint2> offsetList = new List<MyPoint2>();
        offsetList.Add(new MyPoint2(-1, 0));
        offsetList.Add(new MyPoint2(1, 0));
        offsetList.Add(new MyPoint2(0, 1));
        offsetList.Add(new MyPoint2(0, -1));
        for (int x = 0; x < xsize; x++)
        {
            int wx = x + minx;
            if (wx < 1 || wx >= _mapProvider.GetMap().GetHwid() - 1)
            {
                continue;
            }

            for (int y = 0; y < ysize; y++)
            {
                int wy = y + miny;
                if (wy < 1 || wy >= _mapProvider.GetMap().GetHhgt() - 1)
                {
                    continue;
                }

                if (_md.mapZoneIds[wx, wy] != zone.IdKey)
                {
                    continue;
                }

                foreach (MyPoint2 offset in offsetList)
                {
                    int x2 = wx + (int)(offset.X);
                    int y2 = wy + (int)(offset.Y);

                    if (_md.mapZoneIds[x2, y2] != zone.IdKey)
                    {

                        ZoneHeightCellData cell = new ZoneHeightCellData()
                        {
                            zoneId = _md.mapZoneIds[wx, wy],
                            wx = wx,
                            wy = wy,
                            x = x,
                            y = y,
                            heightOffset = 0,
                        };
                        cellQueue.Enqueue(cell);
                        distances[x, y] = 0;
                        break;
                    }
                }
            }
        }


        while (cellQueue.Count > 0)
        {
            ZoneHeightCellData firstCell = cellQueue.Dequeue();

            if (firstCell.wx < 1 || firstCell.wx >= _mapProvider.GetMap().GetHwid()-1 ||
                firstCell.wy < 1 || firstCell.wy >= _mapProvider.GetMap().GetHhgt()-1 ||
                firstCell.x < 1 || firstCell.x >= xsize-1 ||
                firstCell.y < 1 || firstCell.y >= ysize-1)
            {
                continue;
            }
            
            foreach (MyPoint2 offset in offsetList)
            {
                int nwx = firstCell.wx + (int)(offset.X);
                int nwy = firstCell.wy + (int)(offset.Y);
                int nx = firstCell.x + (int)(offset.X);
                int ny = firstCell.y + (int)(offset.Y);
                if (distances[nx, ny] != StartDist)
                {
                    continue;
                }

                short newZoneId = _md.mapZoneIds[nwx, nwy];

                int delta = 0;

                if (newZoneId == zone.IdKey) // Height goes up.
                {
                    delta = 1;
                }
                else
                {
                    delta = -1;
                }

                int newHeightOffset = firstCell.heightOffset + delta;



                ZoneHeightCellData cell = new ZoneHeightCellData()
                {
                    zoneId = newZoneId,
                    wx = nwx,
                    wy = nwy,
                    x = nx,
                    y = ny,
                    heightOffset = newHeightOffset,
                };
                cellQueue.Enqueue(cell);
                distances[nx, ny] = newHeightOffset;
            }

        }


        // Now we have all of these numbers from -extrawidth to extraWidth now raiselower the hills.


        float middleHeightPct = 0.40f;

        float deltaDiv = 8.0f;
        int numCellsChanged = 0;
        for (int x = 0; x < xsize; x++)
        {
            for (int y = 0; y < ysize; y++)
            {

                int nx = x;
                int ny = y;

                int xroadDelta = 0;
                int yroadDelta = 0;

                // Check for roads nearby...if more are "farther" away from center, lower this
                // otherwise raise this.
                for (int xx = x - roadCheckRad; xx <= x + roadCheckRad; xx++)
                {
                    if (xx < 0 || xx >= xsize)
                    {
                        continue;
                    }

                    int drx = xx - x;
                    int wx = xx + minx;
                    if (wx < 0 || wx >= _mapProvider.GetMap().GetHwid())
                    {
                        continue;
                    }

                    for (int yy = y - roadCheckRad; yy <= y + roadCheckRad; yy++)
                    {
                        if (yy < 0 || yy >= ysize)
                        {
                            continue;
                        }

                        int dry = yy - y;
                        int wy = yy + miny;
                        if (wy < 0 || wy >= _mapProvider.GetMap().GetHhgt())
                        {
                            continue;
                        }

                        // Use wx wy for global alphas value
                        if (_md.alphas[wx, wy, MapConstants.RoadTerrainIndex] > 0)
                        {
                            if (xx < x)
                            {
                                xroadDelta--;
                            }
                            else if (xx > x)
                            {
                                xroadDelta++;
                            }

                            if (yy < y)
                            {
                                yroadDelta--;
                            }
                            else if (yy > y)
                            {
                                yroadDelta++;
                            }
                        }
                    }
                }
                nx = MathUtils.Clamp(0, x + (int)(xroadDelta / deltaDiv), xsize - 1);
                ny = MathUtils.Clamp(0, y + (int)(yroadDelta / deltaDiv), ysize - 1);





                int currDist = distances[nx, ny];

                if (currDist > -extraWidth && currDist < extraWidth)
                {
                    if (currDist != 0)
                    {
                        numCellsChanged++;
                    }
                }

                currDist = MathUtils.Clamp(-extraWidth, currDist, extraWidth);

                float heightDistPct = 0.0f;

                if (currDist <= 0)
                {
                    // 0 at -extra width, middleHeightPercent at middle.
                    heightDistPct = ((currDist + extraWidth) * middleHeightPct) / extraWidth;
                }
                else
                {
                    // start at middleHeightPercent at 0, up to 1 at extraWidth
                    heightDistPct = middleHeightPct + currDist * (1 - middleHeightPct) / extraWidth;
                }
                float power = MathUtils.Clamp(minPower, 1.0f + powers[x, y], maxPower);



                float powerDistPct = (float)(Math.Pow(heightDistPct, power));





                float finalHeightOffset = powerDistPct * heightOffset;


                _md.flags[x+minx, y+miny] |= MapGenFlags.IsRaisedOrLowered;
                _md.heights[x+minx, y+miny] += finalHeightOffset;
            }
        }

    }
}