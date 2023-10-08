using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;


using Genrpg.Shared.Core.Entities;



using GEntity = UnityEngine.GameObject;
using System.Threading.Tasks;

using Genrpg.Shared.Utils;
using Genrpg.Shared.Utils.Data;
using Genrpg.Shared.Zones.Entities;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.Shared.ProcGen.Entities;
using System.Threading;

public class AddLocationPatches : BaseZoneGenerator
{
    public override async Task Generate(UnityGameState gs, CancellationToken token)
    {
        await base.Generate(gs, token);
        int edgeSize = MapConstants.LocCenterEdgeSize;
        for (int pass = 0; pass < 2; pass++)
        {
            for (int gx = 0; gx < gs.md.locationGrid.GetLength(0); gx++)
            {
                for (int gy = 0; gy < gs.md.locationGrid.GetLength(1); gy++)
                {
                    if (gs.md.locationGrid[gx, gy] == null)
                    {
                        continue;
                    }

                    foreach (Location loc in gs.md.locationGrid[gx, gy])
                    {
                        if ((pass == 0) != (loc.LocationTypeId == LocationType.ZoneCenter))
                        {
                            continue;
                        }
                        Zone zone = null;

                        if (loc.CenterX < edgeSize || loc.CenterX >= gs.map.GetHwid()-edgeSize ||
                            loc.CenterZ < edgeSize || loc.CenterZ >= gs.map.GetHhgt()-edgeSize)
                        {
                            continue;
                        }

                        if (loc.CenterX >= 0 && loc.CenterX < gs.map.GetHwid() &&
                            loc.CenterZ >= 0 && loc.CenterZ < gs.map.GetHhgt())
                        {
                            short zoneId = gs.md.mapZoneIds[loc.CenterX, loc.CenterZ];
                            if (zoneId == MapConstants.OceanZoneId)
                            {
                                continue;
                            }
                            zone = gs.map.Get<Zone>(zoneId);
                        }

                        if (zone == null)
                        {
                            zone = gs.map.Zones[0];
                        }
                       
                        if (zone != null && zone.IdKey == MapConstants.OceanZoneId)
                        {
                            continue;
                        }
                       

                        if (zone.Locations == null)
                        {
                            zone.Locations = new List<Location>();
                        }
                        if (!zone.Locations.Contains(loc))
                        {
                            gs.logger.Debug("Add Center to zone: " + zone.IdKey + " at " + loc.CenterX + " " + loc.CenterZ);
                            zone.Locations.Add(loc);
                        }
                        AddOneLocationPatch(gs, zone, loc);
                    }
                }
            }
        }
        await Task.CompletedTask;
    }

