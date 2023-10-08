
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using GEntity = UnityEngine.GameObject;


using Genrpg.Shared.Core.Entities;



using System.Threading.Tasks;

using Genrpg.Shared.MapServer.Entities;
using System.Threading;

public class SmoothTerrainTexturesFinal : BaseZoneGenerator
{
    public override async Task Generate(UnityGameState gs, CancellationToken token)
    {
        await base.Generate(gs, token);
        int awid = gs.md.awid;
        int ahgt = gs.md.ahgt;

        int radius = 1;
        float smoothScale = 0.04f;

        float[,,] alphas2 = new float[awid, ahgt, MapConstants.MaxTerrainIndex];

        for (int x = 0; x < awid; x++)
        {
            for (int y = 0; y < ahgt; y++)
            {
                for (int i = 0; i < MapConstants.MaxTerrainIndex; i++)
                {
                    alphas2[x, y, i] = gs.md.alphas[x, y, i];
                }
            }
        }

        for (int x = 0; x < awid; x++)
        {
            for (int y = 0; y < ahgt; y++)
            {
                for (int i = 0; i < MapConstants.MaxTerrainIndex; i++)
                {
                    float totalWeight = 0;
                    float totalVal = 0;
                    for (int xx = x-radius; xx <= x+radius; xx++)
                    {
                        if (xx < 0 || xx >= gs.md.awid)
                        {
                            continue;
                        }

                        int dx = Math.Abs(xx - x);
                        for (int yy = y-radius; yy <= y+radius; yy++)
                        {
                            if (yy < 0 || yy >= gs.md.ahgt)
                            {
                                continue;
                            }

                            int dy = Math.Abs(yy - y);
                            int dist = dx + dy;
                            float currWeight = (float)Math.Pow(smoothScale, dist);
                            totalWeight += currWeight;
                            totalVal += gs.md.alphas[xx, yy, i] * currWeight;
                        }
                    }
                    alphas2[x, y, i] = totalVal / totalWeight;
                }
            }
        }

        for (int x= 0; x < gs.md.awid; x++)
        {
            for (int y = 0; y < gs.md.ahgt; y++)
            {
                float total = 0;
                for (int i = 0; i < MapConstants.MaxTerrainIndex; i++)
                {
                    total += alphas2[x, y, i];
                }
                for (int i = 0; i < MapConstants.MaxTerrainIndex; i++)
                {
                    alphas2[x, y, i] /= total;
                }
            }
        }

        gs.md.alphas = alphas2;
    }
}