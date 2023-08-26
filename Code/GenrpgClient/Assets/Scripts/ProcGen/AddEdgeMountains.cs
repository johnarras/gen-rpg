using Cysharp.Threading.Tasks;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Utils.Data;
using Services.ProcGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class AddEdgeMountains : BaseAddMountains
{
    public override async UniTask Generate(UnityGameState gs, CancellationToken token)
    {
        await UniTask.CompletedTask;
        MyRandom rand = new MyRandom(gs.map.Seed / 4 + 31433);
        short[,] zoneIds = gs.md.mapZoneIds;
        List<MyPointF> points = new List<MyPointF>();
        int mountainWidth = (int)gs.md.GetMountainDefaultSize(gs, gs.map);
        int edgeWidth = mountainWidth + MapConstants.TerrainPatchSize / 4;

        int mapSize = gs.map.GetHwid();
        float heightFreq = mapSize * 1.0f / (MapConstants.TerrainPatchSize / 2);
        float heightAmp = 0.6f;
        float heightPers = 0.4f;
        int heightOctaves = 2;

        float[,] heights = _noiseService.Generate(gs, heightPers, heightFreq, heightAmp, heightOctaves, rand.Next(), mapSize, mapSize);


        float widthFreq = mapSize * 1.0f / (MapConstants.TerrainPatchSize / 2);
        float widthAmp = 0.7f;
        float widthPers = 0.4f;
        int widthOctaves = 2;

        float[,] widths = _noiseService.Generate(gs, widthPers, widthFreq, widthAmp, widthOctaves, rand.Next(), mapSize, mapSize);

        int startMountainWidth = (int)gs.md.GetMountainDefaultSize(gs, gs.map);

        for (int x = edgeWidth; x < gs.map.GetHwid() - 1 - edgeWidth; x++)
        {
            for (int z = edgeWidth; z < gs.map.GetHhgt() - 1 - edgeWidth; z++)
            {
                if (zoneIds[x, z] != zoneIds[x + 1, z] ||
                    zoneIds[x, z] != zoneIds[x, z + 1] ||
                    zoneIds[x, z] != zoneIds[x + 1, z + 1])
                {
                    float heightVal = Mathf.Max(heights[x, z] + 0.5f, 0);
                    if (heightVal > 0)
                    {
                        gs.md.flags[x, z] |= MapGenFlags.IsWallCenter;
                        gs.md.mountainHeights[x, z] = heightVal;
                        points.Add(new MyPointF(x, z));
                    }
                }
            }
        }

        for (int l = 0; l < points.Count; l++)
        {
            MyPointF item = points[l];
            int cx = (int)(item.X);
            int cz = (int)(item.Y);
            if (cx < 0 || cz < 0 || cx >= gs.map.GetHwid() || cz >= gs.map.GetHhgt())
            {
                continue;
            }
            float mainHeight = heights[cx, cz];

            int currWallWidth = (int)(startMountainWidth * (1 + widths[cx,cz]));

            if (gs.md.mapZoneIds[cx, cz] < 1)
            {
                gs.md.mapZoneIds[cx, cz] = MapConstants.MountainZoneId;
            }

            
            gs.md.mountainDistPercent[cx, cz] = 0f;
            gs.md.edgeMountainDistPercent[cx, cz] = 0f;

            float topWidth = 2;
            int mincmidx = Math.Min(gs.map.GetHwid() / 2, cx);
            int maxcmidx = Math.Max(gs.map.GetHwid() / 2, cx);
            int mincmidy = Math.Min(gs.map.GetHhgt() / 2, cz);
            int maxcmidy = Math.Max(gs.map.GetHhgt() / 2, cz);
            for (int y = Math.Max(0, cz - currWallWidth); y <= Math.Min(gs.map.GetHhgt() - 1, cz + currWallWidth); y++)
            {
                int ddy = Math.Abs(y - cz);
                for (int x = Math.Max(0, cx - currWallWidth); x <= Math.Min(gs.map.GetHwid() - 1, cx + currWallWidth); x++)
                {
                    int ddx = Math.Abs(x - cx);

                    double currDist = Math.Sqrt(ddx * ddx + ddy * ddy);
                    double distPct = currDist / currWallWidth;

                    if (currDist < topWidth)
                    {
                        distPct = 0;
                    }
                    else
                    {
                        distPct = (currDist - topWidth) / (currWallWidth - topWidth);
                    }

                    if (distPct >= 1)
                    {
                        continue;
                    }

                    if (gs.md.mapZoneIds[x, y] == 0)
                    {
                        gs.md.mapZoneIds[x, y] = MapConstants.MountainZoneId;
                    }
                    if (gs.md.mountainDistPercent[x, y] > distPct)
                    {
                        gs.md.mountainDistPercent[x, y] = (float)distPct;
                    }
                    if (gs.md.mountainCenterDist[x, y] > currDist)
                    {
                        gs.md.mountainCenterDist[x, y] = (float)(currDist);
                        gs.md.nearestMountainTopHeight[x, y] = mainHeight;
                    }
                    if (gs.md.edgeMountainDistPercent[x, y] > distPct)
                    {
                        gs.md.edgeMountainDistPercent[x, y] = (float)distPct;
                    }
                    gs.md.flags[x, y] |= MapGenFlags.IsEdgeWall;

                    float currPower = MathUtils.Clamp(0.5f, 1.7f, 1.0f + gs.md.mountainDecayPower[x, y]);
                    float newPct = gs.md.mountainHeights[cx, cz] * (float)(1.0f - Math.Pow(distPct, currPower));

                    if (newPct != 0 && gs.md.mountainHeights[x, y] == 0)
                    {
                        gs.md.flags[x, y] |= MapGenFlags.IsSecondaryWall;
                    }

                    if (newPct > gs.md.mountainHeights[x, y])
                    {
                        gs.md.mountainHeights[x, y] = newPct;
                    }
                }
            }
        }
    }
}