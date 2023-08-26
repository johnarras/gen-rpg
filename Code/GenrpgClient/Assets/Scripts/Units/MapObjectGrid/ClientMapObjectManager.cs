using Cysharp.Threading.Tasks;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.MapObjects.Entities;
using System.Collections.Generic;
using UnityEngine;
using Genrpg.Shared.Characters.Entities;
using System;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Spawns.Interfaces;
using Genrpg.Shared.Reflection.Services;
using Genrpg.Shared.MapObjects.Factories;
using Assets.Scripts.Tokens;
using System.Threading;
using System.Threading.Tasks;
using Genrpg.Shared.MapObjects.Messages;


public interface IClientMapObjectManager : ISetupService, IMapTokenService
{
    bool GetUnit(string id, out Unit unit, bool downloadIfNotExist = true);
    bool GetChar(string id, out Character ch, bool downloadIfNotExist = true);
    bool GetObject(string id, out MapObject item, bool downloadIfNotExist = true);
    bool GetGridItem(string id, out ClientMapObjectGridItem gridItem);
    bool GetController(string unitId, out UnitController controller);
    MapObject SpawnObject(IMapSpawn spawn);
    void Reset();
    List<T> GetTypedObjectsNear<T>(float x, float z, float radius) where T : MapObject;
    void RemoveObject(string objId);
    ClientMapObjectGridItem AddObject(MapObject obj, GameObject go);
    GameObject GetFXParent();
    void AddController(ClientMapObjectGridItem gridItem, GameObject go);
}

public class ClientMapObjectManager : IClientMapObjectManager
{
    protected IReflectionService _reflectionService;
    private INetworkService _networkService;

    private List<UnitController> _controllers = new List<UnitController>();
    private Dictionary<long, IMapObjectFactory> _factories = new Dictionary<long, IMapObjectFactory>();

    private Dictionary<string, MapObject> _idDict = new Dictionary<string, MapObject>();
    private Dictionary<string, ClientMapObjectGridItem> _gridItems = new Dictionary<string, ClientMapObjectGridItem>();


    protected IUnityUpdateService _updateService;
    protected List<string> _olderSpawns = new List<string>();
    protected List<string> _recentlyLoadedSpawns = new List<string>();
    List<UnitController> _removeUnitList = new List<UnitController>();

    private GameState _gs;

    private GameObject _fxParent;

    public ClientMapObjectManager()
    {
        UpdateRecentlyLoadedSpawns().Forget();
    }

    public void Reset()
    {
        _controllers = new List<UnitController>();
        _idDict = new Dictionary<string, MapObject>();
        _gridItems = new Dictionary<string, ClientMapObjectGridItem>();
        _olderSpawns = new List<string>();
        _recentlyLoadedSpawns = new List<string>();
        _removeUnitList = new List<UnitController>();
        GameObjectUtils.DestroyAllChildren(_fxParent);
    }

    public GameObject GetFXParent()
    {
        return _fxParent;
    }

    private CancellationToken _token;
    public void SetToken(CancellationToken token)
    {
        _token = token;
    }

    public CancellationToken GetToken()
    {
        return _token;
    }

    private bool _didAddUpdate = false;
    public async Task Setup(GameState gs, CancellationToken token)
    {
        _gs = gs;
        Reset();
        _factories = _reflectionService.SetupDictionary<long, IMapObjectFactory>(gs);
        foreach (IMapObjectFactory mapObjFact in _factories.Values)
        {
            mapObjFact.Setup(gs);
        }
        if (!_didAddUpdate)
        {
            _updateService.AddTokenUpdate(this, FrameUpdate, UpdateType.Regular);
            _didAddUpdate = true;
        }
        _fxParent = GameObjectUtils.FindSingleton("FXParent", true);
        await Task.CompletedTask;
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
        if (PlayerObject.Get() == null)
        {        
            return;
        }
        Vector3 playerPos = PlayerObject.Get().transform.position;
        foreach (UnitController controller in _controllers)
        {
            if (controller != null)
            {
                controller.OnUpdate(token);
                Vector3 pos = controller.transform.position;
                if (Mathf.Abs(pos.x-playerPos.x) >= MessageConstants.DefaultGridDistance*2 ||
                    Mathf.Abs(pos.z-playerPos.z) >= MessageConstants.DefaultGridDistance*2)
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
            FadeOutObject(gridItem.GameObj).Forget();
        }

        return;
    }

    protected async UniTask FadeOutObject(GameObject go)
    {
        await UniTask.NextFrame();
        await UniTask.Delay(1500);
        GameObject.Destroy(go);
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
        if (GetObject(spawn.MapObjectId, out MapObject currObj))
        {
            return currObj;
        }
        
        if (!_factories.ContainsKey(spawn.EntityTypeId))
        {
            return null;
        }

        IMapObjectFactory fact = _factories[spawn.EntityTypeId];

        MapObject obj = fact.Create(_gs, spawn);

        return obj;
    }

    protected async UniTask UpdateRecentlyLoadedSpawns()
    {
        while (true)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(2.0f));
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
                if (Mathf.Sqrt(dx*dx+dz*dz) <= radius)
                {
                    retval.Add(t);
                }
            }
        }
        return retval;
    }
}
