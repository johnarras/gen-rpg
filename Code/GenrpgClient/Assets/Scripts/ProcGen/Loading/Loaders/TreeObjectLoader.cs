using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Genrpg.Shared.Core.Entities;

using Services;
using UnityEngine;
using Genrpg.Shared.Constants;
using Genrpg.Shared.Interfaces;
using Entities;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Zones.Entities;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.Shared.ProcGen.Entities;
using System.Threading;

public class TreeObjectLoader : BaseObjectLoader
{
    public TreeObjectLoader(UnityGameState gs) : base(gs)
    {
    }
    public override bool LoadObject(UnityGameState gs, PatchLoadData loadData, uint objectId,
        int x, int y, Zone currZone, ZoneType currZoneType, CancellationToken token)
    {
        FullTreePrototype fullProto = null;
        TreeType treeType = null;

        if (loadData == null || loadData.terrManager == null)
        {
            return false;
        }


        string assetCategory = AssetCategory.Trees;

        treeType = gs.data.GetGameData<ProcGenSettings>().GetTreeType(objectId);

        if (treeType == null)
        {
            return false;
        }

        if (treeType.HasFlag(TreeFlags.IsBush))
        {
            assetCategory = AssetCategory.Bushes;
        }
        long index = gs.md.GetIndexForTree(gs, currZone, treeType, loadData.gx * y + loadData.gy * x + x * 11 + y * 31);
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

            float minScale = treeType.Scale;
            float maxScale = treeType.Scale * 1.5f;
            float finalScale = minScale + (maxScale - minScale) * (placementSeed % (MapTerrainManager.ScaleStepCount + 1)) / MapTerrainManager.ScaleStepCount;

            if (treeType.HasFlag(TreeFlags.IsBush))
            {
                finalScale *= AddTrees.BushSizeScale;
            }
            else
            {
                finalScale *= AddTrees.TreeSizeScale;
            }
            dlo.scale = finalScale;

            _assetService.LoadAsset(gs, assetCategory, artName, OnDownloadObjectDirect, dlo, null, token);

        }
        else
        {
            fullProto = new FullTreePrototype();
            fullProto.treeType = treeType;
            

            StartPlaceInstance(gs, loadData, treeType, assetCategory, artName, x, y, null, token);
        }
        return true;
    }

    protected void OnDownloadObjectDirect(UnityGameState gs, string url, object obj, object data, CancellationToken token)
    {
        OnDownloadObject(gs, url, obj, data, token);
    }

    protected void StartPlaceInstance(UnityGameState gs, PatchLoadData loadData,
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
            GameObject currObject = loadData.terrManager.GetTerrainProtoObject(artName);

            if (currObject != null)
            {
                tp.prefab = currObject;
                PlaceInstance(gs, dataItem, loadData.treeInstances, protoIndex, loadData.gx, loadData.gy, x, y, extraData);
            }
            else
            {
                _assetService.LoadAsset(gs, assetCategory, artName, OnDownloadPrototype, op, loadData.protoParent, token);
            }
        }

        PlaceInstance(gs, dataItem, loadData.treeInstances, protoIndex, loadData.gx, loadData.gy, x, y, extraData);
    }



    private void PlaceInstance(UnityGameState gs, IIndexedGameItem dataItem, List<TreeInstance> instances, int protoIndex, int gx, int gy, int x, int y, object data)
    {

        long placementSeed = 17041 + x * 9479 + y * 2281 + gx * 5281 + gy * 719 +
            gx * y + gy * x;

        int wx = gx * (MapConstants.TerrainPatchSize - 1) + x;
        int wy = gy * (MapConstants.TerrainPatchSize - 1) + y;
        float ddx = MathUtils.SeedFloatRange(placementSeed * 13, 143, -0.5f, 0.5f, 101);
        float ddy = MathUtils.SeedFloatRange(placementSeed * 17, 149, -0.5f, 0.5f, 101);
        float height = gs.md.SampleHeight(gs, wx, 2000, wy);

        TreeInstance ti = new TreeInstance();
        ti.prototypeIndex = protoIndex;
        

        float ex = x + ddx;
        float ey = height;
        float ez = y + ddy;
        bool isbush = false;
        TreeType tt = dataItem as TreeType;
        if (tt != null)
        {

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
            float finalScale = minScale + (maxScale - minScale) * (placementSeed % (MapTerrainManager.ScaleStepCount + 1)) / MapTerrainManager.ScaleStepCount;

            Vector3 currNormal = gs.md.GetInterpolatedNormal(gs, gs.map, wx, wy);

            float offsetScale = 1.0f;
            if (!tt.HasFlag(TreeFlags.IsBush))
            {
                Vector3 offset = -currNormal * (0.3f + (2.5f * (1 - currNormal.y)) / Math.Max(1.0f, offsetScale));
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
        ti.position = new Vector3(ex * posMult, (ey-extraDepth) / MapConstants.MapHeight, ez * posMult);
        instances.Add(ti);
    }


    private void OnDownloadPrototype(UnityGameState gs, String url, object obj, object data, CancellationToken token)
    {
        GameObject go = obj as GameObject;
        if (go == null)
        {
            return;
        }

        ObjectPrototype op = data as ObjectPrototype;
        if (op == null || op.Prototype == null || !TokenUtils.IsValid(op.token))
        {
            GameObject.Destroy(go);
            return;
        }

        List<MeshRenderer> renderers = GameObjectUtils.GetComponents<MeshRenderer>(go);

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
                for (int c = 0; c < go.transform.childCount; c++)
                {
                    Transform child = go.transform.GetChild(c);
                    if (child != null && child.gameObject != null && child.name.IndexOf("LOD0") < 0)
                    {

                        child.gameObject.SetActive(false);
                    }
                }
                lodGroup.enabled = false;
                lodGroup.animateCrossFading = false;
                lodGroup.fadeMode = LODFadeMode.None;
            }
        }

        //GameObjectUtils.SetStatic(go, true);
        GameObjectUtils.SetLayer(go, LayerNames.ObjectLayer);
        go.transform.localPosition = new Vector3(0, -2000, 0);
    }
}