    public void AddOneLocationPatch(UnityGameState gs, Zone zone, Location loc)
    {
        if ( loc == null || zone == null)
        {
            return;
        }

        MyPointF size = new MyPointF(gs.map.GetHwid(), MapConstants.MapHeight, gs.map.GetHhgt());
        int edgeSize = MapConstants.LocCenterEdgeSize;

        if (size.X < LocationType.MinSize || size.Y <  LocationType.MinSize)
        {
            return;
        }

        if (loc.CenterX < edgeSize || loc.CenterZ < edgeSize || loc.CenterX > gs.map.GetHwid()-edgeSize || loc.CenterZ >= gs.map.GetHhgt() - edgeSize)
        {
            return;
        }

        MyRandom rand = new MyRandom(zone.Seed + loc.Seed);

        int awid = gs.md.awid;
        int ahgt = gs.md.ahgt;


        int cx = loc.CenterX;
        int cy = loc.CenterZ;


        int xsize = loc.XSize;
        int ysize = loc.ZSize;


        float midSize = (xsize + ysize) / 2.0f;

        float minHeight = 100000000;
        float maxHeight = -100000;
        

        float totalHeight = 0;

        int numCells = 0;

        // Get average height of location.
        for (int x = cx-xsize; x <= cx+xsize; x++)
        {
            if (x < 0 || x >= gs.map.GetHwid())
            {
                continue;
            }

            int dx = x - cx;
            for (int y = cy-ysize; y <= cy+ysize; y++)
            {
                if (y < 0 || y >= gs.map.GetHhgt())
                {
                    continue;
                }

                int dy = y - cy;

                float normDist = MathUtils.Sqrt((1.0f * (dx * dx)) / (xsize*xsize) + (1.0f * (dy * dy) / (ysize*ysize)));

                if (loc.IsRectangular() || normDist <= 1)
                {

                    float hgt = gs.md.heights[x, y];
                    totalHeight += hgt;
                    if (hgt > maxHeight)
                    {
                        maxHeight = hgt;
                    }

                    if (hgt < minHeight)
                    {
                        minHeight = hgt;
                    }

                    numCells++;
                }
            }
        }

        if (numCells < 1)
        {
            return;
        }

        MyRandom dirtRand = new MyRandom(loc.CenterX * 113 + loc.CenterZ * 8923);

        float aveHeight = totalHeight / numCells;


        // Used for moving from flat location to regular terrain patches.
        float maxNormDist = 4.0f;
        float minNormDist = 1.0f;

        int extraSize = (int)((loc.XSize + loc.ZSize) * maxNormDist/Math.Sqrt(2));

        // Slightly perturb the height of the location.
        float dz = extraSize / 24.0f;
        aveHeight += (MathUtils.FloatRange(-dz, dz, rand))/MapConstants.MapHeight;

        float heightDiff = maxHeight - minHeight;

        float heightPerturb = (midSize / 2)/MapConstants.MapHeight;

        aveHeight += MathUtils.FloatRange(-heightPerturb,heightPerturb, rand);

        float minRoadHeight = (MapConstants.MinLandHeight + 1.0f) / MapConstants.MapHeight;

        if (aveHeight < minRoadHeight)
        {
            aveHeight = minRoadHeight;
        }

        aveHeight *= MapConstants.MapHeight;
        aveHeight = ((int)(Math.Floor(aveHeight) + 0.3f));
        aveHeight /= MapConstants.MapHeight;

        int roadCheckRad = 5;

        int mapCenterToLocCenterX = Math.Abs(gs.map.GetHwid() / 2 - loc.CenterX);
        int mapCenterToLocCenterZ = Math.Abs(gs.map.GetHhgt() / 2 - loc.CenterZ);

        for (int x = cx - loc.XSize - extraSize; x <= cx + loc.XSize + extraSize; x++)
        {
            if (x < 0 || x >= gs.map.GetHwid())
            {
                continue;
            }

            float dx = Math.Abs(x - cx);
            for (int y = cy - loc.ZSize - extraSize; y <= cy + loc.ZSize + extraSize; y++)
            {
                if (y < 0 || y >= gs.map.GetHhgt())
                {
                    continue;
                }

                float dy = Math.Abs(y - cy);

                float normDist = MathUtils.Sqrt((1.0f * (dx * dx)) / (xsize * xsize) + ((1.0f * (dy * dy)) / (ysize * ysize)));

                // Near roads, scale the dx and dy a bit 
                if (normDist > 1)
                {
                    int xroadDelta = 0;
                    int yroadDelta = 0;

                    for (int xx = x-roadCheckRad; xx <= x+roadCheckRad; xx++)
                    {
                        if (xx < 0 || xx >= gs.map.GetHwid())
                        {
                            continue;
                        }

                        int drx = xx - x;
                        for (int yy = y - roadCheckRad; yy <= y + roadCheckRad; yy++)
                        {
                            if (yy < 0 || yy >= gs.map.GetHhgt())
                            {
                                continue;
                            }

                            int dry = yy - y;

                            double dist = Math.Sqrt(drx * drx + dry * dry);
                            if (gs.md.alphas[xx, yy, MapConstants.RoadTerrainIndex] > 0)
                            {
                                int dxx = Math.Abs(xx - cx);
                                if (dxx <= dx)
                                {
                                    xroadDelta--;
                                }
                                else if (dxx > dx)
                                {
                                    xroadDelta++;
                                }

                                int dyy = Math.Abs(yy - cy);
                                if (dyy <= dy)
                                {
                                    yroadDelta--;
                                }
                                else if (dyy > dy)
                                {
                                    yroadDelta++;
                                }
                            }
                        }
                    }
                    float deltaDiv = 8.0f;
                    dx += xroadDelta / deltaDiv;
                    dy += yroadDelta / deltaDiv;

                }
                
                normDist = MathUtils.Sqrt((1.0f * (dx * dx)) / (xsize * xsize) + ((1.0f * (dy * dy)) / (ysize * ysize)));

                double actualDist = Math.Sqrt(dx * dx + dy * dy);

                normDist = MathUtils.Clamp(minNormDist, normDist, maxNormDist);

                if (normDist <= minNormDist)
                {
                    gs.md.heights[x, y] = aveHeight;
                    gs.md.flags[x,y] |= MapGenFlags.IsLocation;
                }
                else
                {
                    float currHeight = gs.md.heights[x, y];
                    float diff = aveHeight - currHeight;

                    float changePct = (maxNormDist - normDist) / (maxNormDist - minNormDist);

                    if (loc.LocationTypeId == LocationType.Secondary && currHeight > aveHeight)
                    {
                        changePct *= 2.5f;
                        if (changePct > 1)
                        {
                            changePct = 1;
                        }
                    }

                    gs.md.heights[x, y] += diff * changePct;

                }
            }
        }
    }
}
	
