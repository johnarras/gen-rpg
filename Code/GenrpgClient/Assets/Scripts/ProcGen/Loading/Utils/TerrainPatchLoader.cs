
using System;
using System.Linq;
using System.Collections.Generic;

using Genrpg.Shared.Utils.Data;
using System.Threading;
using Genrpg.Shared.MapServer.Constants;
using Assets.Scripts.MapTerrain;
using UnityEngine; // Needed
using Genrpg.Shared.Pathfinding.Constants;
using Genrpg.Shared.Interfaces;

public interface ITerrainPatchLoader : IInitializable
{
    void LoadOneTerrainPatch(int gx, int gy, bool fastLoading, CancellationToken token);
}

public class TerrainPatchLoader : BaseZoneGenerator, ITerrainPatchLoader
{

    private IPlantAssetLoader _plantAssetLoader;
    private ITerrainTextureManager _terrainTextureManager;
    private IPlayerManager _playerManager;

    public override async Awaitable Generate(CancellationToken token)
    { 
        await base.Generate(token);
    }

    public void OnError(string txt)
    {
        _logService.Error("Zone load error: " + txt);

    }

    public void LoadOneTerrainPatch(int gx, int gy, bool fastLoading, CancellationToken token)
    {
        _token = token;
        AwaitableUtils.ForgetAwaitable(InnerLoadOneTerrainPatch(gx, gy, fastLoading, token));
    }

    private async Awaitable InnerLoadOneTerrainPatch(int gx, int gy, bool fastLoading, CancellationToken token)
    {
        if (gx < 0 || gy < 0 ||
            _mapProvider.GetMap() == null)
        {
            OnError("Missing basic data");
            return;
        }
        TerrainPatchData patch = _terrainManager.GetTerrainPatch(gx, gy);

        if (patch == null)
        {
            return;
        }

        if (patch.DataBytes == null)
        {

            string filePath = patch.GetFilePath(false);

            ClientRepositoryCollection<TerrainPatchData> repo = new ClientRepositoryCollection<TerrainPatchData>(_logService);

            patch.DataBytes = repo.LoadBytes(filePath);
            await Awaitable.NextFrameAsync(cancellationToken: token);

            if (patch.DataBytes == null || patch.DataBytes.Length < 1)
            {

                DownloadFileData ddata = new DownloadFileData() 
                { 
                    Handler = OnDownloadTerrainBytes, 
                    Data = patch,
                };
                _fileDownloadService.DownloadFile(filePath, ddata, true, token);
                return;
            }
            else
            {
                _terrainManager.SetTerrainPatchAtGridLocation(gx, gy, _mapProvider.GetMap(), patch);
            }
        }

        _terrainManager.IncrementPatchesAdded();
        await Awaitable.NextFrameAsync(cancellationToken: token);

        if (patch.terrain == null)
        {
            await _terrainManager.SetupOneTerrainPatch(gx, gy, token);
        }

        Terrain terr = patch.terrain as Terrain;

        if (terr == null)
        {
            OnError("No patch terrain setup at " + patch.X + " " + patch.Y);
            return;
        }

        _terrainManager.SetTerrainPatchAtGridLocation(patch.X, patch.Y, _mapProvider.GetMap(), patch);

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
            _logService.Exception(e, "LoadMap3: " + xx + " " + yy + " Len: " + bytelen + " Idx: " + index);
        }

        if (!fastLoading)
        {
            await Awaitable.NextFrameAsync(cancellationToken: token);
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
            _logService.Exception(e, "LoadMap2");
        }

