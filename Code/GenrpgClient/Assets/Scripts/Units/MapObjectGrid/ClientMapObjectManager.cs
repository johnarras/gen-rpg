
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.MapObjects.Entities;
using System.Collections.Generic;
using UnityEngine;
using Genrpg.Shared.Characters.PlayerData;
using System;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Spawns.Interfaces;
using Genrpg.Shared.MapObjects.Factories;
using System.Threading;
using Genrpg.Shared.MapObjects.Messages;
using System.Threading.Tasks;
using Genrpg.Shared.Client.Core;
using Genrpg.Shared.HelperClasses;
using Genrpg.Shared.Movement.Messages;
using Genrpg.Shared.Units.Constants;
using Genrpg.Shared.Utils.Data;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.Shared.MapServer.Services;
using Genrpg.Shared.Logging.Interfaces;
using Genrpg.Shared.Client.Tokens;
using Assets.Scripts.GameObjects;
using Assets.Scripts.Awaitables;

public interface IClientMapObjectManager : IInitializable, IMapTokenService
{
    bool GetUnit(string id, out Unit unit, bool downloadIfNotExist = true);
    bool GetChar(string id, out Character ch, bool downloadIfNotExist = true);
    bool GetMapObject(string id, out MapObject item, bool downloadIfNotExist = true);
    bool GetGridItem(string id, out ClientMapObjectGridItem gridItem);
    bool GetController(string unitId, out UnitController controller);
    IMapObjectLoader GetMapObjectLoader(long entityTypeId);
    MapObject SpawnObject(IMapSpawn spawn);
    void Reset();
    List<T> GetTypedObjectsNear<T>(float x, float z, float radius) where T : MapObject;
    void RemoveObject(string objId);
    ClientMapObjectGridItem AddObject(MapObject obj, GameObject go);
    GameObject GetFXParent();
    void AddController(ClientMapObjectGridItem gridItem, GameObject go);
    void OnServerAddtoGrid(OnAddToGrid onAddToGrid);
}

public class ClientMapObjectManager : IClientMapObjectManager
{
    private IRealtimeNetworkService _networkService;
    private IPlayerManager _playerManager;
    protected IClientRandom _rand;
    protected IMapProvider _mapProvider;
    private ILogService _logService;
    private ISingletonContainer _singletonContainer;
    protected IClientEntityService _clientEntityService;
    protected IAwaitableService _awaitableService;

    private const string FXParentName = "FXParent";


    private List<UnitController> _controllers = new List<UnitController>();
    private SetupDictionaryContainer<long, IMapObjectFactory> _factories = new SetupDictionaryContainer<long, IMapObjectFactory>();

    private Dictionary<string, MapObject> _idDict = new Dictionary<string, MapObject>();
    private Dictionary<string, ClientMapObjectGridItem> _gridItems = new Dictionary<string, ClientMapObjectGridItem>();


    protected IClientUpdateService _updateService;
    protected List<string> _olderSpawns = new List<string>();
    protected List<string> _recentlyLoadedSpawns = new List<string>();
    List<UnitController> _removeUnitList = new List<UnitController>();
    List<ClientMapObjectGridItem> _removeGridItems = new List<ClientMapObjectGridItem>();
    private SetupDictionaryContainer<long, IMapObjectLoader> _mapObjectLoaders = new SetupDictionaryContainer<long, IMapObjectLoader>();

    protected IClientGameState _gs;

    public GameObject _fxParent;

    public void Reset()
    {
        _controllers = new List<UnitController>();
        _idDict = new Dictionary<string, MapObject>();
        _gridItems = new Dictionary<string, ClientMapObjectGridItem>();
        _olderSpawns = new List<string>();
        _recentlyLoadedSpawns = new List<string>();
        _removeUnitList = new List<UnitController>();
        _clientEntityService.DestroyAllChildren(_fxParent);
    }

    public GameObject GetFXParent()
    {
        return _fxParent;
    }

    private CancellationToken _token;
    public void SetMapToken(CancellationToken token)
    {
        _token = token;
    }

