
using System;

using System.Threading;
using UnityEngine; // Needed
using Assets.Scripts.MapTerrain;
using UnityEditor;

public class SetFinalTerrainTextures : BaseZoneGenerator
{
    public override async Awaitable Generate(CancellationToken token)
    {
        await base.Generate(token);

        _mapProvider.GetMap().OverrideZonePercent = 0;        
        _zoneGenService.SetAllAlphamaps(_md.alphas, token);
        await WaitForTerrainLayerLoad(token);
    }

    private async Awaitable WaitForTerrainLayerLoad(CancellationToken token)
    {
        for (int x = 0; x < _md.awid; x++)
        {
            for (int y = 0; y < _md.ahgt; y++)
            {
                float total = 0;
                for (int i =0; i < MapConstants.MaxTerrainIndex; i++)
                {
                    total += _md.alphas[x, y, i];
                }
                if (total < 0.1f)
                {
                    _md.ClearAlphasAt(x, y);
                    _md.alphas[x, y, MapConstants.BaseTerrainIndex] = 1.0f;
                }
                else
                {
                    for (int i = 0; i < MapConstants.MaxTerrainIndex; i++)
                    {
                        _md.alphas[x, y, i] /= total;
                    }
                }
            }
        }

        while (true)
        {
            if (_assetService.IsDownloading())
            {
                await Awaitable.NextFrameAsync(cancellationToken: token);
            }
            else
            {
                break;
            }
        }

        await Awaitable.WaitForSecondsAsync(1.0f, cancellationToken: token);

        while (true)
        {
            bool missingLayer = false;

            for (int x = 0; x < _mapProvider.GetMap().BlockCount; x++)
            {
                if (missingLayer)
                {
                    break;
                }
                for (int y = 0; y < _mapProvider.GetMap().BlockCount; y++)
                {
                    if (missingLayer)
                    {
                        break;
                    }

                    TerrainPatchData patch = _terrainManager.GetTerrainPatch(x, y);

                    if (patch == null)
                    {
                        missingLayer = true;
                        break;
                    }

                    TerrainData tdata = _terrainManager.GetTerrainData(x, y);
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

                    if (!missingLayer && !patch.HaveSetAlphamaps)
                    {
                        missingLayer = true;
                        break;
                    }
                }
            }

            if (missingLayer)
            {
                await Awaitable.WaitForSecondsAsync(1.0f, cancellationToken: token);
            }
            else
            {
                break;
            }
        }

        while (true)
        {
            if (_assetService.IsDownloading())
            {
                await Awaitable.WaitForSecondsAsync(2.0f, cancellationToken: token);
            }
            else
            {
                break;
            }
        }


        await Awaitable.WaitForSecondsAsync(10.0f, cancellationToken: token);

        _md.HaveSetAlphaSplats = true;
	}
}
	
