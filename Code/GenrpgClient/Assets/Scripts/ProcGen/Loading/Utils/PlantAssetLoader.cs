using Assets.Scripts.MapTerrain;
using Genrpg.Shared.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using Genrpg.Shared.Zones.WorldData;
using Genrpg.Shared.Logging.Interfaces;
using Assets.Scripts.ProcGen.Loading.Utils;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.MapServer.Services;
using Genrpg.Shared.Client.Assets.Services;
using Genrpg.Shared.Client.Assets.Constants;

public class FullDetailPrototype : BaseDetailPrototype
{
    public DetailPrototype proto = null;
}

public interface IPlantAssetLoader : IInjectable
{
    void SetupOneMapGrass(int gx, int gy, CancellationToken token);
}
public class PlantAssetLoader : IPlantAssetLoader
{
    private IAssetService _assetService;
    private IMapTerrainManager _terrainManager;
    private ILogService _logService;
    private IZonePlantValidator _zonePlantValidator;
    private IMapProvider _mapProvider;
    protected IClientEntityService _gameObjectService;

    public void SetupOneMapGrass(int gx, int gy, CancellationToken token)
    {
        TaskUtils.ForgetAwaitable(InnerSetupOneMapGrass(gx, gy, token));
    }
    private async Awaitable InnerSetupOneMapGrass(int gx, int gy, CancellationToken token)
    {
        TerrainPatchData patch = _terrainManager.GetTerrainPatch(gx, gy);

        if (patch == null)
        {
            return;
        }

        TerrainData tdata = patch.terrainData as TerrainData;

        if (tdata == null)
        {
            _logService.Error("Tdata missing: " + gx + " " + gy);

            return;
        }
        if (patch.grassAmounts == null || patch.mainZoneIds == null)
        {
            _logService.Error("Core Data missing: " + patch.grassAmounts + " " + patch.mainZoneIds + " " + gx + " " + gy);

            return;
        }

        MyRandom bendRand = new MyRandom(_mapProvider.GetMap().Seed + 3234992);

        UnityEngine.Color color = UnityEngine.Color.white;
        float amount = MathUtils.FloatRange(0.20f, 0.40f, bendRand);
        float speed = MathUtils.FloatRange(0.30f, 0.70f, bendRand);
        float strength = MathUtils.FloatRange(0.30f, 0.70f, bendRand);

        tdata.wavingGrassTint = UnityEngine.Color.white;
        tdata.wavingGrassAmount = amount;
        tdata.wavingGrassSpeed = speed;
        tdata.wavingGrassStrength = strength;

        List<FullDetailPrototype> fullProtoList = new List<FullDetailPrototype>();

        List<Zone> currentZones = new List<Zone>();

        if (patch.MainZoneIdList != null)
        {
            foreach (long zid in patch.MainZoneIdList)
            {
                Zone zone = _mapProvider.GetMap().Get<Zone>(zid);
                if (zone == null)
                {
                    continue;
                }
                currentZones.Add(zone);
                bool isMainTerrain = patch.MainZoneIdList.Contains(zid);
                _zonePlantValidator.UpdateValidPlantTypeList(zone, gx, gy, fullProtoList, isMainTerrain, token);

            }

            fullProtoList = fullProtoList.OrderByDescending(x => x.zoneIds.Count).ToList();

            while (fullProtoList.Count > MapConstants.MaxGrass)
            {
                fullProtoList.RemoveAt(fullProtoList.Count - 1);
            }


            AfterUpdateValidPrototypes(fullProtoList, gx, gy, token);
        }
        else
        {
            _logService.Error("No zones for grass: " + gx + " " + gy);
            return;
        }

        DetailPrototype[] protos = new DetailPrototype[fullProtoList.Count];
        int index = 0;

        foreach (FullDetailPrototype item in fullProtoList)
        {
            protos[index] = item.proto;
            index++;
        }

        for (int i = 0; i < protos.Length; i++)
        {
            if (protos[i] == null)
            {
                _logService.Error("No proto: " + gx + " " + gy + " idx " + i);
                return;
            }
        }

        bool haveAllArt = false;
        while (!haveAllArt)
        {
            haveAllArt = true;
            for (int i = 0; i < protos.Length; i++)
            {
                if (protos[i] == null || (protos[i].prototypeTexture == null && protos[i].prototype == null))
                {
                    haveAllArt = false;
                    break;
                }
            }
            if (!haveAllArt)
            {
                await Awaitable.NextFrameAsync(cancellationToken: token);
            }
        }

        tdata.detailPrototypes = protos;

        await Awaitable.NextFrameAsync(cancellationToken: token);

        tdata.RefreshPrototypes();

        await Awaitable.NextFrameAsync(cancellationToken: token);

        List<int[,]> detailblock = new List<int[,]>();

        for (int i = 0; i < fullProtoList.Count; i++)
        {
            detailblock.Add(new int[MapConstants.TerrainPatchSize, MapConstants.TerrainPatchSize]);
            fullProtoList[i].Index = i;
        }

        int maxOffset = 7;
        int totalOffset = 7 * 2 + 1;

        if (patch.grassAmounts == null)
        {
            patch.grassAmounts = new ushort[MapConstants.TerrainPatchSize, MapConstants.TerrainPatchSize, MapConstants.MaxGrass];
        }

        for (int x = 0; x < MapConstants.TerrainPatchSize - 1; x++)
        {
            for (int y = 0; y < MapConstants.TerrainPatchSize - 1; y++)
            {
                int finalX = x;
                int finalY = y;

                int offsetX = x + ((x * 11 + y * 17 + gx * 31 + gy * 47) % totalOffset - maxOffset);
                int offsetY = y + ((x * 43 + y * 59 + gx * 37 + gy * 53) % totalOffset - maxOffset);

                if (offsetX >= 0 && offsetX < MapConstants.TerrainPatchSize - 1 &&
                    offsetY >= 0 && offsetY < MapConstants.TerrainPatchSize - 1)
                {
                    finalX = offsetX;
                    finalY = offsetY;
                }

                long zoneId = patch.subZoneIds[finalX, finalY];

                if (zoneId < 1)
                {
                    zoneId = patch.mainZoneIds[finalX, finalY];
                }

                List<FullDetailPrototype> currentProtos = fullProtoList.Where(x => x.zoneIds.Contains(zoneId)).ToList();

                if (currentProtos.Count < 1)
                {
                    zoneId = patch.MainZoneIdList.First();

                    currentProtos = fullProtoList.Where(x => x.zoneIds.Contains(zoneId)).ToList();

                    if (currentProtos.Count < 1)
                    {
                        continue;
                    }
                }

                while (currentProtos.Count < MapConstants.MaxGrass)
                {
                    currentProtos.Add(currentProtos[currentProtos.Count / 3]);
                }

                for (int i = 0; i < MapConstants.MaxGrass && i < currentProtos.Count; i++)
                {
                    int offset = 0;
                    if (patch.MainZoneIdList.Count >= 2 && zoneId == patch.MainZoneIdList[1] &&
                        i + MapConstants.MaxGrass < currentProtos.Count)
                    {
                        offset = MapConstants.MaxGrass;
                    }
                    if (patch.grassAmounts[x, y, i] > 0)
                    {
                        FullDetailPrototype proto = currentProtos[i+offset];
                        detailblock[proto.Index][x, y] = (int)patch.grassAmounts[x, y, i];
                    }
                }
            }
        }

        for (int l = 0; l < detailblock.Count; l++)
        {
            tdata.SetDetailLayer(0, 0, l, detailblock[l]);
        }
    }

