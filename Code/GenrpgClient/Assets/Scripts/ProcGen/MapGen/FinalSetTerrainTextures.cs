
using System;
using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine; // Needed

public class SetFinalTerrainTextures : BaseZoneGenerator
{
    public override async UniTask Generate(UnityGameState gs, CancellationToken token)
    {
        await base.Generate(gs, token);

        gs.map.OverrideZonePercent = 0;        
        _zoneGenService.SetAllAlphamaps(gs, gs.md.alphas, token);
        await WaitForTerrainLayerLoad(gs,token);
    }

    private async UniTask WaitForTerrainLayerLoad(UnityGameState gs, CancellationToken token)
    {
        for (int x = 0; x < gs.md.awid; x++)
        {
            for (int y = 0; y < gs.md.ahgt; y++)
            {
                float total = 0;
                for (int i =0; i < MapConstants.MaxTerrainIndex; i++)
                {
                    total += gs.md.alphas[x, y, i];
                }
                if (total < 0.1f)
                {
                    gs.md.ClearAlphasAt(gs, x, y);
                    gs.md.alphas[x, y, MapConstants.BaseTerrainIndex] = 1.0f;
                }
                else
                {
                    for (int i = 0; i < MapConstants.MaxTerrainIndex; i++)
                    {
                        gs.md.alphas[x, y, i] /= total;
                    }
                }
            }
        }

        while (true)
        {
            if (_assetService.IsDownloading(gs))
            {
                await UniTask.NextFrame( cancellationToken: token);
            }
            else
            {
                break;
            }
        }

        await UniTask.Delay(TimeSpan.FromSeconds(1.0f), cancellationToken: token);

        while (true)
        {
            bool missingLayer = false;

            for (int x = 0; x < gs.map.BlockCount; x++)
            {
                if (missingLayer)
                {
                    break;
                }
                for (int y = 0; y < gs.map.BlockCount; y++)
                {
                    if (missingLayer)
                    {
                        break;
                    }

                    if (gs.md.terrainPatches[x,y] == null)
                    {
                        missingLayer = true;
                        break;
                    }

                    TerrainData tdata = gs.md.GetTerrainData(gs, x, y);
                    if (tdata == null)
                    {
                        missingLayer = true;
                        break;
                    }
                    TerrainLayer[] layers = tdata.terrainLayers;
                    if (layers == null || layers.Length < 1)
                    {
                        missingLayer = true;
                        break;
                    }

                    for (int s = 0; s < layers.Length; s++)
                    {
                        if (layers[s] == null || layers[s].diffuseTexture == null)
                        {
                            missingLayer = true;
                            break;
                        }
                    }

                    if (!missingLayer && !gs.md.terrainPatches[x, y].HaveSetAlphamaps)
                    {
                        missingLayer = true;
                        break;
                    }
                }
            }

            if (missingLayer)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(1.0f), cancellationToken: token);
            }
            else
            {
                break;
            }
        }

        while (true)
        {
            if (_assetService.IsDownloading(gs))
            {
                await UniTask.Delay(TimeSpan.FromSeconds(2.0f), cancellationToken: token);
            }
            else
            {
                break;
            }
        }


        await UniTask.Delay(TimeSpan.FromSeconds(10.0f), cancellationToken: token);

        gs.md.HaveSetAlphaSplats = true;
	}
}
	
