using System;

using Genrpg.Shared.Utils;
using System.Threading;
using Genrpg.Shared.ProcGen.Settings.Locations;
using Genrpg.Shared.ProcGen.Settings.Locations.Constants;
using UnityEngine;
using Genrpg.Shared.Client.Core;

public class AddSecondaryLocations : BaseZoneGenerator
{
    public override async Awaitable Generate(CancellationToken token)
    {
        await base.Generate(token);
        PlaceOtherLocations(_gs);
    }

    private void PlaceOtherLocations (IClientGameState gs)
    {
        long locationsDesired = (int)(_mapProvider.GetMap().BlockCount * _mapProvider.GetMap().BlockCount * 0.05f);

        MyRandom rand = new MyRandom(_mapProvider.GetMap().Seed % 1000000000 + 176283);


        if (locationsDesired < 1)
        {
            locationsDesired = 1;
        }
        locationsDesired = Math.Max(1, locationsDesired / 2 + rand.Next() % (locationsDesired + 1));

        int edgeDistance = 2 * MapConstants.TerrainPatchSize;

        if (_mapProvider.GetMap().GetHwid() <= edgeDistance*2 || _mapProvider.GetMap().GetHhgt() <= edgeDistance*2)
        {
            return;
        }

        int minDistToFeature = 40;

        long locationsPlaced = 0;

        int mountainCheckRadius = 50;

        for (int times = 0; times < locationsDesired*100 && locationsPlaced < locationsDesired; times++)
        {
            int cx = edgeDistance + rand.Next() % (_mapProvider.GetMap().GetHwid() - 2*edgeDistance);
            int cy = edgeDistance + rand.Next() % (_mapProvider.GetMap().GetHhgt() - 2*edgeDistance);

            // Not near current loc.
            Location nearLoc = _zoneGenService.FindMapLocation(cx, cy, minDistToFeature);
            if (nearLoc != null)
            {
                continue;
            }

            bool failed = false;
            for (int xx = cx - mountainCheckRadius; xx <= cx + mountainCheckRadius; xx++)
            {
                if (xx < 0 || xx >= _mapProvider.GetMap().GetHwid())
                {
                    continue;
                }

                for (int yy = cy - mountainCheckRadius; yy <= cy + mountainCheckRadius; yy++)
                {
                    if (yy < 0 || yy >= _mapProvider.GetMap().GetHhgt())
                    {
                        continue;
                    }

                    if (FlagUtils.IsSet(base._md.flags[xx, yy], MapGenFlags.IsEdgeWall))
                    {
                        failed = true;
                    }
                }
            }

            if (failed)
            {
                continue;
            }

            if (base._md.roadDistances[cx, cy] < minDistToFeature)
            {
                continue;
            }

            if (base._md.mapZoneIds[cx, cy] < MapConstants.MapZoneStartId)
            {
                continue;
            }

            int minRad = 5;
            int maxRad = 10;

            if (rand.NextDouble() < 0.2f)
            {
                minRad *= 2;
                maxRad *= 2;                
            }

            Location loc = new Location()
            {
                CenterX = cx,
                CenterZ = cy,
                LocationTypeId = LocationTypes.Secondary,
                XSize = MathUtils.IntRange(minRad, maxRad, rand),
                ZSize = MathUtils.IntRange(minRad, maxRad, rand),        
            };

            base._md.AddMapLocation(_mapProvider, loc);
            locationsPlaced++;
        }
    }


}

