using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

using Genrpg.Shared.Utils;
using System.Threading;
using Genrpg.Shared.ProcGen.Settings.Locations;
using Genrpg.Shared.Zones.WorldData;
using Genrpg.Shared.ProcGen.Settings.Locations.Constants;
using Genrpg.Shared.ProcGen.Entities;
using UnityEngine;
using Genrpg.Shared.ProcGen.Settings.LineGen;
using Genrpg.Shared.Utils.Data;

public class AddLocationPatches : BaseZoneGenerator
{
    private ILineGenService _lineGenService = null;
    AddRoads _addRoads = null;
    public override async UniTask Generate(UnityGameState gs, CancellationToken token)
    {
        _addRoads = new AddRoads();
        gs.loc.Resolve(_addRoads);
        await base.Generate(gs, token);
        int edgeSize = MapConstants.LocCenterEdgeSize;
        MyRandom smoothnessRand = new MyRandom(gs.rand.NextLong());
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
                        if ((pass == 0) != (loc.LocationTypeId == LocationTypes.ZoneCenter))
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
                        loc.ZoneId = zone.IdKey;
                        AddOneLocationPatch(gs, loc, MathUtils.FloatRange(0.7f,1.0f, smoothnessRand));
                    }
                }
            }
        }
        await UniTask.CompletedTask;
    }

    public void AddOneLocationPatch(UnityGameState gs, Location loc, float flattenFraction)
    {
        Zone zone = gs.map.Get<Zone>(loc.ZoneId);

        flattenFraction = MathUtils.Clamp(0, flattenFraction, 1);

        if (zone== null)
        {
            return;
        }

        int edgeSize = MapConstants.LocCenterEdgeSize;

        if (loc.CenterX < edgeSize || loc.CenterZ < edgeSize || loc.CenterX > gs.map.GetHwid()-edgeSize || loc.CenterZ >= gs.map.GetHhgt() - edgeSize)
        {
            return;
        }

        MyRandom rand = new MyRandom(zone.Seed + loc.Seed);

        int awid = gs.md.awid;
        int ahgt = gs.md.ahgt;


        int centerX = loc.CenterX;
        int centerZ = loc.CenterZ;


        int xsize = loc.XSize;
        int zsize = loc.ZSize;


        float midSize = (xsize + zsize) / 2.0f;

        float minHeight = 100000000;
        float maxHeight = -100000;
        float totalHeight = 0;
        int cellCount = 0;

        // Used for moving from flat location to regular terrain patches.
        float maxNormDist = 2.5f;
        float minNormDist = 1.0f;

        double extraSizeScale = maxNormDist;

        int extraXSize = (int)(loc.XSize * extraSizeScale);
        int extraZSize = (int)(loc.ZSize * extraSizeScale);

        int fullXSize = xsize + extraXSize;
        int fullZSize = zsize + extraZSize;

        // Get average height of location.
        for (int x = centerX-fullXSize; x <= centerX+fullXSize; x++)
        {
            if (x < 0 || x >= gs.map.GetHwid())
            {
                continue;
            }

            int dx = x - centerX;
            for (int z = centerZ-fullZSize; z <= centerZ+fullZSize; z++)
            {
                if (z < 0 || z >= gs.map.GetHhgt())
                {
                    continue;
                }

                int dz = z - centerZ;

                float normDist = MathUtils.Sqrt((1.0f * (dx * dx)) / (fullXSize*fullXSize) + (1.0f * (dz * dz) / (fullZSize*fullZSize)));

                if (normDist <= 1)
                {
                    float hgt = gs.md.heights[x, z];
                    totalHeight += hgt;
                    if (hgt > maxHeight)
                    {
                        maxHeight = hgt;
                    }

                    if (hgt < minHeight)
                    {
                        minHeight = hgt;
                    }

                    cellCount++;
                }
            }
        }

        if (cellCount < 1)
        {
            return;
        }

        float aveHeight = totalHeight / cellCount;

        // Slightly perturb the height of the location.
        float dVert = (fullXSize+fullZSize) / 50.0f;
        aveHeight += (MathUtils.FloatRange(-dVert, dVert, rand))/MapConstants.MapHeight;

        float heightPerturb = (midSize / 2)/MapConstants.MapHeight;

        aveHeight += MathUtils.FloatRange(-heightPerturb,heightPerturb, rand);


        aveHeight *= MapConstants.MapHeight;
        aveHeight = ((int)(Math.Floor(aveHeight) + 0.3f));
        aveHeight /= MapConstants.MapHeight;

        // Pass 1 set center heights.
        for (int x = centerX - fullXSize; x <= centerX + fullXSize; x++)
        {
            if (x < 0 || x >= gs.map.GetHwid())
            {
                continue;
            }

            float dx = Math.Abs(x - centerX);
            for (int z = centerZ - fullZSize; z <= centerZ + fullZSize; z++)
            {
                if (z < 0 || z >= gs.map.GetHhgt())
                {
                    continue;
                }

                float dz = Math.Abs(z - centerZ);

                float outsideCenterDist = MathUtils.Sqrt((1.0f * (dx * dx)) / (xsize * xsize) + ((1.0f * (dz * dz)) / (zsize * zsize)));

                outsideCenterDist = MathUtils.Clamp(minNormDist, outsideCenterDist, maxNormDist);

                float changePct = MathF.Min(1, (maxNormDist - outsideCenterDist) / (maxNormDist - minNormDist));

                float currFlattenFraction = flattenFraction;

                float roadDistance = gs.md.roadDistances[x, z];

                float roadDistCheck = 15;
                float roadFixFlattenPct = 1.0f;
                if (roadDistance < roadDistCheck)
                {
                    currFlattenFraction += (1 - flattenFraction) * roadFixFlattenPct;
                }
                else if (roadDistance < roadDistCheck*2.0f)
                {
                    currFlattenFraction += ((roadDistCheck * 2.0f - roadDistCheck) / roadDistCheck) * ((1 - flattenFraction) * roadFixFlattenPct);
                }

                float finalOffsetPercent = currFlattenFraction * changePct;

                gs.md.heights[x, z] += (aveHeight - gs.md.heights[x, z]) * finalOffsetPercent;

                if (changePct == 1 && loc.LocationTypeId == LocationTypes.Patch)
                {
                    gs.md.ClearAlphasAt(gs, x, z);
                    float dirtPct = MathUtils.FloatRange(0.9f, 1.0f, rand);
                    gs.md.alphas[x, z, MapConstants.DirtTerrainIndex] = dirtPct;
                    gs.md.alphas[x, z, MapConstants.BaseTerrainIndex] = 1 - dirtPct;
                }

                if (outsideCenterDist <= minNormDist)
                {
                    gs.md.flags[x,z] |= MapGenFlags.IsLocation;
                    if (loc.LocationTypeId == LocationTypes.Patch)
                    {
                        gs.md.flags[x, z] |= MapGenFlags.IsLocationPatch;
                    }
                }
            }
        }

        if (loc.LocationTypeId != LocationTypes.Patch)
        {
            int placeCount = rand.Next(7, 10);
            List<LocationPlace> places = new List<LocationPlace>();

            int xSearchSize = xsize*4/5;
            int zSearchSize = zsize*4/5;
            int minPatchSize = 12;
            for (int i = 0; i < placeCount*50; i++)
            {
                int patchXSize = MathUtils.IntRange(minPatchSize, minPatchSize*3/2, rand);
                int patchZSize = MathUtils.IntRange(minPatchSize, minPatchSize*3/2, rand);
                int xgap = patchXSize * 6 / 2;
                int zgap = patchZSize * 6 / 2;

                int nx = MathUtils.IntRange(loc.CenterX - xSearchSize, loc.CenterX + xSearchSize, rand);
                int nz = MathUtils.IntRange(loc.CenterZ - zSearchSize, loc.CenterZ + zSearchSize, rand);

                if (nx < 0 || nx >= gs.map.GetHwid() || nz < 0 || nz >= gs.map.GetHhgt())
                {
                    continue;
                }

                bool nearPlace = false;
                foreach (LocationPlace lplace in places)
                {
                    if (Math.Abs(lplace.CenterX-nx) < xgap && Math.Abs(lplace.CenterZ-nz) < zgap)
                    {
                        nearPlace = true;
                        break;
                    }
                }

                if (!nearPlace && loc.LocationTypeId != LocationTypes.Patch)
                {
                    LocationPlace newPlace = new LocationPlace() { CenterX = nx, CenterZ = nz, XSize = patchXSize, ZSize = patchZSize };
                    places.Add(newPlace);

                    Location patchLoc = new Location()
                    {
                        CenterX = nx,
                        CenterZ = nz,
                        XSize = patchXSize,
                        ZSize = patchZSize,
                        ZoneId = loc.ZoneId,
                        LocationTypeId = LocationTypes.Patch,
                    };

                    AddOneLocationPatch(gs, patchLoc, 1.0f);

                }

                if (places.Count >= placeCount)
                {
                    break;
                }
            }

            loc.Places = places;

            int placeId = 0;

            List<ConnectPointData> connectPoints = new List<ConnectPointData>();
            foreach (LocationPlace place in places)
            {
                float dcx = loc.CenterX - place.CenterX;
                float dcz = loc.CenterZ - place.CenterZ;

                float dsx = place.XSize;
                float dsz = place.ZSize;

                float placedist = (float)Math.Sqrt(dsx * dsx + dsz * dsz);
                float totalDist = (float)Math.Sqrt(dcx * dcx + dcz * dcz);

                float ratio = placedist / totalDist;



                int roadx = (int)(place.CenterX + dcx * ratio);
                int roadz = (int)(place.CenterZ + dcz * ratio);

                place.EntranceX = roadx;
                place.EntranceZ = roadz;

                connectPoints.Add(new ConnectPointData()
                {
                    Id = ++placeId,
                    X = place.EntranceX,
                    Z = place.EntranceZ,
                    MaxConnections = 3,
                    MinDistToOther = 0,
                });
                
            }

            connectPoints.Add(new ConnectPointData()
            {
                X = loc.CenterX,
                Z = loc.CenterZ,
                MaxConnections = 3,
                MinDistToOther = 0,
            });

            MyRandom connectRand = new MyRandom(gs.rand.Next());

            List<ConnectedPairData> roadsToMake = _lineGenService.ConnectPoints(gs, connectPoints, connectRand, 0.0f);

            foreach (ConnectedPairData rd in roadsToMake)
            {
                ConnectPointData center1 = rd.Point1;
                ConnectPointData center2 = rd.Point2;
                _addRoads.AddRoad(gs, (int)center1.X, (int)center1.Z, (int)center2.X, (int)center2.Z, connectRand.Next(), rand, true);

            }

        }
    }
}
	