    public CancellationToken GetToken()
    {
        return _token;
    }

    private bool _didAddUpdate = false;
    public async Task Initialize(CancellationToken token)
    {
        Reset();

        _awaitableService.ForgetAwaitable(UpdateRecentlyLoadedSpawns(token));
        if (!_didAddUpdate)
        {
            _updateService.AddTokenUpdate(this, FrameUpdate, UpdateType.Regular, token);
            _didAddUpdate = true;
        }
        if (Application.isPlaying)
        {
            _fxParent = _singletonContainer.GetSingleton("FXParent");
        }

        await Task.CompletedTask;
    }


    public IMapObjectLoader GetMapObjectLoader(long entityTypeId)
    {
        if (_mapObjectLoaders.TryGetValue(entityTypeId, out IMapObjectLoader loader))
        {
            return loader;
        }
        return null;
    }


    public bool GetUnit (string id, out Unit unit, bool downloadIfNotExist = true)
    {
        unit = null;
        if (GetMapObject(id, out MapObject obj, downloadIfNotExist))
        {
            unit = obj as Unit;
        }
        return unit != null;
    }

    public bool GetChar(string id, out Character ch, bool downloadIfNotExist = true)
    {
        ch = null;
        if (GetMapObject(id, out MapObject obj, downloadIfNotExist))
        {
            ch = obj as Character;
        }
        return ch != null;
    }

    public bool GetMapObject(string id, out MapObject item, bool downloadIfNotExist = true)
    {
        item = null;
        if (string.IsNullOrEmpty(id))
        {
            return false;
        }
        if (_idDict.TryGetValue(id, out item))
        {
            return true;
        }

        if (!downloadIfNotExist)
        {
            return false;
        }

        if (_recentlyLoadedSpawns.Contains(id) || _olderSpawns.Contains(id))
        {
            return false;
        }
        _recentlyLoadedSpawns.Add(id);

        GetSpawnedObject getObj = new GetSpawnedObject()
        {
            ObjId = id,
        };
        _networkService.SendMapMessage(getObj);
        return false;
    }

    protected ClientMapObjectGridItem CreateGridItem(MapObject obj, int gx, int gz)
    {
        ClientMapObjectGridItem item = new ClientMapObjectGridItem()
        {
            Obj = obj,
            GX = gx,
            GZ = gz,
        };

        return item;
    }

    protected void FrameUpdate(CancellationToken token)
    {
        _removeUnitList.Clear();
        if (_playerManager.GetPlayerGameObject() == null)
        {        
            return;
        }
        Vector3 playerPos = _playerManager.GetPlayerGameObject().transform.position;
        foreach (UnitController controller in _controllers)
        {
            if (controller != null)
            {
                controller.OnUpdate(token);
                if (controller.ShouldRemoveNow())
                {
                    _removeUnitList.Add(controller);
                }
                else
                {
                    Vector3 pos = controller.transform.position;
                    if (Math.Abs(pos.x - playerPos.x) >= MessageConstants.DefaultGridDistance * 3/2 ||
                        Math.Abs(pos.z - playerPos.z) >= MessageConstants.DefaultGridDistance * 3/2)
                    {
                        _removeUnitList.Add(controller);
                    }
                }
            }
        }

        foreach (UnitController controller in _removeUnitList)
        {
            RemoveObject(controller.GetUnit().Id);
        }
        _removeUnitList.Clear();
    }

    public bool GetGridItem(string id, out ClientMapObjectGridItem gridItem)
    {
        gridItem = null;
        if (_gridItems.ContainsKey(id))
        {
            gridItem = _gridItems[id];
        }
        return gridItem != null;
    }

    public void AddController(ClientMapObjectGridItem gridItem, GameObject go)
    {
        if (!TokenUtils.IsValid(_token))
        {
            return;
        }

        if (gridItem.Controller != null)
        {
            _controllers.Add(gridItem.Controller);
        }
    }

