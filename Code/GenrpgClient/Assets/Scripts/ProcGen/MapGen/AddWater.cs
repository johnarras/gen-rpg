using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Zones.Entities;
using System.Threading;
using Genrpg.Shared.ProcGen.Settings.MapWater;
using Genrpg.Shared.ProcGen.Settings.Trees;
using Genrpg.Shared.Zones.Settings;
using Genrpg.Shared.Zones.WorldData;

public class AddWater : BaseZoneGenerator
{

    public override async UniTask Generate(UnityGameState gs, CancellationToken token)
    {
        await base.Generate(gs, token);
        foreach (Zone zone in gs.map.Zones)
        {
            GenerateOne(gs, zone);
        }
    }


    protected List<TreeType> waterPlants = null;

    public bool TryAddPool(UnityGameState gs, WaterGenData genData)
    {
        if ( genData == null ||
            genData.x < MapConstants.TerrainPatchSize ||
            genData.z < MapConstants.TerrainPatchSize ||
            genData.x > gs.map.GetHwid() - MapConstants.TerrainPatchSize ||
            genData.z > gs.map.GetHhgt() - MapConstants.TerrainPatchSize)
        {
            return false;
        }

        if (waterPlants == null)
        {
            waterPlants = gs.data.Get<TreeTypeSettings>(gs.ch).GetData().Where(x => x.HasFlag(TreeFlags.IsWaterItem)).ToList();
        }

        if (genData.stepSize < 1)
        {
            genData.stepSize = 1;
        }

        int nx = genData.x + 0;
        int nz = genData.z + 0;

        int cx = genData.x;
        int cz = genData.z;


        if (gs.md.mapObjects[nx, nz] != 0)
        {
            bool foundOkPosition = false;
            for (int xx = nx - 1; xx <= nx + 1; xx++)
            {
                for (int zz = nz - 1; zz <= nz + 1; zz++)
                {
                    if (gs.md.mapObjects[xx, zz] == 0)
                    {
                        nz = zz;
                        nx = xx;
                        foundOkPosition = true;
                        break;
                    }
                }
                if (foundOkPosition)
                {
                    break;
                }
            }

            if (!foundOkPosition)
            {
                return false;
            }

        }

        int xSizeMaxHeight = 0;
        int ySizeMaxHeight = 0;
        float maxHeightDiff = 0;

        int extraEdge = 4;

        float centerHeight = gs.md.heights[cx,cz] * MapConstants.MapHeight;

        float minHeightTotal = centerHeight + 3;





        for (int xsize = genData.minXSize; xsize <= genData.maxXSize; xsize += genData.stepSize)
        {
            for (int ysize = genData.minZSize; ysize <= genData.maxZSize; ysize += genData.stepSize)
            {


                bool nearWater = false;
                float minHeightAroundEdges = MapConstants.MapHeight;

                float minHeightAnywhere = MapConstants.MapHeight;
                for (int xx = cx - xsize - extraEdge; xx <= cx + xsize + extraEdge; xx++)
                {
                    if (xx < 0 || xx >= gs.map.GetHwid() || nearWater)
                    {
                        minHeightAroundEdges = 0;
                        minHeightAnywhere = 0;
                        continue;
                    }

                    float dx = (xx - cx) * 1.0f / (xsize);
                    float ddx = dx * dx;
                    for (int yy = cz - ysize - extraEdge; yy <= cz + ysize + extraEdge; yy++)
                    {

                        if (yy < 0 || yy >= gs.map.GetHwid())
                        {
                            minHeightAroundEdges = 0;
                            minHeightAnywhere = 0;
                            continue;
                        }

                        if (FlagUtils.IsSet(gs.md.flags[xx, yy], MapGenFlags.NearWater))
                        {
                            nearWater = true;
                            break;
                        }

                        float dy = (yy - cz) * 1.0f / (ysize);
                        float ddy = dy * dy;


                        float currHeight = gs.md.heights[xx,yy] * MapConstants.MapHeight;
                        if (currHeight < minHeightAnywhere)
                        {
                            minHeightAnywhere = currHeight;
                        }
                        if (ddx + ddy >= 1)
                        {
                            if (currHeight < minHeightAroundEdges)
                            {
                                minHeightAroundEdges = currHeight;
                            }
                        }
                    }
                }


                if (genData.maxHeight > 0 && minHeightAroundEdges > genData.maxHeight)
                {
                    minHeightAroundEdges = genData.maxHeight;
                }


                if (minHeightAroundEdges < minHeightAnywhere + 2)
                {
                    continue;
                }



                int heightDiff = (int)((minHeightAroundEdges - MapConstants.MinLandHeight));



                if (heightDiff > maxHeightDiff)
                {
                    xSizeMaxHeight = ysize;
                    ySizeMaxHeight = xsize;
                    maxHeightDiff = heightDiff;
                }
            }
        }

        int nearWaterRad = 2;

        if (xSizeMaxHeight > 0 && ySizeMaxHeight > 0 && maxHeightDiff > 1)
        {
            float waterHeight = (MapConstants.MinLandHeight + (int)maxHeightDiff - 0.5f) / MapConstants.MapHeight;
            float maxPlantHeight = waterHeight + 0.5f / MapConstants.MapHeight;

            MyRandom rand = new MyRandom(genData.x * 31 + genData.z * 71);

            float plantChance = MathUtils.FloatRange(0,1, rand);

            for (int xx = cx - xSizeMaxHeight - extraEdge; xx <= cx + xSizeMaxHeight + extraEdge; xx++)
            {
                if (xx < 0 || xx >= gs.map.GetHwid())
                {
                    continue;
                }
                for (int yy = cz - ySizeMaxHeight - extraEdge; yy <= cz + ySizeMaxHeight + extraEdge; yy++)
                {
                    if (yy < 0 || yy >= gs.map.GetHhgt())
                    {
                        continue;
                    }

                    gs.md.flags[xx,yy] |=  MapGenFlags.NearWater;
                    if (gs.md.heights[xx,yy] < waterHeight)
                    {
                        gs.md.flags[xx, yy] |= MapGenFlags.BelowWater;
                    }


                    int tx = xx + 0*(xx / (MapConstants.TerrainPatchSize - 1));
                    int ty = yy + 0*(yy / (MapConstants.TerrainPatchSize - 1));
                    int lowerObject = gs.md.mapObjects[tx, ty] & (1 << 16);
                    
                    if (lowerObject < MapConstants.BridgeObjectOffset ||
                        lowerObject > MapConstants.BridgeObjectOffset + MapConstants.MapObjectOffsetMult)
                    {
                        if (gs.md.heights[xx,yy] < waterHeight)
                        {
                            //gs.md.mapObjects[tx,ty] = 0;
                        }
                        else
                        {

                            float dxpct = 1.0f * (cx - xx) / xSizeMaxHeight;
                            float dypct = 1.0f * (cz - yy) / ySizeMaxHeight;

                            float dpct = dxpct * dxpct + dypct * dypct;
                            if (dpct <= 1.0f)
                            {
                                int ux = tx + 0 * (xx / (MapConstants.TerrainPatchSize - 1));
                                int uy = ty + 0 * (yy / (MapConstants.TerrainPatchSize - 1));
                                if (gs.md.mapObjects[ux, uy] == 0 && waterPlants.Count > 0 &&
                                    gs.md.heights[xx, yy] < maxPlantHeight && rand.NextDouble() < plantChance)
                                {
                                    bool nearRealWater = false;
                                    for (int x1 = xx - nearWaterRad; x1 <= xx + nearWaterRad; x1++)
                                    {
                                        if (nearRealWater)
                                        {
                                            break;
                                        }

                                        if (x1 < 0 || x1 >= gs.map.GetHwid())
                                        {
                                            continue;
                                        }

                                        for (int y1 = yy - nearWaterRad; y1 <= yy + nearWaterRad; y1++)
                                        {
                                            if (y1 < 0 || y1 >= gs.map.GetHhgt())
                                            {
                                                continue;
                                            }

                                            if (gs.md.heights[x1, y1] < waterHeight)
                                            {
                                                nearRealWater = true;
                                                break;
                                            }
                                        }
                                    }
                                    if (nearRealWater)
                                    {
                                        TreeType plantChosen = waterPlants[rand.Next() % waterPlants.Count];
                                        gs.md.mapObjects[ux, uy] = (int)(MapConstants.TreeObjectOffset + plantChosen.IdKey);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            gs.md.mapObjects[nx, nz] = MapConstants.WaterObjectOffset + (int)maxHeightDiff;
            int szOffset = (int)(xSizeMaxHeight * 256) + ySizeMaxHeight;
            gs.md.mapObjects[nx, nz] += (szOffset << 16);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Attempt to add Pool(s) to a given zone.
    /// </summary>
    /// <param name="gs"></param>
    /// <param name="zone"></param>
    public void GenerateOne(UnityGameState gs, Zone zone)
    {
        if ( zone == null)
        {
            return;
        }

        ZoneType ztype = gs.data.Get<ZoneTypeSettings>(gs.ch).Get(zone.ZoneTypeId);
        if (ztype == null)
        {
            return;
        }

        int minx = zone.XMin;
        int maxx = zone.XMax;
        int minz = zone.ZMin;
        int maxz = zone.ZMax;


        int totalSize = (maxx - minx) * (maxz - minz);

        totalSize /= (MapConstants.TerrainPatchSize * MapConstants.TerrainPatchSize);

        totalSize /= 10;

        float worldBaseHeight = 1.0f * MapConstants.MinLandHeight / MapConstants.MapHeight;

        int numPools = totalSize;

        MyRandom rand = new MyRandom(zone.Seed % 1000000000 + 2438932);


        int currentPools = 0;

        int totalTries = 100 * numPools;

        for (int times = 0; times < totalTries; times++)
        {
            if (currentPools >= numPools)
            {
                break;
            }

            int cx = MathUtils.IntRange(minx, maxx, rand);
            int cz = MathUtils.IntRange(minz, maxz, rand);


            int rad = 100;
            float minDistToFeature = rad * 3 / 2;


            bool onEdgeOfMap = false;

            for (int x = cx-rad; x <= cx+rad; x++)
            {
                if (onEdgeOfMap)
                {
                    break;
                }

                if (x < 0 || x >= gs.map.GetHwid())
                {
                    onEdgeOfMap = true;
                    break;
                }
                int dx = x - cx;
                for (int z = cz-rad; z <= cz+rad; z++)
                {
                    if (z < 0 || z >= gs.map.GetHhgt())
                    {
                        onEdgeOfMap = true;
                        break;
                    }
                    int dy = z - cz;

                    if (gs.md.alphas[x,z,MapConstants.RoadTerrainIndex] > 0 ||
                        gs.md.mountainHeights[x,z] != 0 ||
                        FlagUtils.IsSet(gs.md.flags[x,z],MapGenFlags.IsLocation |
                        MapGenFlags.NearWater))
                    {
                        float dist = (float)Math.Sqrt(dx * dx + dy * dy);
                        if (dist < minDistToFeature)
                        {
                            minDistToFeature = dist;
                        }
                    }
                }
            }

            if (onEdgeOfMap)
            {
                continue;
            }

            int minDist = rad / (1 + (times * 3) / totalTries);

            if (minDistToFeature < minDist)
            {
                continue;
            }

            int maxRadius = (int)(minDistToFeature - 10);


            WaterGenData poolData = new WaterGenData()
            {
                x = cx,
                z = cz,
                minXSize = maxRadius/2,
                maxXSize = maxRadius,
                minZSize = maxRadius/2,
                maxZSize = maxRadius,
                stepSize = 1,
            };

            int deformSeed = rand.Next();

            AlterHeightsNear(gs, cx, cz, maxRadius, deformSeed, true);


            if (!TryAddPool(gs, poolData))
            {
                AlterHeightsNear(gs, cx, cz, maxRadius, deformSeed, false);
            }
            else
            {
                currentPools++;
                for (int x = cx-maxRadius; x <= cx+maxRadius; x++)
                {
                    if (x < 0 || x >= gs.map.GetHwid())
                    {
                        continue;
                    }

                    for (int y = cz - maxRadius; y <= cz + maxRadius; y++)
                    {
                        if (y < 0 || y >= gs.map.GetHhgt())
                        {
                            continue;
                        }

                        if (gs.md.heights[x,y] < worldBaseHeight &&
                            FlagUtils.IsSet(gs.md.flags[x,y], MapGenFlags.BelowWater))
                        {
                            gs.md.heights[x, y] = worldBaseHeight;
                        }
                    }
                }
            }
        }           
    }


    protected void AlterHeightsNear(UnityGameState gs, int cx, int cy, int maxRadius, int randomSeed, bool lowerHeights)
    {
        MyRandom rand = new MyRandom(randomSeed);

        int raiseLowerMult = (lowerHeights ? -1 : 1);

        int size = maxRadius * 2 + 1;

        float heightAmp = MathUtils.FloatRange(0.4f, 0.7f, rand);
        float heightFreq = MathUtils.FloatRange(0.02f, 0.04f, rand) * size;
        float heightPers = MathUtils.FloatRange(0.2f, 0.5f, rand);
        int heightOctaves = 2;
        float[,] heightNoise = _noiseService.Generate(gs, heightPers, heightFreq, heightAmp, heightOctaves, rand.Next(), size, size);

        int maxRadiusX = MathUtils.IntRange(maxRadius * 2 / 3, maxRadius, rand);
        int maxRadiusY = MathUtils.IntRange(maxRadius * 2 / 3, maxRadius, rand);
        int minRadius = Math.Min(maxRadiusX, maxRadiusY);
        float bottomDepth = MathUtils.FloatRange(minRadius / 4, minRadius / 2, rand);


        float radAmp = MathUtils.FloatRange(0.5f, 0.9f, rand);
        float radFreq = MathUtils.FloatRange(5.0f, 14.0f, rand);
        float radPers = MathUtils.FloatRange(0.2f, 0.8f, rand);
        int radOctaves = 2;

        float[,] radNoise = _noiseService.Generate(gs, radPers, radFreq, radAmp, radOctaves, rand.Next(), 360, 360);


        float midPower = MathUtils.FloatRange(0.2f, 0.4f, rand);
        float powerAmp = MathUtils.FloatRange(0.05f, 0.15f, rand);
        float powerFreq = MathUtils.FloatRange(5.0f, 12.0f, rand);
        float powerPers = MathUtils.FloatRange(0.2f, 0.3f, rand);
        int powerOctaves = 2;
        float[,] powerNoise = _noiseService.Generate(gs, powerPers, powerFreq, powerAmp, powerOctaves, rand.Next(), 360, 360);



        int angleRot = rand.Next() % 360;
        int xmin = cx - maxRadiusX;
        int xmax = cx + maxRadiusX;
        int ymin = cy - maxRadiusY;
        int ymax = cy + maxRadiusY;
        for (int xx = xmin; xx <= xmax; xx++)
        {
            if (xx < 0 || xx >= gs.map.GetHwid())
            {
                continue;
            }

            int dx = xx - cx;
            float xpct = (xx - cx) / (1.0f * maxRadiusX);
            int distToEdgeX = Math.Min(Math.Abs(xx - xmin), Math.Abs(xx - xmax));
            int offsetx = xx - xmin;
            for (int yy = ymin; yy <= ymax; yy++)
            {
                int dy = yy - cy;
                if (yy < 0 || yy >= gs.map.GetHhgt())
                {
                    continue;
                }

                float ypct = (yy - cy) / (1.0f * maxRadiusY);
                int offsety = yy - ymin;

                int distToEdgeY = Math.Min(Math.Abs(yy-ymin), Math.Abs(yy-ymax));

                float radMult = 1.0f;

                double angle = Math.Atan2(dx, dy) * 180 / Math.PI + angleRot;

                while (angle >= 360)
                {
                    angle -= 360;
                }

                while (angle < 0)
                {
                    angle += 360;
                }

                int intAngle = (int)(angle);

                float radDelta = radNoise[intAngle, intAngle/2];

                int distToEnd = Math.Abs(Math.Min(intAngle, 360 - intAngle));

                int distToEndCheck = 5;
                if (distToEnd <= distToEndCheck)
                {
                    radDelta *= 1.0f * distToEnd / distToEndCheck;
                }

                if (radDelta < 0)
                {
                    // If delta < 0 set it to at least -0.5f, then cube to shrink it.
                    radDelta = -(float)(Math.Pow(Math.Abs(radDelta), 3.0f));
                    if (radDelta < -0.5f)
                    {
                        radDelta = -0.5f;
                    }
                    // Negative rad = wider area so we bump up against the edge of the region we are modifying.
                }
                radMult = 0.9f + radDelta;




                float powerDelta = powerNoise[intAngle,intAngle/2];

                if (distToEnd <= distToEndCheck)
                {
                    powerDelta *= 1.0f * distToEnd / distToEndCheck;
                }

                float depthPower = midPower + powerDelta;

                float pctToEdge = (float)(Math.Pow((xpct * xpct + ypct * ypct)*radMult, depthPower));

                if (pctToEdge > 1)
                {
                    continue;
                }

                float currNoise = heightNoise[offsetx, offsety];

                float heightDiff = (1 - pctToEdge) * bottomDepth;
                heightDiff += (float)((currNoise * bottomDepth*(1-pctToEdge*pctToEdge)));

                int edgeScaleDist = 8;

                float edgeScaleDown = Math.Min(distToEdgeX, edgeScaleDist) * 1.0f / edgeScaleDist;
                edgeScaleDown *= Math.Min(distToEdgeY, edgeScaleDist) * 1.0f / edgeScaleDist;
                edgeScaleDown = (float)(Math.Pow(edgeScaleDown, 0.8f));

                heightDiff *= edgeScaleDown;

                heightDiff /= MapConstants.MapHeight;


                gs.md.heights[xx, yy] += heightDiff*raiseLowerMult;
            }
        }
    }

}
