
using Cysharp.Threading.Tasks;
using Genrpg.Shared.Zones.Entities;
using Genrpg.Shared.ProcGen.Entities;
using System.Threading;

public class SetMapSpawnPoint : BaseZoneGenerator
{
    public override async UniTask Generate(UnityGameState gs, CancellationToken token)
    {
        await base.Generate(gs, token);
        int minx = gs.map.GetHwid() / 2;
        int miny = gs.map.GetHhgt() / 2;
        long minDist = 1000000000;
        Zone minZone = null;
        int edgeSize = MapConstants.LocCenterEdgeSize;
        foreach (Zone zone in gs.map.Zones)
        {
            foreach (Location loc in zone.Locations)
            {
                if (loc.CenterX < edgeSize || loc.CenterX >= gs.map.GetHwid() - edgeSize ||
                    loc.CenterZ < edgeSize || loc.CenterZ >= gs.map.GetHhgt() - edgeSize)
                {
                    continue;
                }
                

                long currDist = loc.CenterX * loc.CenterX + loc.CenterZ * loc.CenterZ;
                if (currDist < minDist)
                {
                    minDist = currDist;
                    minx = loc.CenterX;
                    miny = loc.CenterZ;
                    minZone = zone;
                }
            }
        }
        gs.map.SpawnX = miny;
        gs.map.SpawnY = minx;
        if (minZone != null)
        {
            minZone.Level = 1;
        }

    }
}

