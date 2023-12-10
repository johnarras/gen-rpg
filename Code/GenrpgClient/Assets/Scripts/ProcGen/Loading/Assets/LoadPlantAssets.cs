

using Assets.Scripts.MapTerrain;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Zones.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using GEntity = UnityEngine.GameObject;


public class FullDetailPrototype : BaseDetailPrototype
{
    public DetailPrototype proto = null;
}

public class LoadPlantAssets
{
    private IAssetService _assetService;
    private IMapTerrainManager _terrainManager;



    public void SetupOneMapGrass(UnityGameState gs, int gx, int gy, CancellationToken token)
    {
        if (_terrainManager == null || _assetService == null)
        {
            gs.loc.Resolve(this);
        }
        InnerSetupOneMapGrass(gs, gx, gy, token).Forget();
    }
    private async UniTask InnerSetupOneMapGrass(UnityGameState gs, int gx, int gy, CancellationToken token)
    {
        if (
              gs.md.terrainPatches == null ||
              gx < 0 || gy < 0 || gx >= MapConstants.MaxTerrainGridSize ||
              gy >= MapConstants.MaxTerrainGridSize)
        {
            gs.logger.Error("Bail out of making grass: " + gx + " " + gy);
            return;
        }

        TerrainPatchData patch = gs.md.terrainPatches[gx, gy];

        if (patch == null)
        {
            gs.logger.Error("Patch missing " + gx + " " + gy);

            return;
        }

        TerrainData tdata = patch.terrainData as TerrainData;

        if (tdata == null)
        {
            gs.logger.Error("Tdata missing: " + gx + " " + gy);

            return;
        }
        if (patch.grassAmounts == null || patch.mainZoneIds == null)
        {
            gs.logger.Error("Core Data missing: " + patch.grassAmounts + " " + patch.mainZoneIds + " " + gx + " " + gy);

            return;
        }

        MyRandom bendRand = new MyRandom(gs.map.Seed + 3234992);

        Color color = Color.white;
        float amount = MathUtils.FloatRange(0.20f, 0.40f, bendRand);
        float speed = MathUtils.FloatRange(0.30f, 0.70f, bendRand);
        float strength = MathUtils.FloatRange(0.30f, 0.70f, bendRand);

        tdata.wavingGrassTint = Color.white;
        tdata.wavingGrassAmount = amount;
        tdata.wavingGrassSpeed = speed;
        tdata.wavingGrassStrength = strength;

        List<FullDetailPrototype> fullProtoList = new List<FullDetailPrototype>();

        List<Zone> currentZones = new List<Zone>();

        if (patch.FullZoneIdList != null)
        {
            foreach (long zid in patch.FullZoneIdList)
            {
                Zone zone = gs.map.Get<Zone>(zid);
                if (zone == null)
                {
                    continue;
                }
                currentZones.Add(zone);
                bool isMainTerrain = patch.MainZoneIdList.Contains(zid);
                AddPlants addplants = new AddPlants();
                addplants.UpdateValidPlantTypeList(gs, zone, gx, gy, fullProtoList, isMainTerrain, token);
                AfterUpdateValidPrototypes(gs, fullProtoList, gx, gy, token);
            }
        }
        else
        {
            gs.logger.Error("No zones for grass: " + gx + " " + gy);
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
                gs.logger.Error("No proto: " + gx + " " + gy + " idx " + i);
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
                await UniTask.NextFrame( cancellationToken: token);
            }
        }

        tdata.detailPrototypes = protos;

        await UniTask.NextFrame( cancellationToken: token);

        tdata.RefreshPrototypes();

        await UniTask.NextFrame( cancellationToken: token);

        if (patch.grassAmounts == null)
        {
            return;
        }

        List<int[,]> detailblock = new List<int[,]>();

        for (int i = 0; i < fullProtoList.Count; i++)
        {
            detailblock.Add(new int[MapConstants.TerrainPatchSize, MapConstants.TerrainPatchSize]);
        }

        int maxOffset = 7;
        int totalOffset = 7 * 2 + 1;

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
                    continue;
                }

                while (currentProtos.Count < MapConstants.MaxGrass)
                {
                    currentProtos.Add(currentProtos[currentProtos.Count / 3]);
                }

                for (int i = 0; i < MapConstants.MaxGrass && i < currentProtos.Count; i++)
                {
                    if (patch.grassAmounts[x, y, i] > 0)
                    {
                        FullDetailPrototype proto = currentProtos[i];
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

    private void AfterUpdateValidPrototypes(UnityGameState gs, List<FullDetailPrototype> fullList, int gx, int gy, CancellationToken token)
    {
        foreach (FullDetailPrototype full in fullList)
        {
            DetailPrototype dp = new DetailPrototype();
            dp.renderMode = DetailRenderMode.Grass;
            dp.healthyColor = GColor.white;
            dp.dryColor = GColor.white;

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
                _assetService.LoadAsset(gs, AssetCategoryNames.Grass, full.plantType.Art, OnDownloadGrass, full, null, token);
            }
        }
    }
private void OnDownloadGrass(UnityGameState gs, string url, object obj, object data, CancellationToken token)
{
    FullDetailPrototype full = data as FullDetailPrototype;
    if (full == null || full.proto == null)
    {
        return;
    }

    GEntity go = obj as GEntity;

    if (go == null)
    {
        gs.logger.Error("no GEntity: " + url + " " + obj + " " + full.plantType.Art);
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
        full.proto.healthyColor = GColor.white;
        full.proto.renderMode = DetailRenderMode.GrassBillboard;
        full.proto.dryColor = GColor.white;
    }

    TerrainPatchData grid = _terrainManager.GetMapGrid(gs, full.XGrid, full.YGrid) as TerrainPatchData;
    if (grid == null)
    {
        GEntityUtils.Destroy(go);
        return;
    }
    Terrain terr = grid.terrain as Terrain;
    if (terr == null)
    {
        GEntityUtils.Destroy(go);
        return;
    }
    GEntityUtils.AddToParent(go, terr.entity());
    go.SetActive(false);
}
}