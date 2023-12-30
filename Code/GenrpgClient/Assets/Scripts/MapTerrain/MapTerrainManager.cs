using System;
using System.Linq;
using System.Collections.Generic;
using GEntity = UnityEngine.GameObject;
using Genrpg.Shared.Core.Entities;
using Cysharp.Threading.Tasks;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Utils.Data;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Zones.Entities;
using Genrpg.Shared.ProcGen.Entities;
using System.Threading;
using Genrpg.Shared.Entities.Constants;
using Assets.Scripts.MapTerrain;
using UnityEngine; // Needed
using System.Threading.Tasks;
using Genrpg.Shared.Zones.Settings;
using Genrpg.Shared.Zones.WorldData;

public class PatchLoadData
{
    public TerrainPatchData patch = null;
    public List<ObjectPrototype> objectProtos = new List<ObjectPrototype>();
    public List<TreeInstance> treeInstances = new List<TreeInstance>();
    public GEntity protoParent = null;
    public int StartX = 0;
    public int StartY = 0;
    public int gx = 0;
    public int gy = 0;

    public IMapTerrainManager terrManager;

    Action AfterPlaceObject;


    public int MapX(int x)
    {
        return x + StartX;
    }

    public int MapY(int y)
    {
        return y + StartY;
    }


}

public delegate void AfterObjectLoad(UnityGameState gs, GEntity go, DownloadObjectData data, CancellationToken token);

public class DownloadObjectData
{
    public UnityGameState ugs;
    public Zone zone;
    public ZoneType zoneType;
    public string url;
    public IIndexedGameItem gameItem;
    public PatchLoadData loadData;
    public int x;
    public int y;
    public float finalZ;
    public float zOffset;
    public MyPointF rotation;
    public object data;
    public string assetCategory;
    public TreeInstance instance;
    public TreePrototype prototype;
    public TerrainData tdata;
    public float scale = 1.0f;
    public bool allowRandomPlacement = true;


    public AfterObjectLoad AfterLoad;

    public long placementSeed;
    public float height;
    public float ddx;
    public float ddy;

}

public class ObjectPrototype
{
    public string Name;
    public TreePrototype Prototype;
    public IIndexedGameItem DataItem;
    public IMapTerrainManager terrManager;
    public CancellationToken token;
}

public class TerrainProtoObject
{
    public string Name;
    public GEntity Prefab;
    public List<int> PatchIds = new List<int>();
}


public interface IMapTerrainManager : ISetupService
{
    GEntity GetTerrainTextureParent();
    GEntity GetTerrainProtoObject(string name);
    void AddTerrainProtoPatch(string name, int gx, int gy);
    void RemovePatchFromPrototypes(int gx, int gy);
    TerrainTextureData GetFromTerrainTextureCache(string textureName);
    void Clear(UnityGameState gs);
    UniTask SetupOneTerrainPatch(UnityGameState gs, int gx, int gy, CancellationToken token);
    bool AddingPatches(UnityGameState gs);
    List<Terrain> GetTerrains(UnityGameState gs);
    TerrainPatchData GetPatchFromMapPos(UnityGameState gs, float worldx, float worldy);
    TerrainPatchData GetMapGrid(UnityGameState gs, int gx, int gy);
    UniTask AddPatchObjects(UnityGameState gs, int gx, int gy, CancellationToken token);
    void AddToTerrainTextureCache(string textureName, TerrainTextureData data);
    void ClearPatches(UnityGameState gs);
    GEntity GetPrototypeParent();
    GEntity AddOrReuseTerrainProtoObject(string name, GEntity go);
    void ClearMapObjects(UnityGameState gs);
}

public class MapTerrainManager : BaseBehaviour, IMapTerrainManager
{

    public static long PatchesAdded = 0;
    public static long PatchesRemoved = 0;

    public const int MaxLoadUnloadCheckTicks = 13;
    public const int MaxPatchLoadTicks = 23;

    // Used to move terrain out of the way when we enter a dungeon.
    public const int ShiftYOffset = -5000;

    public const string PrototypeParent = "PrototypeParent";

    public const int LoadObjectCountBeforePause = 20;
    public const int ScaleStepCount = 20;

