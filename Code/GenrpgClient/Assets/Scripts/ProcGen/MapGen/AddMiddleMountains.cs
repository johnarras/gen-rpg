
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Zones.Entities;
using Genrpg.Shared.Zones.WorldData;

public class AddMiddleMountains : BaseAddMountains
{
    public override async UniTask Generate(CancellationToken token)
    {
        await UniTask.CompletedTask;


        AddMiddleMapMountains(_gs);

    }



    public void AddMiddleMapMountains(IUnityGameState gs)
    {
        foreach (Zone zone in _mapProvider.GetMap().Zones)
        {
            AddMiddleZoneMountains(zone);

        }

       // AddDungeonMountains(gs);
    }

    protected void AddMiddleZoneMountains(Zone zone)
    {
        if (_gs == null || zone == null || _md == null || _md.mountainHeights == null)
        {
            return;
        }

        int xsize = zone.XMax - zone.XMin;
        int ysize = zone.ZMax - zone.ZMin;
        int maxSize = Math.Max(xsize, ysize);
        int minSize = MapConstants.TerrainPatchSize * 5;



        if (xsize < minSize || ysize < minSize)
        {
            return;
        }

        MyRandom middleRand = new MyRandom(zone.Seed + 233499);


        int numWalls = MathUtils.IntRange(0, 1, middleRand);

        if (middleRand.NextDouble() < 0.2f)
        {
            numWalls++;
        }

        int wallsAdded = 0;
        for (int times = 0; times < 200; times++)
        {

            if (wallsAdded >= numWalls)
            {
                break;
            }

            int currMaxLen = MathUtils.IntRange(30, maxSize, middleRand);


            int sx = MathUtils.IntRange(zone.XMin, zone.XMax, middleRand);

            int sy = MathUtils.IntRange(zone.ZMin, zone.ZMax, middleRand);


            int ex = MathUtils.IntRange(sx - currMaxLen, sx + currMaxLen, middleRand);

            int ey = MathUtils.IntRange(sy - currMaxLen, sx + currMaxLen, middleRand);

            if (ex < 0 || ex >= _mapProvider.GetMap().GetHwid() || ey < 0 || ey >= _mapProvider.GetMap().GetHhgt())
            {
                continue;
            }

            if (_md.mapZoneIds[sx, sy] != zone.IdKey || _md.mapZoneIds[ex, ey] != zone.IdKey)
            {
                continue;
            }

            int dx = Math.Abs(ex - sx);
            int dy = Math.Abs(ey - sy);

            int maxDist = Math.Max(dx, dy);

            if (maxDist >= 10 && maxDist < maxSize)
            {
                float height = GetMountainHeightMult(middleRand);
                AddMountainRidge(sx, sy, ex, ey, zone.Seed / 2 + times, false, height, true);
                wallsAdded++;
            }
        }
    }
}