        if (!fastLoading)
        {
            await Awaitable.NextFrameAsync(cancellationToken: token);
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
                        _logService.Exception(e, "LoadMap");
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
        if (_mapProvider.GetMap().OverrideZoneId > 0)
        {
            mainZoneIds.Add((int)_mapProvider.GetMap().OverrideZoneId);
            subZoneIds.Add((int)_mapProvider.GetMap().OverrideZoneId);
        }

        List<IdVal> mainZoneQuantities = new List<IdVal>();

        for (int x = 0; x < MapConstants.TerrainPatchSize; x++)
        {
            for (int y = 0; y < MapConstants.TerrainPatchSize; y++)
            {
                //_md.mapZoneIds[x + startX, y + startY] = bytes[index];
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

        if (!fastLoading)
        {
            await Awaitable.NextFrameAsync(cancellationToken: token);
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
                if (patch.overrideZoneScales[x,y] <= _mapProvider.GetMap().OverrideZonePercent)
                {
                    patch.subZoneIds[x, y] = (byte)_mapProvider.GetMap().OverrideZoneId;
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

        await _terrainTextureManager.SetOneTerrainPatchLayers(patch, token);

        if (!fastLoading)
        {
            await Awaitable.NextFrameAsync(cancellationToken: token);
        }
        else
        {
            await Awaitable.WaitForSecondsAsync(1.0f, cancellationToken: token); 
        }

        await _zoneGenService.SetOnePatchAlphamaps(patch, token);

        if (!fastLoading)
        {
            await Awaitable.NextFrameAsync(cancellationToken: token);
        }
        _zoneGenService.SetOnePatchHeightmaps(patch, null, patch.heights);


        if (!fastLoading)
        {
            await Awaitable.NextFrameAsync(cancellationToken: token);
        }

        _plantAssetLoader.SetupOneMapGrass(patch.X, patch.Y, token);

        if (!fastLoading)
        {
            await Awaitable.NextFrameAsync(cancellationToken: token);
        }

        _terrainManager.SetOneTerrainNeighbors(patch.X, patch.Y);
       
        _terrainManager.SetOneTerrainNeighbors(patch.X + 1, patch.Y);
       
        _terrainManager.SetOneTerrainNeighbors(patch.X - 1, patch.Y);
     
        _terrainManager.SetOneTerrainNeighbors(patch.X, patch.Y - 1);
       
        _terrainManager.SetOneTerrainNeighbors(patch.X, patch.Y + 1);

        if (!fastLoading)
        {
            await Awaitable.NextFrameAsync(cancellationToken: token);
        }
        await _terrainManager.AddPatchObjects(gx, gy, token);

        _terrainManager.RemoveLoadingPatches(gx, gy);

        if (false && _mapProvider.GetPathfinding() != null)
        {
            for (int px = 0; px < MapConstants.TerrainPatchSize; px += PathfindingConstants.BlockSize)
            {
                int worldx = gx * (MapConstants.TerrainPatchSize - 1) + px;
                for (int pz = 0; pz < MapConstants.TerrainPatchSize; pz += PathfindingConstants.BlockSize)
                {
                    int worldz = gy * (MapConstants.TerrainPatchSize - 1) + pz;

                    int finalx = worldx / PathfindingConstants.BlockSize;
                    int finalz = worldz / PathfindingConstants.BlockSize;

                    if (finalx < 0 || finalx >= _mapProvider.GetPathfinding().GetLength(0) ||
                        finalz < 0 || finalz >= _mapProvider.GetPathfinding().GetLength(1))
                    {
                        continue;
                    }
                    if (false && _mapProvider.GetPathfinding()[finalx,finalz])
                    {
                        float height = _terrainManager.SampleHeight(worldx, worldz);
                        GameObject sphere = await _assetService.LoadAssetAsync(AssetCategoryNames.Prefabs, "TestSphere", null, token);
                        sphere.name = "TestSphere_" + worldx + "_" + worldz;
                        GEntityUtils.AddToParent(sphere, patch.terrain.gameObject);
                        sphere.transform.position = new Vector3(worldx, height, worldz);
                    }
                }
            }
        }
    }

    private void OnDownloadTerrainBytes(object obj, object data, CancellationToken token)
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
            _logService.Error("Failed to download Bytes");
            return;
        }

        string filePath = patch.GetFilePath(false);

        ClientRepositoryCollection<TerrainPatchData> repo = new ClientRepositoryCollection<TerrainPatchData>(_logService);
       
        repo.SaveBytes(filePath, bytes);
        patch.DataBytes = bytes;
        LoadOneTerrainPatch(patch.X, patch.Y, _playerManager.GetEntity() == null, _token);
    }
}
	
