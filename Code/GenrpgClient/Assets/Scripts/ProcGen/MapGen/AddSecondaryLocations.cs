using System;
using Cysharp.Threading.Tasks;
using Genrpg.Shared.Utils;
using System.Threading;
using Genrpg.Shared.ProcGen.Settings.Locations;
using Genrpg.Shared.ProcGen.Settings.Locations.Constants;
using Genrpg.Shared.Zones.WorldData;

public class AddSecondaryLocations : BaseZoneGenerator
{
    public override async UniTask Generate(UnityGameState gs, CancellationToken token)
    {
        await base.Generate(gs, token);
        PlaceOtherLocations(gs);
    }

    private void PlaceOtherLocations (UnityGameState gs)
    {
        long locationsDesired = (int)(gs.map.BlockCount * gs.map.BlockCount * 0.05f);

        MyRandom rand = new MyRandom(gs.map.Seed % 1000000000 + 176283);


        if (locationsDesired < 1)
        {
            locationsDesired = 1;
        }
        locationsDesired = Math.Max(1, locationsDesired / 2 + rand.Next() % (locationsDesired + 1));

        int edgeDistance = 2 * MapConstants.TerrainPatchSize;

        if (gs.map.GetHwid() <= edgeDistance*2 || gs.map.GetHhgt() <= edgeDistance*2)
        {
            return;
        }

        int minDistToFeature = 40;

        long locationsPlaced = 0;

        int mountainCheckRadius = 50;

        for (int times = 0; times < locationsDesired*100 && locationsPlaced < locationsDesired; times++)
        {
            int cx = edgeDistance + rand.Next() % (gs.map.GetHwid() - 2*edgeDistance);
            int cy = edgeDistance + rand.Next() % (gs.map.GetHhgt() - 2*edgeDistance);

            // Not near current loc.
            Location nearLoc = _zoneGenService.FindMapLocation(gs, cx, cy, minDistToFeature);
            if (nearLoc != null)
            {
                continue;
            }

            bool failed = false;
            for (int xx = cx - mountainCheckRadius; xx <= cx + mountainCheckRadius; xx++)
            {
                if (xx < 0 || xx >= gs.map.GetHwid())
                {
                    continue;
                }

                for (int yy = cy - mountainCheckRadius; yy <= cy + mountainCheckRadius; yy++)
                {
                    if (yy < 0 || yy >= gs.map.GetHhgt())
                    {
                        continue;
                    }

                    if (FlagUtils.IsSet(gs.md.flags[xx,yy],MapGenFlags.IsEdgeWall))
                    {
                        failed = true;
                    }
                }
            }

            if (failed)
            {
                continue;
            }

            if (gs.md.roadDistances[cx,cy] < minDistToFeature)
            {
                continue;
            }

            if (gs.md.mapZoneIds[cx,cy] < MapConstants.MapZoneStartId)
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

            gs.md.AddMapLocation(gs, loc);
            locationsPlaced++;
        }
    }


}

