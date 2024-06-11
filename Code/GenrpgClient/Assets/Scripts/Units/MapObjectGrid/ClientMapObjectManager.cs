
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.MapObjects.Entities;
using System.Collections.Generic;
using GEntity = UnityEngine.GameObject;
using Genrpg.Shared.Characters.PlayerData;
using System;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Spawns.Interfaces;
using Genrpg.Shared.Utils;
using Genrpg.Shared.MapObjects.Factories;
using Assets.Scripts.Tokens;
using System.Threading;
using Genrpg.Shared.MapObjects.Messages;
using System.Threading.Tasks;
using UnityEngine;
using Assets.Scripts.ProcGen.RandomNumbers;

public interface IClientMapObjectManager : IInitializable, IMapTokenService
{
    bool GetUnit(string id, out Unit unit, bool downloadIfNotExist = true);
    bool GetChar(string id, out Character ch, bool downloadIfNotExist = true);
    bool GetObject(string id, out MapObject item, bool downloadIfNotExist = true);
    bool GetGridItem(string id, out ClientMapObjectGridItem gridItem);
    bool GetController(string unitId, out UnitController controller);
    IMapObjectLoader GetMapObjectLoader(long entityTypeId);
    MapObject SpawnObject(IMapSpawn spawn);
    void Reset();
    List<T> GetTypedObjectsNear<T>(float x, float z, float radius) where T : MapObject;
    void RemoveObject(string objId);
    ClientMapObjectGridItem AddObject(MapObject obj, GEntity go);
    GEntity GetFXParent();
    void AddController(ClientMapObjectGridItem gridItem, GEntity go);
}

public class ClientMapObjectManager : IClientMapObjectManager
{
    private IRealtimeNetworkService _networkService;
    private IPlayerManager _playerManager;
    protected IClientRandom _rand;

    private List<UnitController> _controllers = new List<UnitController>();
    private Dictionary<long, IMapObjectFactory> _factories = new Dictionary<long, IMapObjectFactory>();

    private Dictionary<string, MapObject> _idDict = new Dictionary<string, MapObject>();
    private Dictionary<string, ClientMapObjectGridItem> _gridItems = new Dictionary<string, ClientMapObjectGridItem>();


    protected IUnityUpdateService _updateService;
    protected List<string> _olderSpawns = new List<string>();
    protected List<string> _recentlyLoadedSpawns = new List<string>();
    List<UnitController> _removeUnitList = new List<UnitController>();
    private Dictionary<long, IMapObjectLoader> _mapObjectLoaders = new Dictionary<long, IMapObjectLoader>();

    protected IUnityGameState _gs;

    public GEntity _fxParent;

    public ClientMapObjectManager(CancellationToken token)
    {
        UpdateRecentlyLoadedSpawns(token);
    }

    public void Reset()
    {
        _controllers = new List<UnitController>();
        _idDict = new Dictionary<string, MapObject>();
        _gridItems = new Dictionary<string, ClientMapObjectGridItem>();
        _olderSpawns = new List<string>();
        _recentlyLoadedSpawns = new List<string>();
        _removeUnitList = new List<UnitController>();
        GEntityUtils.DestroyAllChildren(_fxParent);
    }

    public GEntity GetFXParent()
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
    public async Task Initialize(IGameState gs, CancellationToken token)
    {
        Reset();
        _factories = ReflectionUtils.SetupDictionary<long, IMapObjectFactory>(gs);
        foreach (IMapObjectFactory mapObjFact in _factories.Values)
        {
            mapObjFact.Setup(gs);
        }
        if (!_didAddUpdate)
        {
            _updateService.AddTokenUpdate(this, FrameUpdate, UpdateType.Regular);
            _didAddUpdate = true;
        }
        if (Application.isPlaying)
        {
            _fxParent = GEntityUtils.FindSingleton("FXParent", true);
        }

        _mapObjectLoaders = ReflectionUtils.SetupDictionary<long, IMapObjectLoader>(gs);

        
    }


    public IMapObjectLoader GetMapObjectLoader(long entityTypeId)
    {
        if (_mapObjectLoaders.ContainsKey(entityTypeId))
        {
            return _mapObjectLoaders[entityTypeId];
        }
        return null;
    }


    public bool GetUnit (string id, out Unit unit, bool downloadIfNotExist = true)
    {
        unit = null;
        if (GetObject(id, out MapObject obj, downloadIfNotExist))
        {
            unit = obj as Unit;
        }
        return unit != null;
    }

    public bool GetChar(string id, out Character ch, bool downloadIfNotExist = true)
    {
        ch = null;
        if (GetObject(id, out MapObject obj, downloadIfNotExist))
        {
            ch = obj as Character;
        }
        return ch != null;
    }

    public bool GetObject(string id, out MapObject item, bool downloadIfNotExist = true)
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
        if (_playerManager.GetEntity() == null)
        {        
            return;
        }
        GVector3 playerPos = GVector3.Create(_playerManager.GetEntity().transform().position);
        foreach (UnitController controller in _controllers)
        {
            if (controller != null)
            {
                controller.OnUpdate(token);
                GVector3 pos = GVector3.Create(controller.transform().position);
                if (Math.Abs(pos.x-playerPos.x) >= MessageConstants.DefaultGridDistance*2 ||
                    Math.Abs(pos.z-playerPos.z) >= MessageConstants.DefaultGridDistance*2)
                {
                    _removeUnitList.Add(controller);
                }
            }
        }

        foreach (UnitController controller in _removeUnitList)
        {
            RemoveObject(controller.GetUnit().Id);
        }
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

    public void AddController(ClientMapObjectGridItem gridItem, GEntity go)
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

    public ClientMapObjectGridItem AddObject(MapObject obj, GEntity go)
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
            _updateService.AddDelayedUpdate(gridItem.GameObj, (_token) => { FadeOutObject(gridItem.GameObj); }, _token, 1.5f);
       }

        return;
    }

    protected void FadeOutObject(GEntity go)
    {
        GEntityUtils.Destroy(go);
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
        if (GetObject(spawn.ObjId, out MapObject currObj))
        {
            return currObj;
        }
        
        if (!_factories.ContainsKey(spawn.EntityTypeId))
        {
            return null;
        }

        IMapObjectFactory fact = _factories[spawn.EntityTypeId];

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
}
