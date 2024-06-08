using System;
using System.Collections.Generic;
using GEntity = UnityEngine.GameObject;
using Genrpg.Shared.Constants;
using Genrpg.Shared.Interfaces;

using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Zones.Entities;
using Genrpg.Shared.ProcGen.Entities;
using System.Threading;
using UnityEngine;
using Genrpg.Shared.GameSettings.Interfaces;
using Genrpg.Shared.ProcGen.Settings.Trees;
using Genrpg.Shared.Zones.Settings;
using Genrpg.Shared.Zones.WorldData;
using Genrpg.Shared.Core.Entities;

public class TreeObjectLoader : BaseObjectLoader
{
    const int ScaleStepCount = 20;

    public override bool LoadObject(PatchLoadData loadData, uint objectId,
        int x, int y, Zone currZone, ZoneType currZoneType, CancellationToken token)
    {
        FullTreePrototype fullProto = null;
        TreeType treeType = null;

        if (loadData == null || loadData.terrManager == null)
        {
            return false;
        }

        SetupZoneTreeCache(_gs);

        string assetCategory = AssetCategoryNames.Trees;

        treeType = _gameData.Get<TreeTypeSettings>(_gs.ch).Get(objectId);

        if (treeType == null)
        {
            return false;
        }

        if (treeType.HasFlag(TreeFlags.IsBush))
        {
            assetCategory = AssetCategoryNames.Bushes;
        }

        if (!treeType.HasFlag(TreeFlags.IsWaterItem) &&
            _mapProvider.GetMap().OverrideZoneId > 0 && _mapProvider.GetMap().OverrideZonePercent > 0)
        {
            if (loadData.patch.overrideZoneScales[x,y] < _mapProvider.GetMap().OverrideZonePercent)
            {
                Zone zone = _mapProvider.GetMap().Get<Zone>(_mapProvider.GetMap().OverrideZoneId);
                if (zone != null)
                {
                    List<long> okTreeIds = new List<long>();

                    if (treeType.HasFlag(TreeFlags.IsBush))
                    {
                        if (_md.zoneBushIds.TryGetValue(zone.ZoneTypeId, out List<long> bushIds))
                        {
                            okTreeIds = bushIds;
                        }
                    }
                    else
                    {
                        if (_md.zoneTreeIds.TryGetValue(zone.ZoneTypeId, out List<long> treeIds))
                        {
                            okTreeIds = treeIds;
                        }
                    }
                    
                    if (okTreeIds.Count > 0)
                    {
                        long treeTypeId = okTreeIds[(loadData.gx * 191 + loadData.gy * 2189 + x * 108061 + y * 857) % okTreeIds.Count];

                        TreeType treeType2 = _gameData.Get<TreeTypeSettings>(_gs.ch).Get(treeTypeId);

                        if (treeType2 != null)
                        {
                            treeType = treeType2;
                        }

                    }
                }
            }
        }


        long index = GetIndexForTree(currZone, treeType, loadData.gx * y + loadData.gy * x + x * 11 + y * 31);
        string artName = treeType.Art + index;
        if (false && treeType.HasFlag(TreeFlags.DirectPlaceObject))
        {
            DownloadObjectData dlo = new DownloadObjectData();
            dlo.gameItem = treeType;
            dlo.url = artName;
            dlo.loadData = loadData;
            dlo.x = x;
            dlo.y = y;
            dlo.zone = currZone;
            dlo.zoneType = currZoneType;
            dlo.assetCategory = assetCategory;


            long placementSeed = 17041 + x * 9479 + y * 2281 + loadData.gx * 5281 + loadData.gy * 719 +
                loadData.gx * y + loadData.gy * x;

            treeType.Scale = 1.0f; // TODO Fix
            float minScale = treeType.Scale;
            float maxScale = treeType.Scale * 1.50f;
            float finalScale = minScale + (maxScale - minScale) * (placementSeed % (ScaleStepCount + 1)) / ScaleStepCount;

            if (treeType.HasFlag(TreeFlags.IsBush))
            {
                finalScale *= AddTrees.BushSizeScale;
            }
            else
            {
                finalScale *= AddTrees.TreeSizeScale;
            }
            dlo.scale = finalScale;

            _assetService.LoadAsset(assetCategory, artName, OnDownloadObjectDirect, dlo, null, token);

        }
        else
        {
            fullProto = new FullTreePrototype();
            fullProto.treeType = treeType;
            

            StartPlaceInstance(loadData, treeType, assetCategory, artName, x, y, null, token);
        }
        return true;
    }

