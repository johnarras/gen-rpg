
using System;
using System.Linq;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Genrpg.Shared.Utils.Data;
using System.Threading;
using Genrpg.Shared.MapServer.Constants;
using Assets.Scripts.MapTerrain;
using UnityEngine; // Needed
using Genrpg.Shared.Pathfinding.Services;
using System.IO.Compression;
using Genrpg.Shared.Pathfinding.Constants;

public class LoadMap : BaseZoneGenerator
{
    public override async UniTask Generate(UnityGameState gs, CancellationToken token)
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
        InnerLoadOneTerrainPatch(gs, gx, gy, token).Forget();
    }

    private async UniTask InnerLoadOneTerrainPatch(UnityGameState gs, int gx, int gy, CancellationToken token)
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

            ClientRepositoryCollection<TerrainPatchData> repo = new ClientRepositoryCollection<TerrainPatchData>(gs.logger);

            patch.DataBytes = repo.LoadBytes(filePath);
            await UniTask.NextFrame( cancellationToken: token);

            if (patch.DataBytes == null || patch.DataBytes.Length < 1)
            {

                DownloadData ddata = new DownloadData() 
                { 
                    Handler = OnDownloadTerrainBytes, 
                    Data = patch,
                };
                _assetService.DownloadFile(gs, filePath, ddata, true, token);
                return;
            }
            else
            {
                gs.md.SetTerrainPatchAtGridLocation(gs, gx, gy, gs.map, patch);
            }
        }

        MapTerrainManager.PatchesAdded++;
        await UniTask.NextFrame( cancellationToken: token);

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

        // 1. Heights 2
        // 2. Objects 4
        // 3. Alphas 3
        // 4. Zone 1 
        // 5. SubZone 1
        // 6. OverrideZoneScale 1

        // 2 + 4 + 3 + 1 + 1 + 1 = 12;

        ushort shortHeight = 0;
        int index = 0;
        int xx = 0;
        int yy = 0;

        if (patch.heights == null)
        {
            patch.heights = new float[MapConstants.TerrainPatchSize,MapConstants.TerrainPatchSize];
        }

        // 1 Heights 2
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
            await UniTask.NextFrame( cancellationToken: token);
        }
        try
        {
            if (patch.grassAmounts == null)
            {
                patch.grassAmounts = new ushort[MapConstants.TerrainPatchSize, MapConstants.TerrainPatchSize, MapConstants.MaxGrass];
            }

            if (patch.mapObjects == null)
            {
                patch.mapObjects = new uint[MapConstants.TerrainPatchSize, MapConstants.TerrainPatchSize];
            }

            // 2 Objects 4 bytes
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
            await UniTask.NextFrame( cancellationToken: token);
        }

        if (patch.baseAlphas == null)
        {
           patch.baseAlphas = new float[MapConstants.TerrainPatchSize, MapConstants.TerrainPatchSize, MapConstants.MaxTerrainIndex];
        }

        // 3 Alphas 3 bytes 
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

        // 4 ZoneId 1 byte (*divsq)
        List<int> subZoneIds = new List<int>();
        List<int> mainZoneIds = new List<int>();
        if (gs.map.OverrideZoneId > 0)
        {
            mainZoneIds.Add((int)gs.map.OverrideZoneId);
            subZoneIds.Add((int)gs.map.OverrideZoneId);
        }

        List<IdVal> mainZoneQuantities = new List<IdVal>();

        for (int x = 0; x < MapConstants.TerrainPatchSize; x++)
        {
            for (int y = 0; y < MapConstants.TerrainPatchSize; y++)
            {
                //gs.md.mapZoneIds[x + startX, y + startY] = bytes[index];
                byte newMainZoneId = patch.DataBytes[index++];
                patch.mainZoneIds[x, y] = newMainZoneId;
                if (newMainZoneId >= SharedMapConstants.MapZoneStartId)
                {
                    if (!subZoneIds.Contains(newMainZoneId))
                    {
                        subZoneIds.Add(newMainZoneId);
                    }
                    if (!mainZoneIds.Contains(newMainZoneId))
                    {
                        mainZoneIds.Add(newMainZoneId);
                    }

                    IdVal zoneQuantity = mainZoneQuantities.FirstOrDefault(x => x.Id == newMainZoneId);

                    if (zoneQuantity == null)
                    {
                        zoneQuantity = new IdVal() { Id = newMainZoneId };
                        mainZoneQuantities.Add(zoneQuantity);
                    }

                    zoneQuantity.Val++;
                }
            }
        }

        mainZoneIds = mainZoneQuantities.OrderByDescending(x => x.Val).Select(x => x.Id).ToList();

        if (UnityAssetService.LoadSpeed != LoadSpeed.Fast)
        {
            await UniTask.NextFrame( cancellationToken: token);
        }

        // 5 subzoneId 1 byte
        for (int x = 0; x < MapConstants.TerrainPatchSize; x++)
        {
            for (int y = 0; y < MapConstants.TerrainPatchSize; y++)
            {
                patch.subZoneIds[x,y] = patch.DataBytes[index++];
                if (patch.subZoneIds[x,y] > 0 && !subZoneIds.Contains(patch.subZoneIds[x,y]))
                {
                    subZoneIds.Add(patch.subZoneIds[x, y]);
                }
            }
        }


        // 6 OverrideZonePercent 1 byte
        for (int x = 0; x < MapConstants.TerrainPatchSize; x++)
        {
            for (int y = 0; y < MapConstants.TerrainPatchSize; y++)
            {
                patch.overrideZoneScales[x, y] = patch.DataBytes[index++];
                if (patch.overrideZoneScales[x,y] <= gs.map.OverrideZonePercent)
                {
                    patch.subZoneIds[x, y] = (byte)gs.map.OverrideZoneId;
                }
            }
        }


        patch.FullZoneIdList = new List<long>();
        patch.MainZoneIdList = new List<long>();
        foreach (int zid in subZoneIds)
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
            await UniTask.NextFrame( cancellationToken: token);
        }
        else
        {
            await UniTask.Delay(TimeSpan.FromSeconds(1.0f), cancellationToken: token); 
        }

        await _zoneGenService.SetOnePatchAlphamaps(gs, patch, token);

        if (UnityAssetService.LoadSpeed != LoadSpeed.Fast)
        {
            await UniTask.NextFrame( cancellationToken: token);
        }
        _zoneGenService.SetOnePatchHeightmaps(gs, patch, null, patch.heights);


        if (UnityAssetService.LoadSpeed != LoadSpeed.Fast)
        {
            await UniTask.NextFrame( cancellationToken: token);
        }

        LoadPlantAssets lpa = new LoadPlantAssets();
        lpa.SetupOneMapGrass(gs, patch.X, patch.Y, token);

        if (UnityAssetService.LoadSpeed != LoadSpeed.Fast)
        {
            await UniTask.NextFrame( cancellationToken: token);
        }

        gs.md.SetOneTerrainNeighbors(gs, patch.X, patch.Y);
       
        gs.md.SetOneTerrainNeighbors(gs, patch.X + 1, patch.Y);
       
        gs.md.SetOneTerrainNeighbors(gs, patch.X - 1, patch.Y);
     
        gs.md.SetOneTerrainNeighbors(gs, patch.X, patch.Y - 1);
       
        gs.md.SetOneTerrainNeighbors(gs, patch.X, patch.Y + 1);

        if (UnityAssetService.LoadSpeed != LoadSpeed.Fast)
        {
            await UniTask.NextFrame( cancellationToken: token);
        }
        await _terrainManager.AddPatchObjects(gs, gx, gy, token);

        List<MyPoint> loadingItemsToRemove = gs.md.loadingPatchList.Where(item=>item.X == gx && item.Y == gy).ToList();

        foreach (MyPoint item in loadingItemsToRemove)
        {
            gs.md.loadingPatchList.Remove(item);
        }

        if (true && gs.pathfinding != null)
        {
            for (int px = 0; px < MapConstants.TerrainPatchSize; px += PathfindingConstants.BlockSize)
            {
                int worldx = gx * (MapConstants.TerrainPatchSize - 1) + px;
                for (int pz = 0; pz < MapConstants.TerrainPatchSize; pz += PathfindingConstants.BlockSize)
                {
                    int worldz = gy * (MapConstants.TerrainPatchSize - 1) + pz;

                    int finalx = worldx / PathfindingConstants.BlockSize;
                    int finalz = worldz / PathfindingConstants.BlockSize;

                    if (finalx < 0 || finalx >= gs.pathfinding.GetLength(0) ||
                        finalz < 0 || finalz >= gs.pathfinding.GetLength(1))
                    {
                        continue;
                    }
                    if (gs.pathfinding[finalx,finalz])
                    {
                        float height = gs.md.SampleHeight(gs, worldx, 2000, worldz);
                        GameObject sphere = Resources.Load<GameObject>("Prefabs/TestSphere");
                        sphere = GameObject.Instantiate(sphere);
                        sphere.name = "TestSphere_" + worldx + "_" + worldz;
                        GEntityUtils.AddToParent(sphere, patch.terrain.gameObject);
                        sphere.transform.position = new Vector3(worldx, height, worldz);
                    }
                }
            }
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

        ClientRepositoryCollection<TerrainPatchData> repo = new ClientRepositoryCollection<TerrainPatchData>(gs.logger);
       
        repo.SaveBytes(filePath, bytes);
        patch.DataBytes = bytes;
        LoadOneTerrainPatch(gs, patch.X, patch.Y, _token);
    }
}
	
