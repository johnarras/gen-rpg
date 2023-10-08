using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;


using Genrpg.Shared.Core.Entities;




using GEntity = UnityEngine.GameObject;
using System.Threading.Tasks;

using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Utils.Data;
using Genrpg.Shared.Zones.Entities;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.Shared.ProcGen.Entities;

using System.Threading;

public class AddOutcroppings : BaseZoneGenerator
{
    const int MinSize = 60;
    const int MaxSize = 400;
    const int GridBuffer = 200;
    const int GridSize = MaxSize + 2 * GridBuffer;

    const int MaxGridIndex = 2;
    float[,,] grids = null;

    protected ILineGenService _lineGenService;

    public override async Task Generate(UnityGameState gs, CancellationToken token)
    {
        await base.Generate(gs, token);
        grids = new float[GridSize, GridSize, MaxGridIndex];
        ClearGrid();

        foreach (Zone zone in gs.map.Zones)
        {
            GenerateOne(gs, zone, gs.data.GetGameData<ZoneTypeSettings>(gs.ch).GetZoneType(zone.ZoneTypeId), zone.XMin, zone.ZMin, zone.XMax, zone.ZMax);
        }
    }

    private void ClearGrid()
    {
        for (int i = 0; i < 2; i++)
        {
            for (int x = 0; x < GridSize; x++)
            {
                for (int y = 0; y < GridSize; y++)
                {
                    grids[x, y, i] = 0;
                }
            }
        }
    }