    // Used to move world objects out of the way when we enter a dungeon.

    private Dictionary<long, BaseObjectLoader> _staticLoaders = new Dictionary<long, BaseObjectLoader>();

    public Dictionary<string, TerrainTextureData> _terrainTextureCache = new Dictionary<string, TerrainTextureData>();

    public GEntity _prototypeParent = null;
    public GEntity _terrainTextureParent = null;


    protected IZoneGenService _zoneGenService;


    private Dictionary<string, TerrainProtoObject> _terrainProtoObjectData = new Dictionary<string, TerrainProtoObject>();

    public async Task Setup(GameState gs, CancellationToken token)
    {
        AddTokenUpdate(TerrainUpdate, UpdateType.Regular);
        _prototypeParent = GEntityUtils.FindSingleton(PrototypeParent, true);
        _terrainTextureParent = GEntityUtils.FindSingleton(MapConstants.TerrainTextureRoot, true);
        SetupLoaders();
        await UniTask.CompletedTask;
    }




    public GEntity GetTerrainTextureParent()
    {
        return _terrainTextureParent;
    }

    public GEntity GetTerrainProtoObject(string name)
    {
        if (_terrainProtoObjectData.ContainsKey(name))
        {
            return _terrainProtoObjectData[name].Prefab;
        }

        return null;
    }


    private int GetProtoPatchIndex(int gx, int gy)
    {
        return 1000 * gx + gy;
    }

    public void AddTerrainProtoPatch (string name, int gx, int gy)
    {
        int index = GetProtoPatchIndex(gx, gy);

        if (!_terrainProtoObjectData.ContainsKey(name))
        {
            _terrainProtoObjectData[name] = new TerrainProtoObject() { Name = name };
        }

        TerrainProtoObject tpo = _terrainProtoObjectData[name];

        if (!tpo.PatchIds.Contains(index))
        {
            tpo.PatchIds.Add(index);
        }
    }

    public void RemovePatchFromPrototypes(int gx, int gy)
    {

        int index = GetProtoPatchIndex(gx, gy);

        List<string> prefabRemoveList = new List<string>();

        foreach (TerrainProtoObject val in _terrainProtoObjectData.Values)
        {
            if (val.PatchIds.Contains(index))
            {
                val.PatchIds.Remove(index);
                if (val.PatchIds.Count < 1)
                {
                    prefabRemoveList.Add(val.Name);
                }
            }
        }

        foreach (string prefabName in prefabRemoveList)
        {
            GEntityUtils.Destroy(_terrainProtoObjectData[prefabName].Prefab);
            _terrainProtoObjectData.Remove(prefabName);
        }

    }

    public TerrainTextureData GetFromTerrainTextureCache(string textureName)
    {

        if (_terrainTextureCache.TryGetValue(textureName, out TerrainTextureData data))
        {
            return data;
        }
        return null;
    }

    public void AddToTerrainTextureCache (string textureName, TerrainTextureData data)
    {
        if (!_terrainTextureCache.ContainsKey(textureName))
        {
            _terrainTextureCache[textureName] = data;
        }
    }

    public void RemoveFromTerrainTextureCache(string texName)
    {
        if (_terrainTextureCache.ContainsKey(texName))
        {
            _terrainTextureCache.Remove(texName);
        }
    }

    public void Clear(UnityGameState gs)
    {
        _terrainProtoObjectData = new Dictionary<string, TerrainProtoObject>();
        GEntityUtils.DestroyAllChildren(_prototypeParent);
        GEntityUtils.DestroyAllChildren(_terrainTextureParent);
        _terrainTextureCache.Clear();

        if (gs.md != null)
        {
            gs.md.loadingPatchList = new List<MyPoint>();
            gs.md.addPatchList = new List<MyPoint>();
            gs.md.removePatchList = new List<MyPoint>();
        }
        ClearMapObjects(gs);
        ClearPatches(gs);
    }

