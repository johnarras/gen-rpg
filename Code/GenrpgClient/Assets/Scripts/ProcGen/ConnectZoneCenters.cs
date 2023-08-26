using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Entities;
using Genrpg.Shared.Utils;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.Shared.ProcGen.Entities;
using Services.ProcGen;
using System.Threading;

// Use greedy algo (Kruskal's?) to connect the centers.






public class ConnectZoneCenters : BaseZoneGenerator
{
    protected ILineGenService _lineGenService;

    public override async UniTask Generate(UnityGameState gs, CancellationToken token)
    {
        await base.Generate(gs, token);
        float[] extraConnectionsChances = { 0.5f, 0.2f, 0.1f };

        List<ConnectPointData> centers = new List<ConnectPointData>();
        MyRandom rand = new MyRandom(gs.map.Seed + 9923747);

        int zoneWidth = (int)(MapConstants.TerrainPatchSize * gs.map.ZoneSize);

        int edgeSize = MapConstants.TerrainPatchSize / 2;
        foreach (Location center in gs.md.zoneCenters)
        { 
            if (center.CenterX < edgeSize || center.CenterX > gs.map.GetHwid() -edgeSize ||
                center.CenterZ < edgeSize || center.CenterZ > gs.map.GetHhgt() - edgeSize)
            {
                continue;
            }
            ConnectPointData cpd = new ConnectPointData()
            {
                Id = center.Id,
                X = center.CenterX,
                Z = center.CenterZ,
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


        List<ConnectedPairData> roadsToMake = _lineGenService.ConnectPoints(gs, centers, rand, 0.1f);

        AddRoads rs = new AddRoads();
        gs.loc.Resolve(rs);
        foreach (ConnectedPairData rd in roadsToMake)
        {
            ConnectPointData center1 = rd.Point1;
            ConnectPointData center2 = rd.Point2;
            rs.AddRoad(gs, (int)center1.X, (int)center1.Z, (int)center2.X, (int)center2.Z, rand.Next(), rand, true);

        }

        gs.md.zoneConnections = roadsToMake;
        await UniTask.CompletedTask;
    }
}




