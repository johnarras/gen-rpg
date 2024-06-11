
using Genrpg.Shared.Zones.Entities;
using Genrpg.Shared.ProcGen.Entities;

using System.Threading;
using System;
using Genrpg.Shared.Zones.WorldData;
using UnityEngine;

public class SetupNearbyZones : BaseAddMountains
{

    protected IMapGenService _mapGenService;
    public override async Awaitable Generate(CancellationToken token)
    {
        

        foreach (ConnectedPairData conn in _md.zoneConnections)
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
                sx >= _mapProvider.GetMap().GetHwid() || sz >= _mapProvider.GetMap().GetHhgt() ||
                ex >= _mapProvider.GetMap().GetHwid() || ez >= _mapProvider.GetMap().GetHhgt())
            {
                continue;
            }

            short zoneId1 = _md.mapZoneIds[sx, sz];
            short zoneId2 = _md.mapZoneIds[ex, ez];

            if (zoneId1 != zoneId2)
            {
                Zone zone1 = _mapProvider.GetMap().Get<Zone>(zoneId1);
                Zone zone2 = _mapProvider.GetMap().Get<Zone>(zoneId2);

                GenZone genZone1 = _md.GetGenZone(zone1.IdKey);
                GenZone genZone2 = _md.GetGenZone(zone2.IdKey);

                if (zone1 != null && zone2 != null)
                {
                    int xmid1 = (zone1.XMin + zone1.XMax) / 2;
                    int ymid1 = (zone1.ZMin + zone1.ZMax) / 2;
                    int xmid2 = (zone2.XMin + zone2.XMax) / 2;
                    int ymid2 = (zone2.ZMin + zone2.ZMax) / 2;
                    int dx = xmid1 - xmid2;
                    int dy = ymid1 - ymid2;
                    float dist = (float)Math.Sqrt(dx * dx + dy * dy);
                    genZone1.AddNearbyZone(zone2, dist);
                    genZone2.AddNearbyZone(zone1, dist);
                }
                
            }

        }
        _mapGenService.SetPrevNextZones(_gs);
    }


}