    private void AfterUpdateValidPrototypes(List<FullDetailPrototype> fullList, int gx, int gy, CancellationToken token)
    {
        foreach (FullDetailPrototype full in fullList)
        {
            DetailPrototype dp = new DetailPrototype();
            dp.renderMode = DetailRenderMode.Grass;
            dp.healthyColor = Color.white;
            dp.dryColor = Color.white;

            // Now we know the plant object exists, so use the zone type plant data also.

            float minHeight = full.plantType.MinScale * AddPlants.GrassBaseScale;
            float maxHeight = full.plantType.MaxScale * AddPlants.GrassBaseScale;

            dp.maxHeight = maxHeight;
            dp.maxWidth = maxHeight;
            dp.minHeight = minHeight;
            dp.minWidth = minHeight;

            dp.noiseSpread = 1.0f;
            full.proto = dp;
            if (gx >= 0 && gy >= 0)
            {
                _assetService.LoadAsset(AssetCategoryNames.Grass, full.plantType.Art, OnDownloadGrass, full, null, token);
            }
        }
    }
private void OnDownloadGrass(object obj, object data, CancellationToken token)
{
    FullDetailPrototype full = data as FullDetailPrototype;
    if (full == null || full.proto == null)
    {
        return;
    }

    GameObject go = obj as GameObject;

    if (go == null)
    {
        _logService.Error("no GameObject: " + obj + " " + full.plantType.Art);
        return;
    }

    TextureList tlist = go.GetComponent<TextureList>();
    if (tlist == null || tlist.Textures == null || tlist.Textures.Count < 1)
    {
        full.proto.prototype = go;
        full.proto.renderMode = DetailRenderMode.VertexLit;
        full.proto.usePrototypeMesh = true;
    }
    else
    {
        full.proto.prototypeTexture = tlist.Textures[0];
        full.proto.healthyColor = Color.white;
        full.proto.renderMode = DetailRenderMode.GrassBillboard;
        full.proto.dryColor = Color.white;
    }

    TerrainPatchData grid = _terrainManager.GetMapGrid(full.XGrid, full.YGrid) as TerrainPatchData;
    if (grid == null)
    {
        _gameObjectService.Destroy(go);
        return;
    }
    Terrain terr = grid.terrain as Terrain;
    if (terr == null)
    {
        _gameObjectService.Destroy(go);
        return;
    }
    _gameObjectService.AddToParent(go, terr.gameObject);
    go.SetActive(false);
}
}