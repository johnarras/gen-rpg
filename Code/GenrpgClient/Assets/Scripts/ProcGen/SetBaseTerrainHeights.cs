using Cysharp.Threading.Tasks;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Threading;

public class SetBaseTerrainHeights : BaseZoneGenerator
{
    public override async UniTask Generate(UnityGameState gs, CancellationToken token)
    {
        await base.Generate(gs, token);
        int wid = gs.map.GetHwid();
        int hgt = gs.map.GetHhgt();


        MyRandom rand = new MyRandom(gs.map.Seed % 1000000000 + 192873);

        float delta = MathUtils.FloatRange(0.07f, 0.12f, rand);

        float heightPerGrid = MathUtils.FloatRange(MapConstants.MapHeightPerGrid * (1 - delta), MapConstants.MapHeightPerGrid * (1 + delta), rand);

        float minHeight = MapConstants.StartHeightPercent - heightPerGrid * 0.3f;
        float maxHeight = MapConstants.StartHeightPercent + heightPerGrid * (gs.map.BlockCount / 2 - 1);


        // Good settings for overall wide slopes/big mountains/valleys
        long pseed = gs.map.Seed + 19383;

        List<float[,]> heightsList = new List<float[,]>();

        int heightTimes = 3;


        float overworldSizeMult = 1.0f * gs.map.GetHwid() / MapConstants.MapHeight;

        for (int i = 0; i < heightTimes; i++)
        {
            float pers = MathUtils.FloatRange(0.1f, 0.3f, rand);



            // This number is here because these ups and downs should be a percent of the overall world height.
            float amp = MathUtils.FloatRange(0.005f, 0.01f, rand) * overworldSizeMult;
            // We want these features to be approx several hundred units across or so.
            float freq = gs.map.GetHwid() / (MathUtils.FloatRange(6.0f, 9.0f, rand) * MapConstants.TerrainPatchSize);
            // This number is in this range because we want a few bumps to encompass the whole world.
            int octaves = 2;
            float[,] heights = _noiseService.Generate(gs, pers, freq, amp, octaves, pseed, wid, hgt);
            heightsList.Add(heights);
        }

        float perturbDampStartPercent = 0.85f;

        float power = 0.7f;
        for (int x = 0; x < wid; x++)
        {
            float xpct = Math.Abs((x - wid / 2.0f) / (wid / 2.0f));
            for (int y = 0; y < hgt; y++)
            {
                float ypct = Math.Abs((y - hgt / 2.0f) / (hgt / 2.0f));


                float distPct = (float)Math.Pow(Math.Pow(xpct, power) + Math.Pow(ypct, power), 1 / power);

                if (distPct > 1.0f)
                {
                    distPct = 1.0f;
                }

                float dirPct = Math.Max(xpct, ypct);

                float pct = Math.Min(distPct, dirPct);

                float heightPct = maxHeight * (1 - pct) + minHeight * pct;

                gs.md.heights[x, y] = heightPct;

                float perturbScale = 1.0f;
                if (pct > perturbDampStartPercent)
                {
                    perturbScale = (1 - pct) / (1 - perturbDampStartPercent);
                }


                float currHeightNoise = 0;

                for (int i = 0; i < heightTimes; i++)
                {
                    currHeightNoise += heightsList[i][x, y];
                }


                float heightAdjust = currHeightNoise * perturbScale;

                if (heightAdjust < 0)
                {
                    heightAdjust /= 2;
                }

                gs.md.heights[x, y] += heightAdjust;
                if (gs.md.heights[x, y] < 0)
                {
                    gs.md.heights[x, y] = 0;
                }
            }
        }


        for (int x = 0; x < wid; x++)
        {
            for (int y = 0; y < hgt; y++)
            {

                float edgePercent = (float)Math.Pow(gs.md.EdgeHeightmapAdjustPercent(gs, gs.map, x, y), 0.09f);

                if (x < 2 || y < 2 || x >= wid - 3 || y >= hgt - 3)
                {
                    edgePercent = 0;
                }

                gs.md.heights[x, y] *= edgePercent;
                /*
                if (gs.md.heights[x, y] < MapConstants.StartHeightPercent)
                {
                    float ratio = gs.md.heights[x, y] / MapConstants.StartHeightPercent;
                    gs.md.heights[x, y] *= (float)(Math.Pow(ratio, 11.0f));

                }

            */
                /*
                int xedgeDist = Math.Min(x, gs.map.GetHwid() - x);
                int yedgeDist = Math.Min(y, gs.map.GetHhgt() - y);
                int edgeDist = Math.Min(xedgeDist, yedgeDist);
                if (edgeDist < minEdgeDist)
                {
                    gs.md.heights[x, y] *= (1.0f * edgeDist) / minEdgeDist;
                }
                */
            }
        }

    }
}