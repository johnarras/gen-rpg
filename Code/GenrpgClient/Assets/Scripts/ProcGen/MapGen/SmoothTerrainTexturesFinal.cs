
using System;
using System.Threading;
using UnityEngine;

public class SmoothTerrainTexturesFinal : BaseZoneGenerator
{
    public override async Awaitable Generate(CancellationToken token)
    {
        await base.Generate(token);
        int awid = _md.awid;
        int ahgt = _md.ahgt;

        int radius = 1;
        float smoothScale = 0.04f;

        float[,,] alphas2 = new float[awid, ahgt, MapConstants.MaxTerrainIndex];

        for (int x = 0; x < awid; x++)
        {
            for (int y = 0; y < ahgt; y++)
            {
                for (int i = 0; i < MapConstants.MaxTerrainIndex; i++)
                {
                    alphas2[x, y, i] = _md.alphas[x, y, i];
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
                        if (xx < 0 || xx >= _md.awid)
                        {
                            continue;
                        }

                        int dx = Math.Abs(xx - x);
                        for (int yy = y-radius; yy <= y+radius; yy++)
                        {
                            if (yy < 0 || yy >= _md.ahgt)
                            {
                                continue;
                            }

                            int dy = Math.Abs(yy - y);
                            int dist = dx + dy;
                            float currWeight = (float)Math.Pow(smoothScale, dist);
                            totalWeight += currWeight;
                            totalVal += _md.alphas[xx, yy, i] * currWeight;
                        }
                    }
                    alphas2[x, y, i] = totalVal / totalWeight;
                }
            }
        }

        for (int x= 0; x < _md.awid; x++)
        {
            for (int y = 0; y < _md.ahgt; y++)
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

        _md.alphas = alphas2;
    }
}