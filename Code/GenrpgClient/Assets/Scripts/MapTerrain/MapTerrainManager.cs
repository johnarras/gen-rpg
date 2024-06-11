using System;
using System.Linq;
using System.Collections.Generic;
using GEntity = UnityEngine.GameObject;
using Genrpg.Shared.Core.Entities;

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
using Genrpg.Shared.MapServer.Entities;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.MapServer.Services;

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

    public int MapX(int x)
    {
        return x + StartX;
    }

    public int MapY(int y)
    {
        return y + StartY;
    }


}

public delegate void AfterObjectLoad(GEntity go, DownloadObjectData data, CancellationToken token);

public class DownloadObjectData
{
    public IUnityGameState ugs;
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


public class MapTerrainManager : BaseBehaviour, IMapTerrainManager
{

    private const int MaxLoadUnloadCheckTicks = 13;
    private const int MaxPatchLoadTicks = 23;

    private const string PrototypeParent = "PrototypeParent";

    private const int LoadObjectCountBeforePause = 20;

    // Used to move world objects out of the way when we enter a dungeon.

    private Dictionary<long, BaseObjectLoader> _staticLoaders = new Dictionary<long, BaseObjectLoader>();

    private Dictionary<string, TerrainTextureData> _terrainTextureCache = new Dictionary<string, TerrainTextureData>();

    private GEntity _prototypeParent = null;
    private GEntity _terrainTextureParent = null;

    protected IZoneGenService _zoneGenService;
    private ITerrainPatchLoader _patchLoader;
    private IPlayerManager _playerManager;
    protected IMapProvider _mapProvider;
    protected IMapGenData _md;


    private List<MyPoint> _addPatchList = new List<MyPoint>();
    private List<MyPoint> _removePatchList = new List<MyPoint>();
    private List<MyPoint> _loadingPatchList = new List<MyPoint>();

    public TerrainPatchData[,] _terrainPatches = new TerrainPatchData[MapConstants.MaxTerrainGridSize, MapConstants.MaxTerrainGridSize];

    private Dictionary<string, TerrainProtoObject> _terrainProtoObjectData = new Dictionary<string, TerrainProtoObject>();

    public async Task Initialize(IGameState gs, CancellationToken token)
    {
        AddTokenUpdate(TerrainUpdate, UpdateType.Regular);
        _prototypeParent = GEntityUtils.FindSingleton(PrototypeParent, true);
        _terrainTextureParent = GEntityUtils.FindSingleton(MapConstants.TerrainTextureRoot, true);
        SetupLoaders();
        
    }

    private bool _fastLoading = false;
    public void SetFastLoading()
    {
        _fastLoading = true;
    }


    private long _patchesAdded = 0;
    private long _patchesRemoved = 0;

    public long GetPatchesAdded()
    {
        return _patchesAdded;
    }

    public long GetPatchesRemoved() 
    {
        return _patchesRemoved;
    }

