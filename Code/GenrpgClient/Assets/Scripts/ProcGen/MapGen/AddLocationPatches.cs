using System;
using System.Collections.Generic;


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
    private IAddRoadService _addRoadService = null;
    public override async Awaitable Generate(CancellationToken token)
    {
        await base.Generate(token);
        int edgeSize = MapConstants.LocCenterEdgeSize;
        MyRandom smoothnessRand = new MyRandom(_rand.NextLong());
        for (int pass = 0; pass < 2; pass++)
        {
            for (int gx = 0; gx < _md.locationGrid.GetLength(0); gx++)
            {
                for (int gy = 0; gy < _md.locationGrid.GetLength(1); gy++)
                {
                    if (_md.locationGrid[gx, gy] == null)
                    {
                        continue;
                    }

                    foreach (Location loc in _md.locationGrid[gx, gy])
                    {
                        if ((pass == 0) != (loc.LocationTypeId == LocationTypes.ZoneCenter))
                        {
                            continue;
                        }
                        Zone zone = null;

                        if (loc.CenterX < edgeSize || loc.CenterX >= _mapProvider.GetMap().GetHwid()-edgeSize ||
                            loc.CenterZ < edgeSize || loc.CenterZ >= _mapProvider.GetMap().GetHhgt()-edgeSize)
                        {
                            continue;
                        }

                        if (loc.CenterX >= 0 && loc.CenterX < _mapProvider.GetMap().GetHwid() &&
                            loc.CenterZ >= 0 && loc.CenterZ < _mapProvider.GetMap().GetHhgt())
                        {
                            short zoneId = _md.mapZoneIds[loc.CenterX, loc.CenterZ];
                            if (zoneId == MapConstants.OceanZoneId)
                            {
                                continue;
                            }
                            zone = _mapProvider.GetMap().Get<Zone>(zoneId);
                        }

                        if (zone == null)
                        {
                            zone = _mapProvider.GetMap().Zones[0];
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
                            zone.Locations.Add(loc);
                        }
                        loc.ZoneId = zone.IdKey;
                        AddOneLocationPatch(loc, MathUtils.FloatRange(0.9f, 1.0f, smoothnessRand));
                    }
                }
            }
        }
        
    }

    public void AddOneLocationPatch(Location loc, float flattenFraction)
    {
        Zone zone = _mapProvider.GetMap().Get<Zone>(loc.ZoneId);

        flattenFraction = MathUtils.Clamp(0, flattenFraction, 1);

        if (zone== null)
        {
            return;
        }

        int edgeSize = MapConstants.LocCenterEdgeSize;

        if (loc.CenterX < edgeSize || loc.CenterZ < edgeSize || loc.CenterX > _mapProvider.GetMap().GetHwid()-edgeSize || loc.CenterZ >= _mapProvider.GetMap().GetHhgt() - edgeSize)
        {
            return;
        }

        MyRandom rand = new MyRandom(zone.Seed + loc.Seed);

        int awid = _md.awid;
        int ahgt = _md.ahgt;


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

        if (loc.LocationTypeId == LocationTypes.Patch)
        {
            fullXSize = xsize;
            fullZSize = zsize;
        }

        // Get average height of location.
        for (int x = centerX-fullXSize; x <= centerX+fullXSize; x++)
        {
            if (x < 0 || x >= _mapProvider.GetMap().GetHwid())
            {
                continue;
            }

            int dx = x - centerX;
            for (int z = centerZ-fullZSize; z <= centerZ+fullZSize; z++)
            {
                if (z < 0 || z >= _mapProvider.GetMap().GetHhgt())
                {
                    continue;
                }

                int dz = z - centerZ;

                float normDist = MathUtils.Sqrt((1.0f * (dx * dx)) / (fullXSize*fullXSize) + (1.0f * (dz * dz) / (fullZSize*fullZSize)));

                if (normDist <= 1)
                {
                    float hgt = Mathf.Max(MapConstants.StartHeightPercent, _md.heights[x, z]);
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

        fullXSize = xsize + extraXSize;
        fullZSize = zsize + extraZSize;

        if (cellCount < 1)
        {
            return;
        }

        float aveHeight = totalHeight / cellCount;

        // Slightly perturb the height of the location.
        float dVert = 0;// (fullXSize+fullZSize) / 50.0f;
        aveHeight += (MathUtils.FloatRange(-dVert, dVert, rand))/MapConstants.MapHeight;

        float heightPerturb = (midSize / 2)/MapConstants.MapHeight;

        aveHeight += MathUtils.FloatRange(-heightPerturb,heightPerturb, rand);


        aveHeight *= MapConstants.MapHeight;
        aveHeight = ((int)(Math.Floor(aveHeight) + 0.3f));
        aveHeight /= MapConstants.MapHeight;

        // Pass 1 set center heights.
        for (int x = centerX - fullXSize; x <= centerX + fullXSize; x++)
        {
            if (x < 0 || x >= _mapProvider.GetMap().GetHwid())
            {
                continue;
            }

            float dx = Math.Abs(x - centerX);
            for (int z = centerZ - fullZSize; z <= centerZ + fullZSize; z++)
            {
                if (z < 0 || z >= _mapProvider.GetMap().GetHhgt())
                {
                    continue;
                }

                float dz = Math.Abs(z - centerZ);

                float outsideCenterDist = MathUtils.Sqrt((1.0f * (dx * dx)) / (xsize * xsize) + ((1.0f * (dz * dz)) / (zsize * zsize)));

                outsideCenterDist = MathUtils.Clamp(minNormDist, outsideCenterDist, maxNormDist);

                // Starts at 1 near center, goes to 0 near edge.
                float baseChangePct = MathF.Min(1, (maxNormDist - outsideCenterDist) / (maxNormDist - minNormDist));

                float changePct = MathUtils.EaseInOut(baseChangePct);

                float currFlattenFraction = flattenFraction;

                float roadDistance = _md.roadDistances[x, z];

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

                _md.heights[x, z] += (aveHeight - _md.heights[x, z]) * finalOffsetPercent;

                if (false && changePct == 1 && loc.LocationTypeId == LocationTypes.Patch)
                {
                    _md.ClearAlphasAt(x, z);
                    float dirtPct = MathUtils.FloatRange(0.9f, 1.0f, rand);
                    _md.alphas[x, z, MapConstants.DirtTerrainIndex] = dirtPct;
                    _md.alphas[x, z, MapConstants.BaseTerrainIndex] = 1 - dirtPct;
                }

                if (outsideCenterDist <= minNormDist)
                {
                    _md.flags[x,z] |= MapGenFlags.IsLocation;
                    if (loc.LocationTypeId == LocationTypes.Patch)
                    {
                        _md.flags[x, z] |= MapGenFlags.IsLocationPatch;
                    }
                }
            }
        }

        if (loc.LocationTypeId != LocationTypes.Patch)
        {
            int placeCount = rand.Next(10, 20);

            while (rand.NextDouble() < 0.2f && placeCount < 100)
            {
                placeCount *= 2;
                placeCount += rand.Next(0, placeCount / 2);
            }

            List<LocationPlace> places = new List<LocationPlace>();

            int xSearchSize = xsize;
            int zSearchSize = zsize;
            int minPatchSize = 6;
            int locPlaceId = 0;
            for (int i = 0; i < placeCount*100; i++)
            {
                int patchXSize = MathUtils.IntRange(minPatchSize, minPatchSize*3/2, rand);
                int patchZSize = MathUtils.IntRange(minPatchSize, minPatchSize*3/2, rand);
                int xgap = patchXSize * 6 / 2;
                int zgap = patchZSize * 6 / 2;

                int nx = MathUtils.IntRange(loc.CenterX - xSearchSize, loc.CenterX + xSearchSize, rand);
                int nz = MathUtils.IntRange(loc.CenterZ - zSearchSize, loc.CenterZ + zSearchSize, rand);

                if (nx < MapConstants.MapEdgeSize || nx >= _mapProvider.GetMap().GetHwid()-MapConstants.MapEdgeSize || 
                    nz <  MapConstants.MapEdgeSize || nz >= _mapProvider.GetMap().GetHhgt()-MapConstants.MapEdgeSize)
                {
                    continue;
                }

                if (Math.Abs(nx - loc.CenterX) <= patchXSize * 3 / 2 &&
                    Math.Abs(nz - loc.CenterZ) <= patchZSize * 3 / 2)
                {
                    continue;
                }

                int dnx = Math.Abs(nx - centerX);
                int dnz = Math.Abs(nz - centerZ);

                float dxpct = 1.0f * dnx / xsize;
                float dzpct = 1.0f * dnz / zsize;

                // Need the point to be within the location oval.
                if (dxpct * dxpct + dzpct * dzpct > 1)
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
                    LocationPlace newPlace = new LocationPlace() 
                    { 
                        Id = loc.Id + (++locPlaceId), 
                        LocationId = loc.Id,
                        CenterX = nx, 
                        CenterZ = nz, 
                        XSize = patchXSize, 
                        ZSize = patchZSize,
                    };
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

                    AddOneLocationPatch(patchLoc, 1.0f);

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

            MyRandom connectRand = new MyRandom(_rand.Next());

            List<ConnectedPairData> roadsToMake = _lineGenService.ConnectPoints(connectPoints, connectRand, 0.1f);


            foreach (LocationPlace place in loc.Places)
            {
                roadsToMake.Add(new ConnectedPairData()
                {
                    Point1 = new ConnectPointData() { X = place.CenterX, Z = place.CenterZ },
                    Point2 = new ConnectPointData() { X = place.EntranceX, Z = place.EntranceZ },
                });
            }

            foreach (ConnectedPairData rd in roadsToMake)
            {
                ConnectPointData center1 = rd.Point1;
                ConnectPointData center2 = rd.Point2;
                _addRoadService.AddRoad((int)center1.X, (int)center1.Z, (int)center2.X, (int)center2.Z, connectRand.Next(), rand, false, 1, MapGenFlags.MinorRoad);
            }
        }
    }
}
	

