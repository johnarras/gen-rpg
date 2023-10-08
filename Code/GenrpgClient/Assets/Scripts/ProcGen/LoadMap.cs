
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Genrpg.Shared.Utils.Data;
using System.Threading;
using Genrpg.Shared.MapServer.Constants;
using Assets.Scripts.MapTerrain;
using UnityEngine; // Needed

public class LoadMap : BaseZoneGenerator
{
    public override async Task Generate(UnityGameState gs, CancellationToken token)
    { 
        await base.Generate(gs, token);
    }

    public void OnError(UnityGameState gs, string txt)
    {
        gs.logger.Error("Zone load error: " + txt);

    }

    public void LoadOneTerrainPatch(UnityGameState gs, int gx, int gy, CancellationToken token)
    {
        _token = token;
        if (_terrainManager == null)
        {
            gs.loc.Resolve(this);
        }
        TaskUtils.AddTask(InnerLoadOneTerrainPatch(gs, gx, gy, token));
    }

    private async Task InnerLoadOneTerrainPatch(UnityGameState gs, int gx, int gy, CancellationToken token)
    {
        if ( gx < 0 || gy < 0 ||
            gs.map == null)
        {
            OnError(gs, "Missing basic data");
            return;
        }
        TerrainPatchData patch = gs.md.GetTerrainPatch(gs, gx, gy);

        if (patch == null)
        {
            return;
        }

        if (patch.DataBytes == null)
        {

            string filePath = patch.GetFilePath(gs, false);

            ClientRepository<TerrainPatchData> repo = new ClientRepository<TerrainPatchData>(gs.logger);

            patch.DataBytes = repo.LoadBytes(filePath);
            await Task.Delay(1, token);

            if (patch.DataBytes == null || patch.DataBytes.Length < 1)
            {

                DownloadData ddata = new DownloadData() 
                { 
                    Handler = OnDownloadTerrainBytes, 
                    Data = patch,
                };
                _assetService.DownloadFile(gs, filePath, ddata, token);
                return;
            }
            else
            {
                gs.md.SetTerrainPatchAtGridLocation(gs, gx, gy, gs.map, patch);
            }
        }

        MapTerrainManager.PatchesAdded++;
        await Task.Delay(1, token);

        if (patch.terrain == null)
        {
            await _terrainManager.SetupOneTerrainPatch(gs, gx, gy, token);
        }

        Terrain terr = patch.terrain as Terrain;

        if (terr == null)
        {
            OnError(gs, "No patch terrain setup at " + patch.X + " " + patch.Y);
            return;
        }

        gs.md.terrainPatches[patch.X, patch.Y] = patch;

        int startX = patch.Y * (MapConstants.TerrainPatchSize - 1);
        int startY = patch.X * (MapConstants.TerrainPatchSize - 1);


        SetTerrainTextures setTextures = new SetTerrainTextures();

        if (patch.mainZoneIds == null)
        {
            patch.mainZoneIds = new int[MapConstants.TerrainPatchSize, MapConstants.TerrainPatchSize];
        }

        if (patch.overrideZoneIds == null)
        {
            patch.overrideZoneIds = new int[MapConstants.TerrainPatchSize, MapConstants.TerrainPatchSize];
        }

        if (UnityAssetService.LoadSpeed != LoadSpeed.Fast)
        {
            await Task.Delay(1, token);
        }

        if (patch.baseAlphas == null)
        {
            patch.baseAlphas = new float[MapConstants.TerrainPatchSize, MapConstants.TerrainPatchSize, MapConstants.MaxTerrainIndex];
        }

        if (UnityAssetService.LoadSpeed != LoadSpeed.Fast)
        {
            await Task.Delay(1, token);
        }
        if (patch.heights == null)
        {
            patch.heights = new float[MapConstants.TerrainPatchSize, MapConstants.TerrainPatchSize];
        }

        if (UnityAssetService.LoadSpeed != LoadSpeed.Fast)
        {
            await Task.Delay(1, token);
        }

        if (patch.mapObjects == null)
        {
            patch.mapObjects = new uint[MapConstants.TerrainPatchSize, MapConstants.TerrainPatchSize];
        }

        if (UnityAssetService.LoadSpeed != LoadSpeed.Fast)
        {
            await Task.Delay(1, token);
        }

        if (patch.grassAmounts == null)
        {
            patch.grassAmounts = new ushort[MapConstants.TerrainPatchSize, MapConstants.TerrainPatchSize, MapConstants.MaxGrass];
        }

        if (UnityAssetService.LoadSpeed != LoadSpeed.Fast)
        {
            await Task.Delay(1, token);
        }

        // 11 bytes per cell with ordering:
        // 1 Heights 2 bytes
        // 2 Objects 4 bytes
        // 3 Zones 1 byte (*div)
        // 4 Alphas 3 bytes (*div)
        // 1 BaseZonePct 1 byte

        ushort shortHeight = 0;
        int index = 0;
        int xx = 0;
        int yy = 0;
        try
        {
            for (xx = 0; xx < MapConstants.TerrainPatchSize; xx++)
            {
                for (yy = 0; yy < MapConstants.TerrainPatchSize; yy++)
                {
                    shortHeight = patch.DataBytes[index++];
                    shortHeight += (ushort)(patch.DataBytes[index++] << 8);
                    patch.heights[xx, yy] = 1.0f * shortHeight / MapConstants.HeightSaveMult;
                }
            }
        }
        catch (Exception e)
        {
            string bytelen = (patch.DataBytes == null ? "NullBytes" : "Len: " + patch.DataBytes.Length);
            gs.logger.Exception(e, "LoadMap3: " + xx + " " + yy + " Len: " + bytelen + " Idx: " + index);
        }

        if (UnityAssetService.LoadSpeed != LoadSpeed.Fast)
        {
            await Task.Delay(1, token);
        }
        try
        {
            // 2 Objects 2 bytes
            uint worldObjectValue = 0;
            for (int x = 0; x < MapConstants.TerrainPatchSize - 1; x++)
            {
                for (int y = 0; y < MapConstants.TerrainPatchSize - 1; y++)
                {
                    worldObjectValue = patch.DataBytes[index++];
                    worldObjectValue += (uint)(patch.DataBytes[index++] << 8);
                    worldObjectValue += (uint)(patch.DataBytes[index++] << 16);
                    worldObjectValue += (uint)(patch.DataBytes[index++] << 24);

                    patch.mapObjects[x, y] = worldObjectValue;


                    if (patch.mapObjects[x, y] > MapConstants.GrassMinCellValue &&
                        patch.mapObjects[x, y] < MapConstants.GrassMaxCellValue)
                    {
                        worldObjectValue -= MapConstants.GrassMinCellValue;

                        int div = (MapConstants.MaxGrassValue + 1);

                        for (int i = 0; i < MapConstants.MaxGrass; i++)
                        {
                            patch.grassAmounts[y, x, i] = (ushort)(worldObjectValue % div);
                            worldObjectValue = (ushort)(worldObjectValue / div);
                        }
                        patch.mapObjects[x, y] = 0;
                    }
                }
            }

            for (int t = 0; t < MapConstants.TerrainPatchSize; t++)
            {
                uint lowerPos = patch.mapObjects[(t * 13 + gx * 17 + gy * 29) % MapConstants.TerrainPatchSize,
                    MapConstants.TerrainPatchSize - 1];
                if (lowerPos >= MapConstants.GrassMinCellValue && lowerPos <= MapConstants.GrassMaxCellValue)
                {
                    patch.mapObjects[t, MapConstants.TerrainPatchSize - 1] = lowerPos;
                }

                uint leftPos = patch.mapObjects[MapConstants.TerrainPatchSize - 1,
                    (t * 23 + gx * 53 + gy * 71) % MapConstants.TerrainPatchSize];
                if (lowerPos >= MapConstants.GrassMinCellValue && lowerPos <= MapConstants.GrassMaxCellValue)
                {
                    patch.mapObjects[MapConstants.TerrainPatchSize - 1, t] = lowerPos;
                }
            }
        }
        catch (Exception e)
        {
            gs.logger.Exception(e, "LoadMap2");
        }

        if (UnityAssetService.LoadSpeed != LoadSpeed.Fast)
        {
            await Task.Delay(1, token);
        }
        // 3 ZoneIds 1 byte (*divsq)
        List<int> overrideZoneIds = new List<int>();
        List<int> mainZoneIds = new List<int>();
        for (int x = 0; x < MapConstants.TerrainPatchSize; x++)
        {
            for (int y = 0; y < MapConstants.TerrainPatchSize; y++)
            {
                //gs.md.mapZoneIds[x + startX, y + startY] = bytes[index];
                patch.mainZoneIds[x, y] = patch.DataBytes[index++];
                if (patch.mainZoneIds[x, y] >= SharedMapConstants.MapZoneStartId)
                {
                    if (!overrideZoneIds.Contains(patch.mainZoneIds[x, y]))
                    {
                        overrideZoneIds.Add(patch.mainZoneIds[x, y]);
                    }
                    if (!mainZoneIds.Contains(patch.mainZoneIds[x,y]))
                    {
                        mainZoneIds.Add(patch.mainZoneIds[x, y]);
                    }
                }
            }
        }
        if (UnityAssetService.LoadSpeed != LoadSpeed.Fast)
        {
            await Task.Delay(1, token);
        }
        // 4 Alphas 3 bytes (*divsq)
        float alphaTotal = 0;
        float alphaDiv = MapConstants.AlphaSaveMult * 1.0f;
        for (int x = 0; x < MapConstants.TerrainPatchSize; x++)
        {
            for (int y = 0; y < MapConstants.TerrainPatchSize; y++)
            {
                alphaTotal = 0;
                for (int i = 0; i < MapConstants.MaxTerrainIndex - 1; i++)
                {
                    try
                    {
                        patch.baseAlphas[x, y, i] = patch.DataBytes[index++] / alphaDiv;
                    }
                    catch (Exception e)
                    {
                        gs.logger.Exception(e, "LoadMap");
                        throw e;
                    }
                    alphaTotal += patch.baseAlphas[x, y, i];
                }
                if (alphaTotal < 1)
                {
                    patch.baseAlphas[x, y, MapConstants.MaxTerrainIndex - 1] = 1 - alphaTotal;
                }
                else if (alphaTotal > 1)
                {
                    for (int i = 0; i < MapConstants.MaxTerrainIndex; i++)
                    {
                        patch.baseAlphas[x, y, i] /= alphaTotal;
                    }
                }
            }
        }
        for (int x = 0; x < MapConstants.TerrainPatchSize; x++)
        {
            for (int y = 0; y < MapConstants.TerrainPatchSize; y++)
            {
                patch.overrideZoneIds[x,y] = patch.DataBytes[index++];
                if (patch.overrideZoneIds[x,y] > 0 && !overrideZoneIds.Contains(patch.overrideZoneIds[x,y]))
                {
                    overrideZoneIds.Add(patch.overrideZoneIds[x, y]);
                }
            }
        }
        patch.FullZoneIdList = new List<long>();
        patch.MainZoneIdList = new List<long>();
        foreach (int zid in overrideZoneIds)
        {
            patch.FullZoneIdList.Add(zid);
        }

        foreach (int zid in mainZoneIds)
        {
            patch.MainZoneIdList.Add(zid);
        }

        await setTextures.SetOneTerrainPatchLayers(gs, patch, token);

        if (UnityAssetService.LoadSpeed != LoadSpeed.Fast)
        {
            await Task.Delay(1, token);
        }
        else
        {
            await Task.Delay(TimeSpan.FromSeconds(1.0f), cancellationToken: token); 
        }

        await _zoneGenService.SetOnePatchAlphamaps(gs, patch, token);

        if (UnityAssetService.LoadSpeed != LoadSpeed.Fast)
        {
            await Task.Delay(1, token);
        }
        _zoneGenService.SetOnePatchHeightmaps(gs, patch, null, patch.heights);


        if (UnityAssetService.LoadSpeed != LoadSpeed.Fast)
        {
            await Task.Delay(1, token);
        }

        LoadPlantAssets lpa = new LoadPlantAssets();
        lpa.SetupOneMapGrass(gs, patch.X, patch.Y, token);

        if (UnityAssetService.LoadSpeed != LoadSpeed.Fast)
        {
            await Task.Delay(1, token);
        }

        gs.md.SetOneTerrainNeighbors(gs, patch.X, patch.Y);
       
        gs.md.SetOneTerrainNeighbors(gs, patch.X + 1, patch.Y);
       
        gs.md.SetOneTerrainNeighbors(gs, patch.X - 1, patch.Y);
     
        gs.md.SetOneTerrainNeighbors(gs, patch.X, patch.Y - 1);
       
        gs.md.SetOneTerrainNeighbors(gs, patch.X, patch.Y + 1);

        if (UnityAssetService.LoadSpeed != LoadSpeed.Fast)
        {
            await Task.Delay(1, token);
        }
        await _terrainManager.AddPatchObjects(gs, gx, gy, token);

        List<MyPoint> loadingItemsToRemove = gs.md.loadingPatchList.Where(item=>item.X == gx && item.Y == gy).ToList();

        foreach (MyPoint item in loadingItemsToRemove)
        {
            gs.md.loadingPatchList.Remove(item);
        }

       
    }

    private void OnDownloadTerrainBytes(UnityGameState gs, string url, object obj, object data, CancellationToken token)
    {
        TerrainPatchData patch = data as TerrainPatchData;

        if (patch == null)
        {
            return;
        }
        if (obj == null)
        {
            return;
        }

        if (!TokenUtils.IsValid(_token))
        {
            return;
        }

        byte[] bytes = obj as byte[];

        if (bytes == null || bytes.Length < 10)
        {
            string txt = "No bytes";
            if (bytes != null)
            {
                txt = System.Text.Encoding.UTF8.GetString(bytes);
            }
            gs.logger.Error("Failed to download Bytes from: " + url + " " + bytes);
            return;
        }

        string filePath = patch.GetFilePath(gs, false);

        ClientRepository<TerrainPatchData> repo = new ClientRepository<TerrainPatchData>(gs.logger);
       
        repo.SaveBytes(filePath, bytes);
        patch.DataBytes = bytes;
        LoadOneTerrainPatch(gs, patch.X, patch.Y, _token);
    }
}
	