    public ClientMapObjectGridItem AddObject(MapObject obj, GameObject go)
    {
        if (!TokenUtils.IsValid(_token))
        {
            return null;
        }

        ClientMapObjectGridItem item = new ClientMapObjectGridItem()
        {
            Obj = obj,
            GameObj = go,
        };

        if (!_gridItems.ContainsKey(obj.Id))
        {
            _gridItems[obj.Id] = item;
        }
        else
        {
            _gridItems[obj.Id].Obj = obj;
            if (go != null)
            {
                _gridItems[obj.Id].GameObj = go;
            }
        }

        if (!_idDict.ContainsKey(obj.Id))
        {
            _idDict[obj.Id] = obj;
        }
        return item;
    }

    public void RemoveObject(string objId)
    {
        if (!GetGridItem(objId, out ClientMapObjectGridItem gridItem))
        {
            return;
        }

        if (gridItem.Controller != null)
        {
            _controllers.Remove(gridItem.Controller);
        }

        gridItem.Obj?.SetDeleted(true);
        _idDict.Remove(objId);
        _gridItems.Remove(objId);

        if (gridItem.GameObj != null)
        {
            _updateService.AddDelayedUpdate(this, (_token) => { FadeOutObject(gridItem.GameObj); }, 1.5f, _token);
        }

        return;
    }

    protected void FadeOutObject(GameObject go)
    {
        _clientEntityService.Destroy(go);
    }

    public bool GetController (string unitId, out UnitController controller)
    {
        if (!GetGridItem(unitId, out ClientMapObjectGridItem gridItem))
        {
            controller = null;
            return false;
        }

        controller = gridItem.Controller;

        return controller != null;
    }

    public virtual MapObject SpawnObject(IMapSpawn spawn)
    {
        if (GetMapObject(spawn.ObjId, out MapObject currObj))
        {
            return currObj;
        }
        
        if (!_factories.TryGetValue(spawn.EntityTypeId, out IMapObjectFactory fact))
        {
            return null;
        }

        MapObject obj = fact.Create(_rand, spawn);

        return obj;
    }

    protected async Awaitable UpdateRecentlyLoadedSpawns(CancellationToken token)
    {
        while (true)
        {
            await Awaitable.WaitForSecondsAsync(2.0f, cancellationToken: token);
            _olderSpawns = _recentlyLoadedSpawns;
            _recentlyLoadedSpawns = new List<string>();
        }
    }

    public List<T> GetTypedObjectsNear<T>(float x, float z, float radius) where T : MapObject
    {
        List<T> retval = new List<T>();
        foreach (ClientMapObjectGridItem gridItem in _gridItems.Values)
        {
            if (gridItem.Obj is T t)
            {
                float dx = t.X - x;
                float dz = t.Z - z;
                if (Math.Sqrt(dx*dx+dz*dz) <= radius)
                {
                    retval.Add(t);
                }
            }
        }
        return retval;
    }

    public void OnServerAddtoGrid(OnAddToGrid onAddToGrid)
    {
        if (onAddToGrid.UserId != _gs.ch.Id)
        {
            return;
        }

        int minX = onAddToGrid.GridX - UnitConstants.PlayerObjectVisRadius;
        int maxX = onAddToGrid.GridX + UnitConstants.PlayerObjectVisRadius; 
        int minZ = onAddToGrid.GridZ - UnitConstants.PlayerObjectVisRadius;
        int maxZ = onAddToGrid.GridZ + UnitConstants.PlayerObjectVisRadius;

        int gridSize = MapUtils.GetMapObjectGridSize(_mapProvider.GetMap());

        _removeGridItems.Clear();
        foreach (ClientMapObjectGridItem gridItem in _gridItems.Values)
        {
            PointXZ gridPos = MapUtils.GetGridCoordinates(gridItem.Obj.X, gridItem.Obj.Z, gridSize);

            if (gridPos.X < minX || gridPos.X > maxX || gridPos.Z < minZ || gridPos.Z > maxZ)
            {
                _removeGridItems.Add(gridItem);
            }
        }

        foreach (ClientMapObjectGridItem removeItem in _removeGridItems)
        {
            RemoveObject(removeItem.Obj.Id);
        }


        _removeGridItems.Clear();
    }
}