    public GEntity AddOrReuseTerrainProtoObject(string name, GEntity go)
    {
        if (string.IsNullOrEmpty(name) || go == null)
        {
            return null;
        }

        if (_terrainProtoObjectData.ContainsKey(name))
        {
            TerrainProtoObject tpObject = _terrainProtoObjectData[name];

            if (tpObject.Prefab != null)
            {
                GEntityUtils.Destroy(go);
                return tpObject.Prefab;
            }
            else
            {
                tpObject.Prefab = go;
                return go;
            }
        }
        else
        {
            TerrainProtoObject tpo = new TerrainProtoObject()
            {
                Name = name,
                Prefab = go,
            };
            _terrainProtoObjectData[name] = tpo;
            return tpo.Prefab;
        }
    }



    private void SetupLoaders()
    {
        _staticLoaders = new Dictionary<long, BaseObjectLoader>();
        _staticLoaders[MapConstants.TreeObjectOffset / MapConstants.MapObjectOffsetMult] = new TreeObjectLoader(_gs);
        _staticLoaders[MapConstants.RockObjectOffset / MapConstants.MapObjectOffsetMult] = new RockObjectLoader(_gs);
        _staticLoaders[MapConstants.FenceObjectOffset / MapConstants.MapObjectOffsetMult] = new FenceObjectLoader(_gs);
        _staticLoaders[MapConstants.BridgeObjectOffset / MapConstants.MapObjectOffsetMult] = new BridgeObjectLoader(_gs);
        _staticLoaders[MapConstants.ClutterObjectOffset / MapConstants.MapObjectOffsetMult] = new ClutterObjectLoader(_gs);
        _staticLoaders[MapConstants.WaterObjectOffset / MapConstants.MapObjectOffsetMult] = new WaterObjectLoader(_gs);


    }

    void TerrainUpdate(CancellationToken token)
    {
        UpdatePatchVisibility(_gs, token);
    }

    private float _baseVisibilityRadius = MapConstants.TerrainBlockVisibilityRadius;
    private float _currVisibilityRadus = MapConstants.TerrainBlockVisibilityRadius;
    public float GetVisbilityRadius()
    {
        float rad = _currVisibilityRadus;
        return rad;
    }

    public void SetVisibilityRadiusScale(float scale)
    {
        scale = MathUtils.Clamp(0.1f, scale, 1);
        _currVisibilityRadus = _baseVisibilityRadius * scale;
        if (_currVisibilityRadus < 1.4f)
        {
            _currVisibilityRadus = 1.0f;
        }
    }



    protected bool ListContainsCell(List<MyPoint> list, int gx, int gy)
    {
        if (list == null)
        {
            return false;
        }

        return (list.FirstOrDefault(x => x.X == gx && x.Y == gy) != null);
    }

    protected void AddListCell(List<MyPoint> list, int gx, int gy)
    {
        if (list == null)
        {
            return;
        }

        {
            list.Add(new MyPoint(gx, gy));
        }
    }

    protected void RemoveListCell(List<MyPoint> list, int gx, int gy)
    {
        if (list == null)
        {
            return;
        }

        MyPoint item = list.FirstOrDefault(x => x.X == gx && x.Y == gy);
        if (item != null)
        {
            list.Remove(item);
        }
    }


    public bool AddingPatches (UnityGameState gs)
    {
        if (gs.md.addPatchList.Count > 0 || 
            gs.md.loadingPatchList.Count > 0 ||
            gs.md.removePatchList.Count > 0)
        {
            return true;
        }

        return false;
    }


    int XGrid = 1;
    int YGrid = 1;
    int loadUnloadCheckTicks = 0;
    int patchCheckTicks = 0;
    GVector3 playerPos = GVector3.zero;
    float checkRad = 0;
    float loadRad = 0;
    float unloadRad = 0;
    int minx = 0;
    int miny = 0;
    int maxx = 0;
    int maxy = 0;
    float dx = 0;
    float dy = 0;
    float dist = 0;
    int layerIdx = 0;
    TerrainLayer layer = null;
    TerrainPatchData patch = null;
    Terrain terr = null;
    MyPoint firstItem = null;

    LoadMap loadGen = new LoadMap();   