    public void IncrementPatchesAdded()
    {
        _patchesAdded++;
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

    public bool IsLoadingPatches()
    {
        return _addPatchList?.Count > 0 ||
            _loadingPatchList?.Count > 0;
    }

    public void RemoveLoadingPatches(int gx, int gy)
    {
        if (_loadingPatchList != null)
        {
            _loadingPatchList = _loadingPatchList.Where(x => x.X != gx || x.Y != gy).ToList();
        }
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

    public void Clear()
    {
        _terrainProtoObjectData = new Dictionary<string, TerrainProtoObject>();
        GEntityUtils.DestroyAllChildren(_prototypeParent);
        GEntityUtils.DestroyAllChildren(_terrainTextureParent);
        _terrainTextureCache.Clear();

        _loadingPatchList = new List<MyPoint>();
        _addPatchList = new List<MyPoint>();
        _removePatchList = new List<MyPoint>();
        ClearMapObjects();
        ClearPatches();
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

    public BaseObjectLoader GetLoader(long mapObjectOffset)
    {
        return _staticLoaders[mapObjectOffset / MapConstants.MapObjectOffsetMult];
    }


    private void SetupLoaders()
    {
        _staticLoaders = new Dictionary<long, BaseObjectLoader>();
        _staticLoaders[MapConstants.TreeObjectOffset / MapConstants.MapObjectOffsetMult] = new TreeObjectLoader();
        _staticLoaders[MapConstants.RockObjectOffset / MapConstants.MapObjectOffsetMult] = new RockObjectLoader();
        _staticLoaders[MapConstants.FenceObjectOffset / MapConstants.MapObjectOffsetMult] = new FenceObjectLoader();
        _staticLoaders[MapConstants.BridgeObjectOffset / MapConstants.MapObjectOffsetMult] = new BridgeObjectLoader();
        _staticLoaders[MapConstants.ClutterObjectOffset / MapConstants.MapObjectOffsetMult] = new ClutterObjectLoader();
        _staticLoaders[MapConstants.WaterObjectOffset / MapConstants.MapObjectOffsetMult] = new WaterObjectLoader();

        foreach (BaseObjectLoader loader in _staticLoaders.Values)
        {
            _gs.loc.Resolve(loader);
        }
    }

    void TerrainUpdate(CancellationToken token)
    {
        UpdatePatchVisibility(token);
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


    public bool AddingPatches ()
    {
        if (_addPatchList.Count > 0 || 
            _loadingPatchList.Count > 0 ||
            _removePatchList.Count > 0)
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

    void UpdatePatchVisibility(CancellationToken token)
    {
        if (!_playerManager.Exists())
        {
            return;
        }

        if (_mapProvider.GetMap() == null)
        {
            return;
        }

        loadUnloadCheckTicks--;
        patchCheckTicks--;

        if (_fastLoading)
        {
            loadUnloadCheckTicks = 0;
            patchCheckTicks = 0;
        }

        if (loadUnloadCheckTicks <= 0)
        {
            if (_playerManager.TryGetUnit(out Unit unit))
            {

                MyPointF ppos = unit.GetPos();
                playerPos = new GVector3(ppos.X, ppos.Y, ppos.Z);

                loadUnloadCheckTicks = MaxLoadUnloadCheckTicks;

                XGrid = (int)((playerPos.x + MapConstants.TerrainPatchSize / 2) / (MapConstants.TerrainPatchSize - 1));
                YGrid = (int)((playerPos.z + MapConstants.TerrainPatchSize / 2) / (MapConstants.TerrainPatchSize - 1));


                loadRad = GetVisbilityRadius() + 0.25f;
                unloadRad = loadRad + 1.0f;
                checkRad = unloadRad + 2.0f;

                if (_rand.NextDouble() < 0.2f)
                {
                    checkRad = _mapProvider.GetMap().BlockCount + 1;
                }

                minx = (int)Math.Max(0, XGrid - checkRad);
                maxx = (int)Math.Min(_mapProvider.GetMap().BlockCount - 1, XGrid + checkRad);
                miny = (int)Math.Max(0, YGrid - checkRad);
                maxy = (int)Math.Min(_mapProvider.GetMap().BlockCount - 1, YGrid + checkRad);

                for (int x = 0; x < _mapProvider.GetMap().BlockCount; x++)
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
                        if (dist < loadRad * (MapConstants.TerrainPatchSize - 1))
                        {
                            patch = GetTerrainPatch(x, y);
                            if (patch != null)
                            {
                                terr = patch.terrain as Terrain;
                                if (terr == null)
                                {
                                    if (!ListContainsCell(_addPatchList, x, y) &&
                                        !ListContainsCell(_loadingPatchList, x, y))
                                    {
                                        AddListCell(_addPatchList, x, y);
                                        RemoveListCell(_removePatchList, x, y);
                                    }
                                }
                            }
                        }
                        // Check if we unload the patch.
                        else if (dist > unloadRad * (MapConstants.TerrainPatchSize - 1))
                        {
                            patch = GetTerrainPatch(x, y, false);
                            if (patch != null)
                            {
                                terr = patch.terrain as Terrain;
                                if (terr != null)
                                {
                                    if (!ListContainsCell(_loadingPatchList, x, y))
                                    {
                                        RemoveListCell(_addPatchList, x, y);
                                        if (!ListContainsCell(_removePatchList, x, y) && terr != null)
                                        {
                                            AddListCell(_removePatchList, x, y);
                                        }
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
            if (_loadingPatchList.Count > 2)
            {
                return;
            }

            // Add patches if there are any to add if it's normal speed or
            // we have nothing to remove in fast remove mode.
            if (_addPatchList.Count > 0)
            {

                int loadTimes = (_fastLoading ? 100 : 1);

                for (int tt = 0; tt < loadTimes; tt++)
                {
                    if (_addPatchList.Count < 1)
                    {
                        break;
                    }

                    firstItem = _addPatchList[0];
                    _addPatchList.RemoveAt(0);
                    _loadingPatchList.Add(firstItem);
                    
                    _patchLoader.LoadOneTerrainPatch(firstItem.X, firstItem.Y, _fastLoading, token);
                }
            }
            
            if (_removePatchList.Count > 0)
            {
                firstItem = _removePatchList[0];
                _removePatchList.Remove(firstItem);
                _patchesRemoved++;
                patch = GetTerrainPatch(firstItem.X, firstItem.Y);
                if (patch == null)
                {
                    return;
                }
                RemoveTerrainPatch(firstItem.X, firstItem.Y);
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

        if (_fastLoading && _addPatchList.Count < 1 && _loadingPatchList.Count < 1)
        {
            _fastLoading = false;
        }
	}


    public void ClearPatches()
    {      
        if (_terrainPatches == null)
        {
            return;
        }

        for (int x = 0; x < _terrainPatches.GetLength(0); x++)
        {
            for (int y = 0; y < _terrainPatches.GetLength(1); y++)
            {
                if (_terrainPatches[x, y] != null)
                {
                    Terrain terr = _terrainPatches[x, y].terrain as Terrain;
                    if (terr != null)
                    {
                        GEntityUtils.Destroy(terr.entity());
                    }
                }
            }
        }
        _terrainPatches = new TerrainPatchData[MapConstants.MaxTerrainGridSize, MapConstants.MaxTerrainGridSize];
                
    }

    public async Awaitable SetupOneTerrainPatch(int gx, int gy, CancellationToken token)
    {
        if (gx < 0 || gy < 0 || gx >= _mapProvider.GetMap().BlockCount ||
            gy >= _mapProvider.GetMap().BlockCount)
        {
            return;
        }
        TerrainPatchData patch = GetTerrainPatch(gx, gy);
        if (patch == null)
        {
            return;
        }

        float heightMapSize = _mapProvider.GetMap().GetHwid();
        int patchSize = MapConstants.TerrainPatchSize;


        float[,] patchHeights = new float[patchSize, patchSize];
        for (int x = 0; x < patchSize; x++)
        {
            for (int y = 0; y < patchSize; y++)
            {
                patchHeights[x, y] = MapConstants.StartHeightPercent;
            }
        }


        if (!_fastLoading)
        {
            await Awaitable.NextFrameAsync(cancellationToken: token);
        }
        int alphaPatchSize = patchSize * MapConstants.AlphaMapsPerTerrainCell;
        float[,,] patchAlphas = new float[alphaPatchSize, alphaPatchSize, MapConstants.MaxTerrainIndex];

        GVector3 offsetPos = new GVector3 (gx * (MapConstants.TerrainPatchSize-1), 0, gy * (MapConstants.TerrainPatchSize-1));
        string terrainName = "Terrain" + gx + "_" + gy;


        GEntity terrObj2 = await _assetService.LoadAssetAsync(AssetCategoryNames.Prefabs, "TerrainMaterialPlaceholder", null, token);
        terrObj2.name = terrainName;    

        terrObj2.transform().localPosition = GVector3.Create(offsetPos);
        Terrain terr2 = terrObj2.GetComponent<Terrain>();
        terr2.terrainData.detailPrototypes = new DetailPrototype[0];
        terr2.terrainData.treePrototypes = new TreePrototype[0];
        terr2.terrainData = GEntity.Instantiate<TerrainData>(terr2.terrainData);
        TerrainCollider coll = GEntityUtils.GetOrAddComponent<TerrainCollider>(_gs, terrObj2); 
        coll.terrainData = terr2.terrainData;

  
        patch.terrain = terr2;
        patch.terrainData = terr2.terrainData;

        _zoneGenService.InitTerrainSettings(gx, gy, patchSize, token);

        if (!_fastLoading)
        {
            await Awaitable.NextFrameAsync(cancellationToken: token);
        }
        terr2.terrainData.SetHeights(0, 0, patchHeights);

        if (!_fastLoading)
        {
            await Awaitable.NextFrameAsync(cancellationToken: token);
        }


        TerrainLayer[] arr = new TerrainLayer[MapConstants.MaxTerrainIndex];
        for (int s = 0; s < arr.Length; s++)
        {
            arr[s] = CreateTerrainLayer(null);
        }
        terr2.terrainData.terrainLayers = arr;

        if (!_fastLoading)
        {
            await Awaitable.NextFrameAsync(cancellationToken: token);
        }
        terr2.terrainData.SetAlphamaps(0, 0, patchAlphas);
        terr2.Flush();

        //terr2.bakeLightProbesForTrees = false;       

        float maxHeight = MapConstants.MapHeight;
        if (!_fastLoading)
        {
            await Awaitable.NextFrameAsync(cancellationToken: token);
        }
        terr2.terrainData.heightmapResolution = patchSize;
        terr2.terrainData.size = GVector3.Create(patchSize-1, maxHeight, patchSize-1);

        terr2.terrainData.alphamapResolution = patchSize * MapConstants.AlphaMapsPerTerrainCell;

        if (!_fastLoading)
        {
            await Awaitable.NextFrameAsync(cancellationToken: token);
        }
        terr2.Flush();
        if (!_fastLoading)
        {
            await Awaitable.NextFrameAsync(cancellationToken: token);
        }

    }

    public List<Terrain> GetTerrains()
    {
        List<Terrain> retval = new List<Terrain>();
        if (_terrainPatches == null)
        {
            return retval;
        }

        for (int x = 0; x < _terrainPatches.GetLength(0); x++)
        {
            for (int y = 0; y < _terrainPatches.GetLength(1); y++)
            {
                if (_terrainPatches[x,y] != null)
                {
                    Terrain terr = _terrainPatches[x, y].terrain as Terrain;
                    if (terr != null)
                    {
                        retval.Add(terr);
                    }
                }
            }
        }

        return retval;
    }


    public GEntity GetPrototypeParent()
    {
        return _prototypeParent;
    }

    public async Awaitable AddPatchObjects(int gx, int gy, CancellationToken token)
    {
        PatchLoadData loadData = new PatchLoadData();
        loadData.gx = gx;
        loadData.gy = gy;
        loadData.StartX = loadData.gx * (MapConstants.TerrainPatchSize - 1);
        loadData.StartY = loadData.gy * (MapConstants.TerrainPatchSize - 1);
        loadData.terrManager = this;

        patch = GetTerrainPatch(gx, gy);
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
            Zone zn = _mapProvider.GetMap().Get<Zone>(zid);
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
                        currZone = _mapProvider.GetMap().Get<Zone>(currZoneId);
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
                        currZoneType = _gameData.Get<ZoneTypeSettings>(_gs.ch).Get(currZone.ZoneTypeId);
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
                _staticLoaders[loaderIndex].LoadObject(loadData, worldObjectValue, x, y, currZone, currZoneType, token);

                addTimes++;
                if (addTimes >= LoadObjectCountBeforePause)
                {
                    addTimes = 0;
                    if (!_fastLoading)
                    {
                        await Awaitable.NextFrameAsync(cancellationToken: token);
                    }
                }
                if (_terrainPatches[loadData.gx, loadData.gy] == null)
                {
                    return;
                }
            }
        }


        await Awaitable.NextFrameAsync(cancellationToken: token);

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

            await Awaitable.NextFrameAsync(cancellationToken: token);

        }

        if (_md != null && loadData != null && loadData.patch != null && _terrainPatches[loadData.gx, loadData.gy] != null)
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


                await Awaitable.NextFrameAsync(cancellationToken: token);

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
    public TerrainPatchData GetPatchFromMapPos(float worldx, float worldy)
    {
        gridPosX = (int)(worldx / (MapConstants.TerrainPatchSize - 1));
        gridPosY = (int)(worldy / (MapConstants.TerrainPatchSize - 1));    
        return GetMapGrid(gridPosX, gridPosY);
    }

    public TerrainPatchData GetMapGrid(int gx, int gy)
    {
        if (gx < 0 || gy < 0 || gx >= MapConstants.MaxTerrainGridSize || gy >= MapConstants.MaxTerrainGridSize)
        {
            return null;
        }
        return _terrainPatches[gx, gy];
    }





    public void ClearMapObjects()
    {
        if (_terrainPatches == null)
        {
            return;
        }

        for (int x = 0; x < _terrainPatches.GetLength(0); x++)
        {
            for (int y = 0; y < _terrainPatches.GetLength(1); y++)
            {
                TerrainPatchData patch = _terrainPatches[x, y];
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
                _terrainPatches[x, y] = null;

            }
        }
    }


    const float NormalEdgePct = 0.001f;
    int divSize = MapConstants.TerrainPatchSize - 1;
    int normalXGrid = 0;
    int normalYGrid = 0;
    TerrainPatchData normalPatch = null;
    public GVector3 GetInterpolatedNormal(Map map, float x, float y)
    {
        if (!_md.HaveSetHeights)
        {
            throw new Exception("You must set the terrain heights before interpolating height.");
        }

        float startx = x;
        float starty = y;

        normalXGrid = (int)(x / (divSize));
        normalYGrid = (int)(y / (divSize));

        x -= normalXGrid * (divSize);
        y -= normalYGrid * (divSize);

        if (normalXGrid < 0 || normalYGrid < 0 || normalXGrid >= map.BlockCount || normalYGrid >= map.BlockCount ||
            _terrainPatches == null)
        {
            return GVector3.up;
        }

        normalPatch = _terrainPatches[normalXGrid, normalYGrid];

        if (normalPatch == null)
        {
            return GVector3.up;
        }

        normalTerrainData = normalPatch.terrainData as TerrainData;

        if (normalTerrainData == null)
        {
            return GVector3.up;
        }
        x = MathUtils.Clamp(NormalEdgePct, x / (divSize), 1 - NormalEdgePct);
        y = MathUtils.Clamp(NormalEdgePct, y / (divSize), 1 - NormalEdgePct);
        GVector3 norm = GVector3.Create(normalTerrainData.GetInterpolatedNormal(x, y));

        return norm;
    }
    public float GetSteepness(float xpos, float ypos)
    {
        if (!_md.HaveSetHeights)
        {
            return 0.0f;
            //throw new Exception("Tried to calc steepness before setting heights!");
        }


        int xgrid = (int)(xpos / (MapConstants.TerrainPatchSize - 1));
        int ygrid = (int)(ypos / (MapConstants.TerrainPatchSize - 1));

        float localx = xpos - xgrid * (MapConstants.TerrainPatchSize - 1);
        float localy = ypos - ygrid * (MapConstants.TerrainPatchSize - 1);

        if (localx < 0.1f)
        {
            localx += 0.1f;
        }

        if (localy < 0.1f)
        {
            localy += 0.1f;
        }

        TerrainData tdata2 = GetTerrainData(xgrid, ygrid);

        if (tdata2 == null)
        {
            return 0.0f;
        }

        float endDelta = 0.2f;
        localx = MathUtils.Clamp(endDelta, localx, MapConstants.TerrainPatchSize - 1 - endDelta);
        localy = MathUtils.Clamp(endDelta, localy, MapConstants.TerrainPatchSize - 1 - endDelta);

        return tdata2.GetSteepness((localx + 0.0f) / (MapConstants.TerrainPatchSize - 1), (localy + 0.0f) / (MapConstants.TerrainPatchSize - 1));
    }

    int interpXGrid = 0;
    int interpYGrid = 0;
    float interpLocalX = 0;
    float interpLocalY = 0;
    TerrainData normalTerrainData = null;
    public float GetInterpolatedHeight(float xpos, float ypos)
    {
        if (!_md.HaveSetHeights)
        {
            return 0.0f;
        }
        interpXGrid = (int)(xpos / (MapConstants.TerrainPatchSize - 1));
        interpYGrid = (int)(ypos / (MapConstants.TerrainPatchSize - 1));

        interpLocalX = xpos - interpXGrid * (MapConstants.TerrainPatchSize - 1);
        interpLocalY = ypos - interpYGrid * (MapConstants.TerrainPatchSize - 1);

        if (interpLocalX < 0.1f)
        {
            interpLocalX += 0.1f;
        }

        if (interpLocalY < 0.1f)
        {
            interpLocalY += 0.1f;
        }

        normalTerrainData = GetTerrainData(interpXGrid, interpYGrid);

        if (normalTerrainData == null)
        {
            return 0.0f;
        }


        interpLocalX = MathUtils.Clamp(0.1f, interpLocalX, MapConstants.TerrainPatchSize - 1.1f);
        interpLocalY = MathUtils.Clamp(0.1f, interpLocalY, MapConstants.TerrainPatchSize - 1.1f);

        return normalTerrainData.GetInterpolatedHeight((interpLocalX + 0.0f) / MapConstants.TerrainPatchSize, (interpLocalY + 0.0f) / MapConstants.TerrainPatchSize);
    }

    int sampleXGrid = 0;
    int sampleYGrid = 0;
    Terrain sampleTerrain = null;
    public float SampleHeight(float x, float z)
    {
        if (!_md.HaveSetHeights)
        {
            return 0.0f;
        }


        sampleXGrid = (int)(x / (MapConstants.TerrainPatchSize - 1));
        sampleYGrid = (int)(z / (MapConstants.TerrainPatchSize - 1));


        sampleTerrain = GetTerrain(sampleXGrid, sampleYGrid);
        if (sampleTerrain == null)
        {
            return 0.0f;
        }



        return sampleTerrain.SampleHeight(GVector3.Create(x, MapConstants.MapHeight, z));
    }


    TerrainPatchData getDataPatch = null;
    public TerrainData GetTerrainData(int gx, int gy)
    {
        getDataPatch = GetTerrainPatch(gx, gy);
        if (getDataPatch == null)
        {
            return null;
        }
        return getDataPatch.terrainData as TerrainData;

    }


    public void SetAllTerrainNeighbors()
    {
        for (int gx = 0; gx < _mapProvider.GetMap().BlockCount; gx++)
        {
            for (int gy = 0; gy < _mapProvider.GetMap().BlockCount; gy++)
            {
                SetOneTerrainNeighbors(gx, gy);
            }
        }
    }


    TerrainPatchData getTerrainPatch = null;
    public Terrain GetTerrain(int gx, int gy)
    {
        getTerrainPatch = GetTerrainPatch(gx, gy);
        if (getTerrainPatch == null)
        {
            return null;
        }
        return getTerrainPatch.terrain as Terrain;
    }

    public void SetOneTerrainNeighbors(int gx, int gy)
    {
        if (gx < 0 || gy < 0)
        {
            return;
        }

        Terrain mid = GetTerrain(gx, gy);
        if (mid == null)
        {
            return;
        }
        Terrain top = GetTerrain(gx, gy + 1);
        Terrain bottom = GetTerrain(gx, gy - 1);
        Terrain left = GetTerrain(gx - 1, gy);
        Terrain right = GetTerrain(gx + 1, gy);
        mid.SetNeighbors(left, top, right, bottom);
    }

    public int GetHeightmapSize()
    {
        if (_mapProvider.GetMap() == null || _mapProvider.GetMap().BlockCount < 4)
        {
            return MapConstants.DefaultHeightmapSize;
        }
        return _mapProvider.GetMap().GetMapSize();
    }

    public Texture2D[] _basicTerrainTextures = null;

    public Texture2D GetBasicTerrainTexture(int index)
    {
        if (_basicTerrainTextures == null)
        {
            Color[] colors = new Color[] { GColor.green * 0.6f, new Color(0.6f, 0.3f, 0), GColor.white * 0.4f, GColor.white * 0.8f };
            _basicTerrainTextures = new Texture2D[MapConstants.MaxTerrainIndex];
            for (int c = 0; c < colors.Length && c < MapConstants.MaxTerrainIndex; c++)
            {

                Texture2D tex = new Texture2D(4, 4, TextureFormat.ARGB32, false, true);
                Color32[] texColors = tex.GetPixels32();
                for (int i = 0; i < texColors.Length; i++)
                {
                    texColors[i] = colors[c];
                }
                tex.SetPixels32(texColors);
                _basicTerrainTextures[c] = tex;
            }
        }

        if (index < 0 || index >= _basicTerrainTextures.Length)
        {
            return new Texture2D(2, 2);
        }
        return _basicTerrainTextures[index];
    }

    public TerrainLayer CreateTerrainLayer(Texture2D diffuse, Texture2D normal = null)
    {
        TerrainLayer tl = new TerrainLayer();
        if (diffuse == null)
        {
            diffuse = new Texture2D(2, 2);
        }

        tl.diffuseTexture = diffuse;
        tl.normalMapTexture = normal;
        SetTerrainLayerData(tl);
        return tl;
    }

    public void SetTerrainLayerData(TerrainLayer tl)
    {
        if (tl == null)
        {
            return;
        }
        tl.normalScale = 1.0f;
        tl.metallic = 0.00f; // Set to 0 if using Standard terrain shader.
        tl.smoothness = 0.00f;
        tl.specular = (Color.gray * 0.00f);
        tl.tileOffset = new Vector2(MapConstants.TerrainLayerOffset, MapConstants.TerrainLayerOffset);
        tl.tileSize = new Vector2(MapConstants.TerrainLayerTileSize, MapConstants.TerrainLayerTileSize);
        tl.diffuseRemapMax = Vector4.zero;
        tl.diffuseRemapMin = Vector4.zero;
    }

    public TerrainPatchData GetTerrainPatch(int gx, int gy, bool createIfNotThere = true)
    {
        if (gx < 0 || gy < 0 || 
            _terrainPatches == null ||
            gx >= MapConstants.MaxTerrainGridSize ||
            gy >= MapConstants.MaxTerrainGridSize)
        {
            return null;
        }
        if (_terrainPatches[gx, gy] == null)
        {
            SetTerrainPatchAtGridLocation(gx, gy, _mapProvider.GetMap(), null);
        }
        return _terrainPatches[gx, gy];
    }

    public void RemoveTerrainPatch(int gx, int gy)
    {
        if (gx < 0 || gy < 0 || _terrainPatches == null ||
            gx >= MapConstants.MaxTerrainGridSize || gy >= MapConstants.MaxTerrainGridSize)
        {
            return;
        }
        _terrainPatches[gx, gy] = null;
    }

    public void SetTerrainPatchAtGridLocation(int xgrid, int ygrid, Map map, TerrainPatchData data)
    {
        if (xgrid < 0 || ygrid < 0 || xgrid >= MapConstants.MaxTerrainGridSize || ygrid >= MapConstants.MaxTerrainGridSize ||
           map == null)
        {
            return;
        }

        TerrainPatchData oldPatch = _terrainPatches[xgrid, ygrid];

        if (data == null)
        {
            data = new TerrainPatchData();
        }

        data.MapId = map.Id;
        data.MapVersion = map.MapVersion;
        data.X = xgrid;
        data.Y = ygrid;
        if (map != null)
        {
            data.MapId = map.Id;
        }
        _terrainPatches[xgrid, ygrid] = data;
        if (oldPatch != null)
        {
            data.terrain = oldPatch.terrain;
            data.terrainData = oldPatch.terrainData;
        }
    }

}