    protected void OnDownloadObjectDirect(object obj, object data, CancellationToken token)
    {
        OnDownloadObject(obj, data, token);
    }


    public long GetIndexForTree(Zone zone, TreeType treeType, int localSeed)
    {
        if (zone == null || treeType == null)
        {
            return 1;
        }

        if (treeType.VariationCount > 1)
        {
            return 1 + (zone.Seed % 100000000 + treeType.IdKey * 12 + treeType.IdKey * treeType.IdKey + localSeed * 13 + localSeed * treeType.IdKey * 17) % treeType.VariationCount;
        }
        return 1;

    }



    protected void StartPlaceInstance(PatchLoadData loadData,
        IIndexedGameItem dataItem,
        string assetCategory, string artName, int x, int y, object extraData, CancellationToken token)
    {
        if (string.IsNullOrEmpty(artName) || loadData == null)
        {
            return;
        }

        string key = artName + assetCategory;

        TreePrototype proto = null;
        int protoIndex = -1;

        for (int i = 0; i < loadData.objectProtos.Count; i++)
        {
            if (loadData.objectProtos[i].Name == key)
            {
                proto = loadData.objectProtos[i].Prototype as TreePrototype;
                protoIndex = i;
                break;
            }
        }

        if (proto == null || protoIndex < 0)
        {
            TreePrototype tp = new TreePrototype();
            ObjectPrototype op = new ObjectPrototype() 
            {   
                Name = key, 
                Prototype = tp, 
                DataItem = dataItem, 
                terrManager = loadData.terrManager,
                token = token,
            };
            loadData.objectProtos.Add(op);
            proto = tp;
            protoIndex = loadData.objectProtos.Count - 1;


            loadData.terrManager.AddTerrainProtoPatch(artName, loadData.gx, loadData.gy);
            GEntity currObject = loadData.terrManager.GetTerrainProtoObject(artName);

            if (currObject != null)
            {
                tp.prefab = currObject;
                PlaceInstance(dataItem, loadData.treeInstances, protoIndex, loadData.gx, loadData.gy, x, y, extraData);
            }
            else
            {
                _assetService.LoadAsset(assetCategory, artName, OnDownloadPrototype, op, loadData.protoParent, token);
            }
        }

        PlaceInstance(dataItem, loadData.treeInstances, protoIndex, loadData.gx, loadData.gy, x, y, extraData);
    }



    private void PlaceInstance(IIndexedGameItem dataItem, List<TreeInstance> instances, int protoIndex, int gx, int gy, int x, int y, object data)
    {

        long placementSeed = 17041 + x * 9479 + y * 2281 + gx * 5281 + gy * 719 +
            gx * y + gy * x;

        int wx = gx * (MapConstants.TerrainPatchSize - 1) + x;
        int wy = gy * (MapConstants.TerrainPatchSize - 1) + y;
        float ddx = MathUtils.SeedFloatRange(placementSeed * 13, 143, -0.5f, 0.5f, 101);
        float ddy = MathUtils.SeedFloatRange(placementSeed * 17, 149, -0.5f, 0.5f, 101);
        float height = _terrainManager.SampleHeight(wx, wy);

        TreeInstance ti = new TreeInstance();
        ti.prototypeIndex = protoIndex;
        

        float ex = x + ddx;
        float ey = height;
        float ez = y + ddy;
        bool isbush = false;
        TreeType tt = dataItem as TreeType;
        if (tt != null)
        {

            tt.Scale = 1.0f; // TODO Fix
            float minScale = tt.Scale;
            float maxScale = minScale * 1.5f;

            if (tt.HasFlag(TreeFlags.IsBush))
            {
                isbush = true;
                minScale = 1.0f;
                maxScale = 1.0f;
            }
            else
            {
                maxScale *= AddTrees.TreeSizeScale;
            }
            float finalScale = minScale + (maxScale - minScale) * (placementSeed % (ScaleStepCount + 1)) / ScaleStepCount;

            GVector3 currNormal = _terrainManager.GetInterpolatedNormal(_mapProvider.GetMap(), wx, wy);

            float offsetScale = 1.0f;
            if (!tt.HasFlag(TreeFlags.IsBush))
            {
                GVector3 offset = currNormal * -(0.3f + (2.5f * (1 - currNormal.y)) / Math.Max(1.0f, offsetScale));
                ex += offset.x;
                ey += offset.y-1.0f;
                ez += offset.z;
            }

            ti.heightScale = finalScale;
            ti.widthScale = finalScale;
            ti.rotation = (placementSeed * 1.7f);

        }
        float posMult = 1.0f / (MapConstants.TerrainPatchSize - 1);
        float extraDepth = (isbush ? 0 : 1.5f);
        ti.position = GVector3.Create(ex * posMult, (ey-extraDepth) / MapConstants.MapHeight, ez * posMult);
        instances.Add(ti);
    }


