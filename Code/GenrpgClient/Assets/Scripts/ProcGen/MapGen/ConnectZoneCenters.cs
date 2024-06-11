using System;
using System.Collections.Generic;
using Genrpg.Shared.Utils;
using Genrpg.Shared.ProcGen.Entities;
using System.Threading;
using Genrpg.Shared.Utils.Data;
using UnityEngine;

// Use greedy algo (Kruskal's?) to connect the centers.

public class ConnectZoneCenters : BaseZoneGenerator
{
    protected ILineGenService _lineGenService;
    private IAddRoadService _addRoadService;

    public override async Awaitable Generate(CancellationToken token)
    {
        await base.Generate(token);
        float[] extraConnectionsChances = { 0.5f, 0.2f, 0.1f };

        List<ConnectPointData> centers = new List<ConnectPointData>();
        MyRandom rand = new MyRandom(_mapProvider.GetMap().Seed + 9923747);

        int zoneWidth = (int)(MapConstants.TerrainPatchSize * _mapProvider.GetMap().ZoneSize);

        int edgeSize = MapConstants.TerrainPatchSize;
        int centerId = 0;
        foreach (MyPoint center in _md.zoneCenters)
        { 
            if (center.X < edgeSize || center.X > _mapProvider.GetMap().GetHwid() -edgeSize ||
                center.Y < edgeSize || center.Y > _mapProvider.GetMap().GetHhgt() -edgeSize)
            {
                continue;
            }
            ConnectPointData cpd = new ConnectPointData()
            {
                Id = (++centerId),
                X = center.X,
                Z = center.Y,
                Data = center,
                MaxConnections = 3,
            };
            centers.Add(cpd);

            for (int c = 0; c < extraConnectionsChances.Length; c++)
            {
                if (rand.NextDouble() < extraConnectionsChances[c])
                {
                    cpd.MaxConnections++;
                }
                else
                {
                    break;
                }
            }
        }


        List<ConnectedPairData> roadsToMake = _lineGenService.ConnectPoints(centers, rand, 0.1f);

        foreach (ConnectedPairData rd in roadsToMake)
        {
            ConnectPointData center1 = rd.Point1;
            ConnectPointData center2 = rd.Point2;
            _addRoadService.AddRoad((int)center1.X, (int)center1.Z, (int)center2.X, (int)center2.Z, rand.Next(), rand, true);

        }

        _md.zoneConnections = roadsToMake;
        
    }
}