    public void GenerateOne(UnityGameState gs, Zone zone, ZoneType zoneType, int startx, int starty, int endx, int endy)
    {
        if ( zone == null || endx <= startx || endy <= starty)
        {
            return;
        }

        MyRandom rand = new MyRandom(zone.Seed % 2000000000 + 15434454);

        int edgeSize = MapConstants.TerrainPatchSize * 2;

        startx = MathUtils.Clamp(edgeSize, startx, gs.map.GetHwid() - edgeSize);
        endx = MathUtils.Clamp(edgeSize, endx, gs.map.GetHwid() - edgeSize);

        starty = MathUtils.Clamp(edgeSize, starty, gs.map.GetHwid() - edgeSize);
        endy = MathUtils.Clamp(edgeSize, endy, gs.map.GetHwid() - edgeSize);


        int numOutcroppings = 2;
        

        for (int times = 0; times < 10; times++)
        {
            if (rand.NextDouble() < 0.4f)
            {
                numOutcroppings++;
            }
            else
            {
                break;
            }
        }


        numOutcroppings = 1;

        float InGridVal = 1.0f;
        for (int times = 0; times < numOutcroppings; times++)
        {
            ClearGrid();


            float amp = MathUtils.FloatRange(0.1f, 0.2f, rand);
            float freq = MathUtils.FloatRange(5f, 15f, rand);
            float pers = MathUtils.FloatRange(0.2f, 0.5f, rand);

            float[,] heightScales = _noiseService.Generate(gs, pers, freq, amp, 2, rand.Next(), GridSize, GridSize);

            for (int tries = 0; tries < 20; tries++)
            {
                int sx = MathUtils.IntRange(startx, endx, rand);
                int sy = MathUtils.IntRange(starty, endy, rand);

                int ex = MathUtils.IntRange(startx, endx, rand);
                int ey = MathUtils.IntRange(starty, endy, rand);

                int dx = Math.Abs(ex - sx);
                int dy = Math.Abs(ey - sy);

                if (dx < MinSize || dx > MaxSize || dy < MinSize || dy > MaxSize)
                {
                    continue;
                }
                float finalHeightScale = 1.0f;

                if (rand.NextDouble() < 0.5)
                {
                    finalHeightScale = -finalHeightScale;
                }

                int minSize = Math.Min(dx, dy);
                int maxSize = Math.Max(dx, dy);

                int maxWidth = MathUtils.IntRange(minSize * 2 / 3, minSize, rand);

                float fullHeight = MathUtils.FloatRange(20.0f,60.0f,rand) / MapConstants.MapHeight;

                int mx = (sx + ex) / 2;
                int my = (sy + ey) / 2;

                LineGenParameters lineParams = new LineGenParameters()
                {
                    UseOvalWidth = true,
                    MinWidthSize = Math.Max(4, minSize / 10),
                    MaxWidthSize = maxWidth,
                    WidthSizeChangeAmount = maxWidth/4,
                    WidthSizeChangeChance = 0.2f,
                    Seed = rand.Next(),
                    WidthPosShiftChance = 0.4f,
                    WidthPosShiftSize = 2,
                    LinePathNoiseScale = 1.0f,
                };

                MyPoint start = new MyPoint(sx, sy);
                MyPoint end = new MyPoint(ex, ey);


                List<MyPointF> line = _lineGenService.GetBressenhamLine(gs, start, end, lineParams);

                if (line.Count < 1)
                {
                    continue;
                }

                int numCenters = 0;
                int numAdjusted = 0;
                if (line != null)
                {
                    foreach (MyPointF pt in line)
                    {
                        int px = (int)(pt.X - mx)+GridSize/2;
                        int py = (int)(pt.Y - my)+GridSize/2;

                        
                        if (px < 1 || py < 1 || px >= GridSize-1 || py >= GridSize-1)
                        {
                            continue;
                        }
                        grids[px, py, 0] = InGridVal;
                        numCenters++;
                                
                    }
                }

                List<MyPointF> lowestPoints = new List<MyPointF>();


                float smoothFreq = MathUtils.FloatRange(0.03f, 0.7f, rand);
                float smoothAmp = MathUtils.FloatRange(0.2f, 0.3f, rand);
                float smoothPers = MathUtils.FloatRange(0.1f, 0.3f, rand);
                int smoothOctaves = 2;

                float[,] smoothNoise = _noiseService.Generate(gs, pers, freq, amp, smoothOctaves, rand.Next(), GridSize, GridSize);


                int baseSmoothRad = Math.Max(7, (int)(fullHeight/6));


                List<MyPointF> potentialLowestPoints = new List<MyPointF>();
            
                for (int x = 0; x < GridSize; x++)
                {
                    for (int y = 0; y < GridSize; y++)
                    {
                        int numCells = 0;
                        float totalSum = 0;
                        int smoothRad = Math.Max(2, (int)(baseSmoothRad * (1 + smoothNoise[x, y])));
                        for (int xx = x-smoothRad; xx <= x+smoothRad; xx++)
                        {
                            if (xx < 0 || xx >= GridSize)
                            {
                                continue;
                            }

                            for (int yy = y-smoothRad; yy <= y+smoothRad; yy++)
                            {
                                if (yy < 0 || yy >= GridSize)
                                {
                                    continue;
                                }

                                totalSum += grids[xx, yy, 0];
                                numCells++;
                            }
                        }

                        if (numCells > 0)
                        {
                            grids[x, y, 1] = totalSum / numCells;
                            if (totalSum < numCells && totalSum > 0)
                            {
                                potentialLowestPoints.Add(new MyPointF(x, y, 0));
                            }
                        }
                        else
                        {
                            grids[x, y, 1] = 0;
                        }

                    }
                }
                int numLowestPoints = 1;
                for (int i = 0; i < 3; i++)
                {
                    if (rand.NextDouble() < 0.1 / (i + 1))
                    {
                        numLowestPoints++;
                    }
                    else
                    {
                        break;
                    }
                }

                if (potentialLowestPoints.Count > 10)
                {
                    for (int i = 0; i < numLowestPoints; i++)
                    {
                        lowestPoints.Add(potentialLowestPoints[rand.Next() % potentialLowestPoints.Count]);
                    }
                }

                foreach (MyPointF pt in lowestPoints)
                {
                    float lowPointRadius = MathUtils.FloatRange(1.25f, 2.0f, rand) * fullHeight * MapConstants.MapHeight;

                    float power = MathUtils.FloatRange(1.0f, 1.5f, rand);

                    for (int x = 0; x < GridSize; x++)
                    {
                        float ddx = x - pt.X;
                        for (int y = 0; y < GridSize; y++)
                        {
                            float ddy = y - pt.Y;

                            double dist = Math.Sqrt(ddx * ddx + ddy * ddy);
                            float scaleDist = (float)Math.Pow(Math.Min(1.0f, dist / lowPointRadius), power);
                            grids[x, y, 1] *= scaleDist;
                        }
                    }
                }

                // Find lowest point where the grid is >= 1.

                float lowestMapHeight = 1.0f;
                float highestMapHeight = 0.0f;
                for (int x = 0; x < GridSize; x++)
                {
                    int wx = x + mx - GridSize / 2;
                    if (wx < 0 || wx >= gs.map.GetHwid())
                    {
                        continue;
                    }

                    for (int y = 0; y < GridSize; y++)
                    {
                        int wy = y + my - GridSize / 2;
                        if (wy < 0 || wy >= gs.map.GetHhgt())
                        {
                            continue;
                        }

                        if (grids[x, y, 1] != 0)
                        {
                            float roadDist = gs.md.roadDistances[wx, wy];


                            float roadScalePercent = MathUtils.GetSmoothScalePercent(10, 60, roadDist);

                            if (roadDist < 5)
                            {
                                roadScalePercent = 0;
                            }

                            grids[x, y, 1] *= roadScalePercent;

                            float mountainHeight = gs.md.mountainDistPercent[wx, wy];

                            grids[x, y, 1] *= mountainHeight;

                            if (grids[x,y,1] == 1)
                            {
                                float hgt = gs.md.heights[wx, wy];
                                if (hgt < lowestMapHeight)
                                {
                                    lowestMapHeight = hgt;
                                }

                                if (hgt > highestMapHeight)
                                {
                                    highestMapHeight = hgt;
                                }
                            }
                        }
                    }
                }
                // We target the grid to go to the highest worldheight modified by the noise/scale down.
                if (highestMapHeight <= 0 || lowestMapHeight >= 1)
                {
                    continue;
                }

                float heightDiff = highestMapHeight - lowestMapHeight;
                fullHeight -= heightDiff;
                if (fullHeight < 2.0f / MapConstants.MapHeight)
                {
                    fullHeight = 2.0f / MapConstants.MapHeight;
                }

                for (int x = 0; x < GridSize; x++)
                {
                    int wx = x + mx - GridSize / 2;
                    if (wx < 0 || wx >= gs.map.GetHwid())
                    {
                        continue;
                    }

                    for (int y = 0; y < GridSize; y++)
                    {
                        int wy = y + my - GridSize / 2;
                        if (wy < 0 || wy >= gs.map.GetHhgt())
                        {
                            continue;
                        }

                        if (grids[x, y, 1] <= 0)
                        {
                            continue;
                        }

                        if (finalHeightScale > 0)
                        {

                            // Calculate the height difference.
                            // grid val * ((maxHeight-worldHeight)+(outcropping overallheight*(1+noise)))
                            float gridval = grids[x, y, 1] * ((highestMapHeight - gs.md.heights[wx, wy]) + fullHeight * (1 + heightScales[x, y]));

                            gs.md.heights[wx, wy] += gridval * finalHeightScale;
                        }
                        else
                        {
                            float gridval = grids[x, y, 1] * ((gs.md.heights[wx, wy]-lowestMapHeight) - fullHeight * (1 + heightScales[x, y]));

                            gs.md.heights[wx, wy] += gridval * -finalHeightScale;
                        }
                        numAdjusted++;
                    }
                }

                break;
            }
        }
    }
}