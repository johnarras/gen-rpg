using System;
using System.Collections.Generic;

using Genrpg.Shared.Utils;
using System.Threading;
using Genrpg.Shared.ProcGen.Settings.Locations;
using Genrpg.Shared.ProcGen.Settings.Locations.Constants;
using UnityEngine;

// Connect these zone centers to "closest object.

public class ConnectSecondaryLocations : BaseZoneGenerator
{
    private IAddRoadService _addRoadService = null;
    public override async Awaitable Generate(CancellationToken token)
    {
        await base.Generate(token);

        // This can happen if there are no secondary locations added.
        if (_md.locationGrid == null)
        {
            return;
        }

        MyRandom rand = new MyRandom(_mapProvider.GetMap().Seed/2 + 9977747);

        List<Location> locations = new List<Location>();

        for (int x = 0; x < _md.locationGrid.GetLength(0); x++)
        {
            for (int y = 0; y < _md.locationGrid.GetLength(1); y++)
            {
                if (_md.locationGrid[x, y] == null)
                {
                    continue;
                }
                foreach (Location loc in _md.locationGrid[x, y])
                {
                    if (loc.LocationTypeId != LocationTypes.ZoneCenter)
                    {
                        locations.Add(loc);
                    }
                }
            }
        }

        while (locations.Count > 0)
        {
            int pos = rand.Next() % locations.Count;
            Location loc = locations[pos];
            locations.RemoveAt(pos);

            int radiusStart = 10;
            int radiusEnd = 300;
            int rskip = 2;

            int roadx = -1;
            int roady = -1;
            double minRoadDist = 1000000;

            int cx = loc.CenterX;
            int cy = loc.CenterZ;
            for (int r = radiusStart; r <= radiusEnd; r += rskip)
            {
                if (roadx >= 0 && roady >= 0 && r > minRoadDist*5/4)
                {
                    break;
                }
                int rad = r / 2;
                int[] yvals = new int[] { cy - rad, cy + rad };

                foreach (int y in yvals)
                {
                    if (y >= 0 && y < _md.ahgt)
                    {
                        int dy = y - cy;
                        for (int x = cx - rad; x <= cx + rad; x += rskip)
                        {
                            if (x < 0 || x >= _md.awid)
                            {
                                continue;
                            }

                            if (_md.alphas[x, y, MapConstants.RoadTerrainIndex] > 0)
                            {
                                int dx = x - cx;
                                double dist = Math.Sqrt(dx * dx + dy * dy);
                                if (dist < minRoadDist)
                                {
                                    minRoadDist = dist;
                                    roadx = x;
                                    roady = y;
                                }
                            }
                        }
                    }
                }

                int[] xvals = new int[] { cx - rad, cx + rad };

                foreach (int x in xvals)
                {
                    if (x >= 0 && x < _md.awid)
                    {
                        int dx = x - cx;
                        for (int y = cy - rad; y <= cy+rad; y+= rskip)
                        {
                            if (y < 0 || y >= _md.ahgt)
                            {
                                continue;
                            }

                            if (_md.alphas[x, y, MapConstants.RoadTerrainIndex] > 0)
                            {
                                int dy = y - cy;
                                double dist = Math.Sqrt(dx * dx + dy * dy);
                                if (dist < minRoadDist)
                                {
                                    minRoadDist = dist;
                                    roadx = x;
                                    roady = y;
                                }
                            }
                        }
                    }
                }
            }

            if (roadx > 0 && roady > 0)
            {
                _addRoadService.AddRoad(cx, cy, roadx, roady, cx * 31 + cy * 37 + _mapProvider.GetMap().Seed / 3, rand, false);
            }

        }

        
    }
}