    void UpdatePatchVisibility(UnityGameState gs, CancellationToken token)
    {
        if (UnityAssetService.LoadSpeed == LoadSpeed.Paused)
        {
            return;
        }

        if (PlayerObject.Get() == null)
        {
            return;
        }

        if (gs.md == null || gs.map == null)
        {
            return;
        }

        loadUnloadCheckTicks--;
        patchCheckTicks--;

        if (UnityAssetService.LoadSpeed == LoadSpeed.Fast)
        {
            loadUnloadCheckTicks = 0;
            patchCheckTicks = 0;
        }

        if (loadUnloadCheckTicks <= 0)
        {
            MyPointF ppos = PlayerObject.GetUnit()?.GetPos();
            playerPos = new GVector3(ppos.X, ppos.Y, ppos.Z);

            loadUnloadCheckTicks = MaxLoadUnloadCheckTicks;

            XGrid = (int)((playerPos.x + MapConstants.TerrainPatchSize / 2) / (MapConstants.TerrainPatchSize - 1));
            YGrid = (int)((playerPos.z + MapConstants.TerrainPatchSize / 2) / (MapConstants.TerrainPatchSize - 1));


            loadRad = GetVisbilityRadius() + 0.25f;
            unloadRad = loadRad + 1.0f;
            checkRad = unloadRad + 2.0f;

            if (gs.rand.NextDouble() < 0.2f)
            {
                checkRad = gs.map.BlockCount + 1;
            }


            minx = (int)Math.Max(0, XGrid - checkRad);
            maxx = (int)Math.Min(gs.map.BlockCount - 1, XGrid + checkRad);
            miny = (int)Math.Max(0, YGrid - checkRad);
            maxy = (int)Math.Min(gs.map.BlockCount - 1, YGrid + checkRad);

            for (int x = 0; x < gs.map.BlockCount; x++)
            {
                // Loop over all x in the middle range, or the full row check range.
                if (x < minx || x > maxx)
                {
                    continue;
                }
                // continue;
                for (int y = miny; y <= maxy; y++)
                {
                    // If we are on the full row check and not in the main middle block,
                    // only check that one row.
                    if (y < miny || y > maxy)
                    {
                        continue;
                    }
                    dx = playerPos.x - (x + 0.5f) * (MapConstants.TerrainPatchSize - 1);
                    dy = playerPos.z - (y + 0.5f) * (MapConstants.TerrainPatchSize - 1);

                    dist = (float)Math.Sqrt(dx * dx + dy * dy);

                    // Check if we load the patch if it's near the player's position.
                    if (dist < loadRad * (MapConstants.TerrainPatchSize-1))
                    {
                        patch = gs.md.GetTerrainPatch(gs, x, y);
                        if (patch != null)
                        {
                            terr = patch.terrain as Terrain;
                            if (terr == null)
                            {
                                if (!ListContainsCell(gs.md.addPatchList, x, y) &&
                                    !ListContainsCell(gs.md.loadingPatchList, x, y))
                                {
                                    AddListCell(gs.md.addPatchList, x, y);
                                    RemoveListCell(gs.md.removePatchList, x, y);
                                }
                            }
                        }
                    }
                    // Check if we unload the patch.
                    else if (dist > unloadRad * (MapConstants.TerrainPatchSize - 1))
                    {
                        patch = gs.md.GetTerrainPatch(gs, x, y, false);
                        if (patch != null)
                        {
                            terr = patch.terrain as Terrain;
                            if (terr != null)
                            {
                                if (!ListContainsCell(gs.md.loadingPatchList, x, y))
                                {
                                    RemoveListCell(gs.md.addPatchList, x, y);
                                    if (!ListContainsCell(gs.md.removePatchList, x, y) && terr != null)
                                    {
                                        AddListCell(gs.md.removePatchList, x, y);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        if (patchCheckTicks <= 0)
        {
            patchCheckTicks = MaxPatchLoadTicks;
            if (gs.md.loadingPatchList.Count > 2)
            {
                return;
            }

            // Add patches if there are any to add if it's normal speed or
            // we have nothing to remove in fast remove mode.
            if (gs.md.addPatchList.Count > 0)
            {

                int loadTimes = (UnityAssetService.LoadSpeed == LoadSpeed.Fast ? 100 : 1);

                for (int tt = 0; tt < loadTimes; tt++)
                {
                    if (gs.md.addPatchList.Count < 1)
                    {
                        break;
                    }

                    firstItem = gs.md.addPatchList[0];
                    gs.md.addPatchList.RemoveAt(0);
                    gs.md.loadingPatchList.Add(firstItem);
                    loadGen.LoadOneTerrainPatch(gs, firstItem.X, firstItem.Y, token);
                }
            }
            
            if (gs.md.removePatchList.Count > 0)
            {
                firstItem = gs.md.removePatchList[0];
                gs.md.removePatchList.Remove(firstItem);
                PatchesRemoved++;
                patch = gs.md.GetTerrainPatch(gs, firstItem.X, firstItem.Y);
                if (patch == null)
                {
                    return;
                }
                gs.md.RemoveTerrainPatch(gs, firstItem.X, firstItem.Y);
                terr = patch.terrain as Terrain;

                if (terr != null)
                {                   
                    if (terr.terrainData != null && terr.terrainData.terrainLayers != null)
                    {                       
                        terr.terrainData.treeInstances = new TreeInstance[0];

                        terr.terrainData.treePrototypes = new TreePrototype[0];
                        terr.terrainData.RefreshPrototypes();

                        RemovePatchFromPrototypes(firstItem.X, firstItem.Y);
                        
                        for (layerIdx = 0; layerIdx < terr.terrainData.terrainLayers.Length; layerIdx++)
                        {
                            layer = terr.terrainData.terrainLayers[layerIdx];
                            if (layer.diffuseTexture != null)
                            {
                                string texName = layer.diffuseTexture.name.Replace("_d", "");
                                TerrainTextureData tdata = null;
                                tdata = GetFromTerrainTextureCache(texName);
                                if (tdata != null)
                                {
                                    tdata.InstanceCount--;
                                    if (tdata.InstanceCount <= 0)
                                    {
                                        RemoveFromTerrainTextureCache(texName);
                                        GEntityUtils.Destroy(tdata.TextureContainer);

                                        layer.diffuseTexture = null;
                                        layer.normalMapTexture = null;
                                    }
                                }
                            }
                        }
                    }

                    GEntityUtils.Destroy(terr.entity());
                }
                patch.terrain = null;
                patch.terrainData = null;
                patch.DataBytes = null;
                patch.grassAmounts = null;
                patch.heights = null;
                patch.overrideZoneScales = null;
                patch.subZoneIds = null;
                patch.mainZoneIds = null;
                patch.baseAlphas = null;
                patch.mapObjects = null;
                patch.FullZoneIdList = null;
                patch.MainZoneIdList = null;
                patch.parentObject = null;
            }
        }
	}


    public void ClearPatches(UnityGameState gs)
    { 
        for (int x = 0; x < gs.md.terrainPatches.GetLength(0); x++)
        {
            for (int y = 0; y < gs.md.terrainPatches.GetLength(1); y++)
            {
                if (gs.md.terrainPatches[x, y] != null)
                {
                    Terrain terr = gs.md.terrainPatches[x, y].terrain as Terrain;
                    if (terr != null)
                    {
                        GEntityUtils.Destroy(terr.entity());
                    }
                }
            }
        }
        gs.md.terrainPatches = new TerrainPatchData[MapConstants.MaxTerrainGridSize, MapConstants.MaxTerrainGridSize];
                
    }

    public async UniTask SetupOneTerrainPatch(UnityGameState gs, int gx, int gy, CancellationToken token)
    {
        if (gx < 0 || gy < 0 || gx >= gs.map.BlockCount ||
            gy >= gs.map.BlockCount)
        {
            return;
        }
        TerrainPatchData patch = gs.md.GetTerrainPatch(gs, gx, gy);
        if (patch == null)
        {
            return;
        }

        float heightMapSize = gs.map.GetHwid();
        int patchSize = MapConstants.TerrainPatchSize;


        float[,] patchHeights = new float[patchSize, patchSize];
        for (int x = 0; x < patchSize; x++)
        {
            for (int y = 0; y < patchSize; y++)
            {
                patchHeights[x, y] = MapConstants.StartHeightPercent;
            }
        }


        if (UnityAssetService.LoadSpeed != LoadSpeed.Fast)
        {
            await UniTask.NextFrame( cancellationToken: token);
        }
        int alphaPatchSize = patchSize * MapConstants.AlphaMapsPerTerrainCell;
        float[,,] patchAlphas = new float[alphaPatchSize, alphaPatchSize, MapConstants.MaxTerrainIndex];

        GVector3 offsetPos = new GVector3 (gx * (MapConstants.TerrainPatchSize-1), 0, gy * (MapConstants.TerrainPatchSize-1));
        string terrainName = "Terrain" + gx + "_" + gy;

        GEntity terrObj2 = AssetUtils.LoadResource<GEntity>("Prefabs/TerrainMaterialPlaceholder");
        terrObj2 = Instantiate<GEntity>(terrObj2);
        terrObj2.name = terrainName;    

        terrObj2.transform().localPosition = GVector3.Create(offsetPos);
        Terrain terr2 = terrObj2.GetComponent<Terrain>();
        terr2.terrainData.detailPrototypes = new DetailPrototype[0];
        terr2.terrainData.treePrototypes = new TreePrototype[0];
        terr2.terrainData = GEntity.Instantiate<TerrainData>(terr2.terrainData);
        TerrainCollider coll = GEntityUtils.GetOrAddComponent<TerrainCollider>(gs, terrObj2); 
        coll.terrainData = terr2.terrainData;

  
        patch.terrain = terr2;
        patch.terrainData = terr2.terrainData;

        _zoneGenService.InitTerrainSettings(gs, gx, gy, patchSize, token);

        if (UnityAssetService.LoadSpeed != LoadSpeed.Fast)
        {
            await UniTask.NextFrame( cancellationToken: token);
        }
        terr2.terrainData.SetHeights(0, 0, patchHeights);

        if (UnityAssetService.LoadSpeed != LoadSpeed.Fast)
        {
            await UniTask.NextFrame( cancellationToken: token);
        }


        TerrainLayer[] arr = new TerrainLayer[MapConstants.MaxTerrainIndex];
        for (int s = 0; s < arr.Length; s++)
        {
            arr[s] = MapGenData.CreateTerrainLayer(null);
        }
        terr2.terrainData.terrainLayers = arr;

        if (UnityAssetService.LoadSpeed != LoadSpeed.Fast)
        {
            await UniTask.NextFrame( cancellationToken: token);
        }
        terr2.terrainData.SetAlphamaps(0, 0, patchAlphas);
        terr2.Flush();

        //terr2.bakeLightProbesForTrees = false;       

        float maxHeight = MapConstants.MapHeight;
        if (UnityAssetService.LoadSpeed != LoadSpeed.Fast)
        {
            await UniTask.NextFrame( cancellationToken: token);
        }
        terr2.terrainData.heightmapResolution = patchSize;
        terr2.terrainData.size = GVector3.Create(patchSize-1, maxHeight, patchSize-1);

        terr2.terrainData.alphamapResolution = patchSize * MapConstants.AlphaMapsPerTerrainCell;

        if (UnityAssetService.LoadSpeed != LoadSpeed.Fast)
        {
            await UniTask.NextFrame( cancellationToken: token);
        }
        terr2.Flush();
        if (UnityAssetService.LoadSpeed != LoadSpeed.Fast)
        {
            await UniTask.NextFrame( cancellationToken: token);
        }

    }

    public List<Terrain> GetTerrains(UnityGameState gs)
    {
        List<Terrain> retval = new List<Terrain>();
        if (gs.md.terrainPatches == null)
        {
            return retval;
        }

        for (int x = 0; x < gs.md.terrainPatches.GetLength(0); x++)
        {
            for (int y = 0; y < gs.md.terrainPatches.GetLength(1); y++)
            {
                if (gs.md.terrainPatches[x,y] != null)
                {
                    Terrain terr = gs.md.terrainPatches[x, y].terrain as Terrain;
                    if (terr != null)
                    {
                        retval.Add(terr);
                    }
                }
            }
        }

        return retval;
    }


    public async UniTask ShiftItems(bool outOfPlace, CancellationToken token)
    {
        int newYValue = (outOfPlace ? ShiftYOffset : 0);
        if (newYValue == _lastYShift)
        {
            return;
        }
        _lastYShift = newYValue;
        int maxShiftsAtOnce = 1;
        int currShifts = 0;
        for (int x = 0; x < _gs.md.terrainPatches.GetLength(0); x++)
        {
            for (int y = 0; y < _gs.md.terrainPatches.GetLength(1); y++)
            {
                TerrainPatchData terrainPatch = _gs.md.GetTerrainPatch(_gs, x, y);
                if (terrainPatch == null)
                {
                    continue;
                }
                Terrain terr = terrainPatch.terrain as Terrain;
                if (terr == null || terr.entity() == null)
                {
                    continue;
                }
                GVector3 pos = GVector3.Create(terr.entity().transform().position);
                terr.entity().transform().position = GVector3.Create(pos.x, newYValue, pos.z);

                if (++currShifts % maxShiftsAtOnce == 0)
                {
                    await UniTask.NextFrame( cancellationToken: token);
                }
            }
        }
    }

    public GEntity GetPrototypeParent()
    {
        return _prototypeParent;
    }

    public async UniTask AddPatchObjects(UnityGameState gs, int gx, int gy, CancellationToken token)
    {
        PatchLoadData loadData = new PatchLoadData();
        loadData.gx = gx;
        loadData.gy = gy;
        loadData.StartX = loadData.gx * (MapConstants.TerrainPatchSize - 1);
        loadData.StartY = loadData.gy * (MapConstants.TerrainPatchSize - 1);
        loadData.terrManager = this;

        patch = gs.md.GetTerrainPatch(gs, gx, gy);
        loadData.patch = patch;


        if (patch == null || patch.FullZoneIdList == null || patch.FullZoneIdList.Count < 1)
        {
            return;
        }

        Terrain pterr = loadData.patch.terrain as Terrain;
        if (pterr != null)
        {
            //loadData.protoParent = pterr.entity();
            loadData.protoParent = _prototypeParent;
        }



        loadData.objectProtos = new List<ObjectPrototype>();
        loadData.treeInstances = new List<TreeInstance>();

        List<Zone> currZones = new List<Zone>();

        foreach (long zid in loadData.patch.FullZoneIdList)
        {
            Zone zn = gs.map.Get<Zone>(zid);
            if (zn != null)
            {
                currZones.Add(zn);
            }
        }

        List<ZoneType> zoneTypeCache = new List<ZoneType>();

        if (loadData.patch.mapObjects == null)
        {
            loadData.patch.mapObjects = new uint[MapConstants.TerrainPatchSize, MapConstants.TerrainPatchSize];
        }

        int addTimes = 0;
       
        int currZoneId = -1;
        Zone currZone = null;
        ZoneType currZoneType = null;

        for (int x = 0; x < MapConstants.TerrainPatchSize - 1; x++)
        {
            for (int y = 0; y < MapConstants.TerrainPatchSize - 1; y++)
            {
                if (loadData.patch.mapObjects[x, y] == 0)
                {
                    continue;
                }

                uint worldObjectValue = (uint)loadData.patch.mapObjects[x, y];

                if (loadData.patch.heights == null || loadData.patch.heights[y, x] < MapConstants.StartHeightPercent * 0.75f)
                {
                    continue;
                }

                int zoneId = loadData.patch.mainZoneIds[x, y];


                if (zoneId != currZoneId)
                {
                    currZoneId = zoneId;
                    currZone = currZones.FirstOrDefault(xx => xx.IdKey == zoneId);
                    if (currZone == null)
                    {
                        currZone = gs.map.Get<Zone>(currZoneId);
                        if (currZone == null)
                        {
                            currZoneId = -1;
                            currZoneType = null;
                            continue;
                        }
                        currZones.Add(currZone);
                    }
                    currZoneType = zoneTypeCache.FirstOrDefault(xx => xx.IdKey == currZone.ZoneTypeId);
                    if (currZoneType == null)
                    {
                        currZoneType = gs.data.GetGameData<ZoneTypeSettings>(gs.ch).GetZoneType(currZone.ZoneTypeId);
                        if (currZoneType == null)
                        {
                            currZoneId = -1;
                            currZone = null;
                            continue;
                        }
                        zoneTypeCache.Add(currZoneType);
                    }
                }

                if (currZone == null || currZoneType == null)
                {
                    continue;
                }



                int loaderIndex = (int)(worldObjectValue % (1 << 16)) / MapConstants.MapObjectOffsetMult;

                if (!_staticLoaders.ContainsKey(loaderIndex))
                {
                    continue;
                }
                _staticLoaders[loaderIndex].LoadObject(gs, loadData, worldObjectValue, x, y, currZone, currZoneType, token);

                addTimes++;
                if (addTimes >= LoadObjectCountBeforePause)
                {
                    addTimes = 0;
                    if (UnityAssetService.LoadSpeed != LoadSpeed.Fast)
                    {
                        await UniTask.NextFrame(cancellationToken: token);
                    }
                }
                if (gs.md.terrainPatches[loadData.gx, loadData.gy] == null)
                {
                    return;
                }
            }
        }


        await UniTask.NextFrame( cancellationToken: token);

        // Wait until all protos have been downloaded.
        while (true)
        {
            bool haveAllProtos = true;
            for (int p = 0; p < loadData.objectProtos.Count; p++)
            {
                if (loadData.objectProtos[p].Prototype.prefab == null)
                {
                    haveAllProtos = false;
                    break;
                }
            }

            if (haveAllProtos)
            {
                break;
            }

            await UniTask.NextFrame( cancellationToken: token);

        }

        if (gs.md != null && loadData != null && loadData.patch != null && gs.md.terrainPatches[loadData.gx, loadData.gy] != null)
        {
            TerrainData tdata = loadData.patch.terrainData as TerrainData;
            Terrain terr = loadData.patch.terrain as Terrain;
            if (tdata != null && terr != null)
            {
                TreePrototype[] treeProtos = new TreePrototype[loadData.objectProtos.Count];
                for (int p = 0; p < loadData.objectProtos.Count; p++)
                {
                    treeProtos[p] = loadData.objectProtos[p].Prototype;
                }

                tdata.treePrototypes = treeProtos;
                tdata.RefreshPrototypes();


                await UniTask.NextFrame( cancellationToken: token);

                TreeInstance[] tarray = loadData.treeInstances.ToArray();


                if (tdata != null)
                {
                    tdata.SetTreeInstances(tarray, false);
                    terr.Flush();
                }

                // Disable and enable this AFTER adding the tree instances so that the tree
                // colliders will work!
                TerrainCollider tcol = terr.GetComponent<TerrainCollider>();
                if (tcol != null)
                {
                    tcol.enabled = false;
                    tcol.enabled = true;
                }
            }
        }
    }


    int gridPosX = 0;
    int gridPosY = 0;
    public TerrainPatchData GetPatchFromMapPos(UnityGameState gs, float worldx, float worldy)
    {
        gridPosX = (int)(worldx / (MapConstants.TerrainPatchSize - 1));
        gridPosY = (int)(worldy / (MapConstants.TerrainPatchSize - 1));    
        return GetMapGrid(gs, gridPosX, gridPosY);
    }

    public TerrainPatchData GetMapGrid(UnityGameState gs, int gx, int gy)
    {
        if (gx < 0 || gy < 0 || gx >= MapConstants.MaxTerrainGridSize || gy >= MapConstants.MaxTerrainGridSize)
        {
            return null;
        }
        return gs.md.terrainPatches[gx, gy];
    }





    public void ClearMapObjects(UnityGameState gs)
    {
        if (gs.md.terrainPatches == null)
        {
            return;
        }

        for (int x = 0; x < gs.md.terrainPatches.GetLength(0); x++)
        {
            for (int y = 0; y < gs.md.terrainPatches.GetLength(1); y++)
            {
                TerrainPatchData patch = gs.md.terrainPatches[x, y];
                if (patch == null)
                {
                    continue;
                }

                Terrain terr = patch.terrain as Terrain;
                if (terr == null)
                {
                    continue;
                }

                GEntityUtils.Destroy(terr.entity());
                gs.md.terrainPatches[x, y] = null;

            }
        }
    }

    private int _lastYShift = 0;



}
