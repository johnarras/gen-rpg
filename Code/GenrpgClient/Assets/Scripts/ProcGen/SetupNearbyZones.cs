using System.Threading.Tasks;
using Genrpg.Shared.Zones.Entities;
using Genrpg.Shared.ProcGen.Entities;

using System.Threading;
using System;

public class SetupNearbyZones : BaseAddMountains
{

    protected IMapGenService _mapGenService;
    public override async Task Generate(UnityGameState gs, CancellationToken token)
    {
        await Task.CompletedTask;

        foreach (ConnectedPairData conn in gs.md.zoneConnections)
        {
            if (conn.Point1 == null || conn.Point2 == null)
            {
                continue;
            }

            int sx = (int)conn.Point1.X;
            int sz = (int)conn.Point1.Z;
            int ex = (int)conn.Point2.X;
            int ez = (int)conn.Point2.Z;

            if (sx < 0 || sz < 0 || ex < 0 || ez < 0 || 
                sx >= gs.map.GetHwid() || sz >= gs.map.GetHhgt() ||
                ex >= gs.map.GetHwid() || ez >= gs.map.GetHhgt())
            {
                continue;
            }

            short zoneId1 = gs.md.mapZoneIds[sx, sz];
            short zoneId2 = gs.md.mapZoneIds[ex, ez];

            if (zoneId1 != zoneId2)
            {
                Zone zone1 = gs.map.Get<Zone>(zoneId1);
                Zone zone2 = gs.map.Get<Zone>(zoneId2);

                GenZone genZone1 = gs.GetGenZone(zone1.IdKey);
                GenZone genZone2 = gs.GetGenZone(zone2.IdKey);

                if (zone1 != null && zone2 != null)
                {
                    int xmid1 = (zone1.XMin + zone1.XMax) / 2;
                    int ymid1 = (zone1.ZMin + zone1.ZMax) / 2;
                    int xmid2 = (zone2.XMin + zone2.XMax) / 2;
                    int ymid2 = (zone2.ZMin + zone2.ZMax) / 2;
                    int dx = xmid1 - xmid2;
                    int dy = ymid1 - ymid2;
                    float dist = (float)Math.Sqrt(dx * dx + dy * dy);
                    genZone1.AddNearbyZone(gs, zone2, dist);
                    genZone2.AddNearbyZone(gs, zone1, dist);
                }
                
            }

        }
        _mapGenService.SetPrevNextZones(gs);
    }


}