    private void OnDownloadPrototype(object obj, object data, CancellationToken token)
    {
        GEntity go = obj as GEntity;
        if (go == null)
        {
            return;
        }

        ObjectPrototype op = data as ObjectPrototype;
        if (op == null || op.Prototype == null || !TokenUtils.IsValid(op.token))
        {
            GEntityUtils.Destroy(go);
            return;
        }

        List<MeshRenderer> renderers = GEntityUtils.GetComponents<MeshRenderer>(go);

        foreach (MeshRenderer renderer in renderers)
        {
            List<Material> oldList = new List<Material>();
            renderer.GetMaterials(oldList);
            foreach (Material mat in oldList)
            {
            }
        }

        go = op.terrManager.AddOrReuseTerrainProtoObject(op.Name, go);


        op.Prototype.prefab = go;

        TreeType treeType = op.DataItem as TreeType;
        if (treeType != null && treeType.HasFlag(TreeFlags.IsBush))
        {
            LODGroup lodGroup = go.GetComponent<LODGroup>();
            if (lodGroup != null)
            {
                for (int c = 0; c < go.transform().childCount; c++)
                {
                    Transform child = go.transform().GetChild(c);
                    if (child != null && child.entity() != null && child.name.IndexOf("LOD0") < 0)
                    {

                        child.entity().SetActive(false);
                    }
                }
                lodGroup.enabled = false;
                lodGroup.animateCrossFading = false;
                lodGroup.fadeMode = LODFadeMode.None;
            }
        }

        //GEntityUtils.SetStatic(go, true);
        GEntityUtils.SetLayer(go, LayerNames.ObjectLayer);
        go.transform().localPosition = GVector3.Create(0, -2000, 0);
    }

    private void SetupZoneTreeCache(IUnityGameState gs)
    {
        if (_md.zoneTreeIds != null && _md.zoneBushIds != null)
        {
            return;
        }

        _md.zoneTreeIds = new Dictionary<long, List<long>>();
        _md.zoneBushIds = new Dictionary<long, List<long>>();

        TreeTypeSettings treeSettings = _gameData.Get<TreeTypeSettings>(gs.ch);
        foreach (IGameSettings settings in _gameData.Get<ZoneTypeSettings>(gs.ch).GetChildren())
        {
            if (settings is ZoneType zoneType)
            {
                List<long> treeList = new List<long>();
                List<long> bushList = new List<long>();

                _md.zoneTreeIds[zoneType.IdKey] = treeList;
                _md.zoneBushIds[zoneType.IdKey] = bushList;

                foreach (ZoneTreeType ztt in zoneType.TreeTypes)
                {
                    TreeType treeType = treeSettings.Get(ztt.TreeTypeId);

                    if (treeType.HasFlag(TreeFlags.IsWaterItem))
                    {
                        continue;
                    }

                    if (treeType.HasFlag(TreeFlags.IsBush))
                    {
                        bushList.Add(treeType.IdKey);
                    }
                    else
                    {
                        treeList.Add(treeType.IdKey);
                    }
                }
            }
        }

    }
}
