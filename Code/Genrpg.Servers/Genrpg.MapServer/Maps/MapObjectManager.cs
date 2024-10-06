
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;
using System.Threading;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.Utils.Data;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Spawns.Interfaces;
using Genrpg.Shared.MapObjects.Factories;
using Genrpg.Shared.Utils;
using Genrpg.Shared.MapServer.Constants;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Entities.Constants;
using Genrpg.MapServer.Maps.Messaging;
using Genrpg.MapServer.Maps.Filters;
using Genrpg.MapServer.MapMessaging.Interfaces;
using Genrpg.Shared.AI.Settings;
using Genrpg.Shared.MapServer.Messages;
using Genrpg.Shared.MapObjects.Messages;
using Genrpg.Shared.Movement.Messages;
using Genrpg.Shared.Spawns.Settings;
using Genrpg.Shared.Spawns.WorldData;
using MongoDB.Bson.IO;
using Genrpg.Shared.Units.Constants;
using Genrpg.Shared.Logging.Interfaces;
using Genrpg.Shared.GameSettings;
using Genrpg.MapServer.Maps.Constants;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.MapServer.Services;
using Genrpg.Shared.HelperClasses;
using Genrpg.Shared.MapServer.Entities;

namespace Genrpg.MapServer.Maps
{

    public interface IMapObjectManager : IInitializable, IMessageTarget
    {
        MapObjectCounts GetCounts();
        void Init(IRandom rand, CancellationToken token);
        void UpdatePosition(IRandom rand, MapObject obj, int keysDown);
        MapObject SpawnObject(IRandom rand, IMapSpawn spawn);
        bool GetChar(string id, out Character ch, GetMapObjectParams objParams = null);
        bool GetUnit(string id, out Unit unit, GetMapObjectParams objParams = null);
        bool GetObject(string id, out MapObject item, GetMapObjectParams objParams = null);
        void FinalRemoveObjectFromOldGrid(IRandom rand, MapObject obj, MapObjectGridData gridData, MapObjectGridItem item);
        MapObjectGridItem RemoveObject(IRandom rand, string objId, float delaySeconds = 0);
        MapObjectGridItem AddObject(IRandom rand, MapObject obj, IMapSpawn spawn);
        List<T> GetTypedObjectsNear<T>(float wx, float wz, MapObject filterObject,
            float distance = MessageConstants.DefaultGridDistance,
            bool enforceDistance = false, List<long> filters = null) where T : MapObject;
        List<MapObject> GetObjectsNear(float wx, float wz,
            MapObject filterObj,
            float distance = MessageConstants.DefaultGridDistance,
            bool enforceDistance = false,
            List<long> filters = null);
        List<Character> GetAllCharacters();
        int GetGridIndexFromCoord(double mapPos, bool useCeiling);
        IReadOnlyList<MapObject> GetObjectsFromGridCell(int gx, int gz);
    }

    public class MapObjectManager : IMapObjectManager
    {

        protected const int IdHashSize = 10000;

        protected MapObjectGridData[,] _objectGrid = null;

        protected Dictionary<int, ConcurrentDictionary<string, MapObjectGridItem>> _idDict =
            new Dictionary<int, ConcurrentDictionary<string, MapObjectGridItem>>();

        protected Dictionary<long, ConcurrentDictionary<string, Character>> _zoneDict =
            new Dictionary<long, ConcurrentDictionary<string, Character>>();

        protected bool _didSetupOnce = false;

        protected int _gridSize = 0;

        CancellationToken _token;

        protected long _objectCount = 0;
        protected long _unitCount = 0;
        protected long _totalQueries = 0;
        protected long _totalGridReads = 0;
        protected long _totalGridLocks = 0;
        protected long _unitsAdded = 0;
        protected long _unitsRemoved = 0;

        protected long _totalUnits = 0;
        protected long _totalObjects = 0;

        protected long _idLookupObjectCount = 0;
        protected long _gridObjectCount = 0;
        protected long _zoneObjectCount = 0;

        private SetupDictionaryContainer<long, IMapObjectFactory> _factories = new();
        private SetupDictionaryContainer<long, IObjectFilter> _filters = new();

        private IMapMessageService _messageService = null;
        private ILogService _logService = null;
        private IGameData _gameData = null;
        protected IRepositoryService _repoService = null;
        private IMapProvider _mapProvider = null;

        // Used when we need messages to mapobjects that no longer exist.
        const int MessageTargetCount = 100;
        private long _messageTargetIndex = 0;
        private MapObject[] _messageTargets = null;

        public MapObjectCounts GetCounts()
        {

            if (!MapInstanceConstants.ServerTestMode)
            {
                _idLookupObjectCount = 0;
                foreach (ConcurrentDictionary<string,MapObjectGridItem> dict in _idDict.Values)
                {
                    _idLookupObjectCount += dict.Count;
                }

                _gridObjectCount = 0;
                for (int x = 0; x < _gridSize; x++)
                {
                    for (int y = 0; y < _gridSize; y++)
                    {
                        _gridObjectCount += _objectGrid[x, y].GetObjects().Count;
                    }
                }

                _zoneObjectCount = 0;
                foreach (ConcurrentDictionary<string,Character> zdict in _zoneDict.Values)
                {
                    _zoneObjectCount += zdict.Count;
                }

            }

            MapObjectCounts counts = new MapObjectCounts()
            {
                CurrentObjectCount = _objectCount,
                UnitsAdded = _unitsAdded,
                UnitsRemoved = _unitsRemoved,
                CurrentUnitCount = _unitCount,
                TotalQueries = _totalQueries,
                TotalGridReads = _totalGridReads,
                TotalGridLocks = _totalGridLocks,
                UnitTotal = _totalUnits,
                ObjectTotal = _totalObjects,
                IdLookupObjectAccount = _idLookupObjectCount,
                GridObjectCount = _gridObjectCount,
                ZoneObjectCount = _zoneObjectCount,
            };
            return counts;
        }
        public async Task Initialize( CancellationToken token)
        {
            for (int i = 0; i < 256; i++)
            {
                _zoneDict[i] = new ConcurrentDictionary<string, Character>();
            }
            await Task.CompletedTask;
        }

        public virtual void Init(IRandom rand, CancellationToken token)
        {
            _token = token;

            _gridSize = MapUtils.GetMapObjectGridSize(_mapProvider.GetMap());

            _objectGrid = new MapObjectGridData[_gridSize, _gridSize];

            for (int x = 0; x < _gridSize; x++)
            {
                for (int z = 0; z < _gridSize; z++)
                {
                    _objectGrid[x, z] = new MapObjectGridData() { GX = x, GZ = z };
                }
            }

            SetupGridSpawns(rand);

            if (!_didSetupOnce)
            {
                for (int i = 0; i < IdHashSize; i++)
                {
                    _idDict[i] = new ConcurrentDictionary<string, MapObjectGridItem>();
                }
                for (int x = 0; x < _gridSize; x++)
                {
                    for (int z = 0; z < _gridSize; z++)
                    {
                        SpawnGridObjects(rand, x, z);
                    }
                }
                _didSetupOnce = true;
                _unitsAdded = 0; 
            }

            _messageTargets = new MapObject[MessageTargetCount];
            for (int i = 0; i < MessageTargetCount; i++)
            {
                _messageTargets[i] = new MapObject(_repoService) { Id = (i * 13 + 17) + ".ObjManager" };
            }
        }

        public void FinalRemoveObjectFromOldGrid(IRandom rand, MapObject obj, MapObjectGridData gridData, MapObjectGridItem item)
        {
            gridData.RemoveObj(item.Obj);
            // Should be ok to do thsi here becaue this will happen microseconds after the move between grid cells,
            // and objects can only move between grid cells every 3 seconds.
            item.OldGX = item.GX;
            item.OldGZ = item.GZ;
            _totalGridLocks++;
        }

        private long _multiRemoveTimes = 0;
        private void RemoveItemFromOldGridOnRemoveFromMap(MapObjectGridItem item)
        {
            int oldgz = item.OldGZ;
            int oldgx = item.OldGX;

            if ((oldgz != item.GZ || oldgx != item.GX) &&
            (oldgx >= 0 && oldgx < _gridSize && oldgz >= 0 && oldgz < _gridSize))
            {
                if (_objectGrid[oldgx,oldgz].GetObjects().Contains(item.Obj))
                {
                    _logService.Message($"Removing object from old grid in middle of destroy {++_multiRemoveTimes}");
                }
                _objectGrid[oldgx, oldgz].RemoveObj(item.Obj);
                item.OldGX = item.GX;
                item.OldGZ = item.GZ;
            }
            _totalGridLocks++;
        }

        public void UpdatePosition(IRandom rand, MapObject obj, int keysDown)
        {

            OnUpdatePos posMessage = obj.GetCachedMessage<OnUpdatePos>(true);

            posMessage.ObjId = obj.Id;
            posMessage.TargetId = obj.TargetId;
            posMessage.SetX(obj.X);
            posMessage.SetY(obj.Y);
            posMessage.SetZ(obj.Z);
            posMessage.SetRot(obj.Rot);
            posMessage.SetSpeed(obj.Speed);
            posMessage.SetKeysDown(keysDown);

            _messageService.SendMessageNear(obj, posMessage, playersOnly: false);

            PointXZ newGridPos = MapUtils.GetGridCoordinates(obj.X, obj.Z, _gridSize);

            if (GetGridItem(obj.Id, out MapObjectGridItem gridItem))
            {
                int currgx = gridItem.GX;
                int currgz = gridItem.GZ;

                if (newGridPos.X != currgx || newGridPos.Z != currgz)
                {
                    if (obj.LastGridChange > DateTime.UtcNow.AddSeconds(-UnitConstants.GridChangeCooldownSeconds))
                    {
                        return;
                    }

                    MapObjectGridData oldGrid = _objectGrid[currgx, currgz];
                    MapObjectGridData newGrid = _objectGrid[newGridPos.X, newGridPos.Z];


                    newGrid.AddObj(gridItem.Obj);
                    _totalGridLocks += 2;

                    gridItem.OldGX = gridItem.GX;
                    gridItem.OldGZ = gridItem.GZ;
                    gridItem.GX = newGridPos.X;
                    gridItem.GZ = newGridPos.Z;
                    OnAddObjectToGrid(rand, obj, newGridPos.X, newGridPos.Z);

                    // Slight delay in removing grid item to allow for things processing nearby cells to complete.
                    _messageService.SendMessage(obj, new RemoveObjectFromGridCell() { GridItem = gridItem, GridData = oldGrid });

                }
            }
            UpdateZone(obj);
        }

        public List<T> GetTypedObjectsNear<T>(float wx, float wz, MapObject filterObject,
            float distance = MessageConstants.DefaultGridDistance,
            bool enforceDistance = false, List<long> filters = null) where T : MapObject
        {
            return GetObjectsNear(wx, wz, filterObject, distance, enforceDistance, filters).OfType<T>().ToList();
        }

        public List<MapObject> GetObjectsNear(float wx, float wz,
            MapObject filterObj,
            float distance = MessageConstants.DefaultGridDistance,
            bool enforceDistance = false,
            List<long> filters = null)
        {
            int gxmin = GetGridIndexFromCoord(wx - distance, false);
            int gxmax = GetGridIndexFromCoord(wx + distance, true);
            int gzmin = GetGridIndexFromCoord(wz - distance, false);
            int gzmax = GetGridIndexFromCoord(wz + distance, true);

            List<MapObject> tempObjects = GetObjectsFromGridBox(gxmin, gxmax, gzmin, gzmax);

            float distSquared = distance * distance;

            List<MapObject> retval = new List<MapObject>();

            foreach (MapObject obj in tempObjects)
            {
                if (obj.IsDeleted())
                {
                    continue;
                }

                if (enforceDistance)
                {
                    float dx = Math.Abs(obj.X - wx);
                    if (dx > distance)
                    {
                        continue;
                    }
                    float dz = Math.Abs(obj.Z - wz);
                    if (dz > distance)
                    {
                        continue;
                    }
                    if (dx * dx + dz * dz > distSquared) // Maybe drop this?
                    {
                        continue;
                    }
                }

                retval.Add(obj);
            }

            if (filters != null && filterObj != null)
            {
                foreach (long filterTypeId in filters)
                {
                    if (GetObjectFilter(filterTypeId, out IObjectFilter objFilter))
                    {
                        retval = objFilter.Filter(filterObj, retval);
                    }
                }
            }

            return retval;

        }

        public IReadOnlyList<MapObject> GetObjectsFromGridCell(int gx, int gz)
        {
            if (gx >= 0 && gz >= 0 && gx < _gridSize && gz <  _gridSize)
            {
                return _objectGrid[gx, gz].GetObjects();
            }
            return new List<MapObject>();   
        }

        protected List<MapObject> GetObjectsFromGridBox(int gxmin, int gxmax, int gzmin, int gzmax)
        {
            _totalQueries++;
            List<MapObject> retval = new List<MapObject>();

            gxmin = MathUtils.Clamp(0, gxmin, _gridSize - 1);
            gxmax = MathUtils.Clamp(0, gxmax, _gridSize - 1);
            gzmin = MathUtils.Clamp(0, gzmin, _gridSize - 1);
            gzmax = MathUtils.Clamp(0, gzmax, _gridSize - 1);

            for (int gx = gxmin; gx <= gxmax; gx++)
            {
                for (int gz = gzmin; gz <= gzmax; gz++)
                {
                    // Do not need to lock here because the .Objs list is immutable (copied then updated and replaced when add/remove happens)
                    retval.AddRange(_objectGrid[gx, gz].GetObjects());
                    _totalGridReads++;
                }
            }

            return retval;
        }

        public bool GetChar(string id, out Character ch, GetMapObjectParams objParams = null)
        {
            if (GetObject(id, out MapObject obj, objParams))
            {
                ch = obj as Character;
                return ch != null;
            }
            ch = null;
            return false;
        }

        public bool GetUnit(string id, out Unit unit, GetMapObjectParams objParams = null)
        {
            if (GetObject(id, out MapObject obj, objParams))
            {
                unit = obj as Unit;
                return unit != null;
            }
            unit = null;
            return false;
        }

        public virtual bool GetObject(string id, out MapObject item, GetMapObjectParams objParams = null)
        {
            if (GetGridItem(id, out MapObjectGridItem gridItem))
            {
                if (gridItem.Obj.IsDeleted())
                {
                    item = null;
                    return false;
                }
                item = gridItem.Obj;
                return true;
            }
            item = null;
            return false;
        }


        protected bool GetGridItem(string objId, out MapObjectGridItem retval)
        {
            if (string.IsNullOrEmpty(objId))
            {
                retval = null;
                return false;
            }
            if (_idDict[StrUtils.GetIdHash(objId) % IdHashSize].TryGetValue(objId, out MapObjectGridItem item))
            {
                retval = item;
                return true;
            }
            retval = null;
            return false;
        }

        public int GetGridIndexFromCoord(double mapPos, bool useCeiling)
        {
            return MapUtils.GetGridIndexFromCoord(mapPos, _gridSize, useCeiling);
            
        }

        public virtual MapObjectGridItem AddObject(IRandom rand, MapObject obj, IMapSpawn spawn)
        {
            PointXZ pt = MapUtils.GetGridCoordinates(obj.X, obj.Z, _gridSize);

            if (GetGridItem(obj.Id, out MapObjectGridItem currentGridItem))
            {
                return currentGridItem;
            }

            MapObjectGridItem newGridItem = CreateGridItem(rand, obj, pt.X, pt.Z);

            _idDict[obj.GetIdHash() % IdHashSize].TryAdd(obj.Id, newGridItem);
            obj.Spawn = spawn;
            if (obj is Unit unit)
            {
                _unitCount++;
                _unitsAdded++;
                if (obj is Character ch)
                {
                    OnAddObjectToGrid(rand, ch, pt.X, pt.Z);
                }
            }
            else
            {
                _objectCount++;
            }
            _objectGrid[pt.X, pt.Z].AddObj(newGridItem.Obj);
            _totalGridLocks++;


            if (_didSetupOnce && _messageService != null)
            {
                _messageService.SendMessageNear(obj, new OnSpawn(obj));
            }

            UpdateZone(obj);

            return newGridItem;
        }

        public MapObjectGridItem RemoveObject(IRandom rand, string objId, float delaySeconds = 0)
        {
            if (!GetGridItem(objId, out MapObjectGridItem currentItem))
            {
                return null;
            }
            if (delaySeconds > 0)
            {
                RemoveObjectFromMap delayed = new RemoveObjectFromMap()
                {
                    ObjectId = objId,
                };

                _messageService.SendMessage(currentItem.Obj, delayed, delaySeconds);
                return currentItem;
            }

            if (!_idDict[StrUtils.GetIdHash(objId) % IdHashSize].TryRemove(objId, out MapObjectGridItem gridItem))
            {
                return null;
            }
            gridItem.Obj.SetDeleted(true);
            gridItem.Obj.Dispose();

            if (gridItem.Obj is Unit unit)
            {
                _unitCount--;
                _unitsRemoved++;
            }
            else
            {
                _objectCount--;
            }
            if (gridItem.GX >= 0 && gridItem.GX < _gridSize &&
                gridItem.GZ >= 0 && gridItem.GZ < _gridSize)
            {
                _objectGrid[gridItem.GX, gridItem.GZ].RemoveObj(gridItem.Obj);
                RemoveItemFromOldGridOnRemoveFromMap(gridItem);
                _totalGridLocks++;
            }

            if (gridItem.Obj is Character ch)
            {
                if (_zoneDict.TryGetValue(ch.ZoneId, out ConcurrentDictionary<string, Character> currDict))
                {
                    if (currDict.TryGetValue(ch.Id, out Character currChar))
                    {
                        currDict.TryRemove(ch.Id, out Character delChar);
                    }
                }
            }

            OnRemove(rand, gridItem);
            return gridItem;
        }

        public MapObject GetMessageTarget()
        {
            return _messageTargets[++_messageTargetIndex % MessageTargetCount];
        }

        protected virtual void OnRemove(IRandom rand, MapObjectGridItem gridItem)
        {

            if (gridItem.Obj.Spawn != null)
            {
                gridItem.Obj.Spawn.SpawnSeconds = 20;
                _messageService.SendMessage(GetMessageTarget(), new RespawnObject() { Spawn = gridItem.Obj.Spawn },
                    MathUtils.IntRange(gridItem.Obj.Spawn.SpawnSeconds,gridItem.Obj.Spawn.SpawnSeconds*2, rand));
            }

            DespawnObject despawn = new DespawnObject()
            {
                ObjId = gridItem.Obj.Id,
            };

            _messageService.SendMessageNear(gridItem.Obj, despawn, MessageConstants.DefaultGridDistance * 2);

        }

        protected virtual MapObjectGridItem CreateGridItem(IRandom rand, MapObject obj, int gx, int gz)
        {
            MapObjectGridItem item = new MapObjectGridItem()
            {
                Obj = obj,
                GX = gx,
                GZ = gz,
            };
            return item;
        }
        protected virtual void SetupGridSpawns(IRandom rand)
        {

            for (int x = 0; x < _gridSize; x++)
            {
                for (int y = 0; y < _gridSize; y++)
                {
                    _objectGrid[x, y].Spawns = new List<MapSpawn>();
                }
            }
            _totalUnits = _mapProvider.GetSpawns().Data
                .Where(x => x.EntityTypeId == EntityTypes.Unit || x.EntityTypeId == EntityTypes.ZoneUnit)
                .Count();
            _totalObjects = _mapProvider.GetSpawns().Data.Count - _totalUnits;

            List<MapSpawn> copySpawns = new List<MapSpawn>();
            foreach (MapSpawn spawn in _mapProvider.GetSpawns().Data)
            {
                if (spawn.EntityTypeId == EntityTypes.Unit || spawn.EntityTypeId== EntityTypes.ZoneUnit)
                {
                    int maxTimes = 1;// + _rand.Next() % 3;
                    for (int times = 0; times < maxTimes; times++)
                    {
                        MapSpawn copySpawn = SerializationUtils.FastMakeCopy(spawn);
                        if (times > 0)
                        {
                            int delta = 25;
                            copySpawn.X += MathUtils.IntRange(-delta, delta, rand);
                            copySpawn.Z += MathUtils.IntRange(-delta, delta, rand);
                            copySpawn.ObjId += "." + times.ToString();
                        }
                        if (!copySpawn.GetAddons().Any())
                        {
                            copySpawn.FactionTypeId = MathUtils.IntRange(1, 4, rand);
                        }
                        copySpawns.Add(copySpawn);
                    }
                }
                else
                {
                    copySpawns.Add(spawn);
                }
            }

            _mapProvider.GetSpawns().Data = copySpawns;

            foreach (MapSpawn spawn in _mapProvider.GetSpawns().Data)
            {

                if (rand.NextDouble() > _gameData.Get<SpawnSettings>(null).MapSpawnChance)
                {
                    continue;
                }

                int gx = GetGridIndexFromCoord(spawn.X, false);
                int gz = GetGridIndexFromCoord(spawn.Z, false);

                _objectGrid[gx, gz].Spawns.Add(spawn);
            }
        }

        protected virtual bool SpawnGridObjects(IRandom rand, int gx, int gy)
        {

            MapObjectGridData grid = _objectGrid[gx, gy];
            lock (grid.SpawnLock)
            {
                if (grid.SpawnedObjects)
                {
                    return false;
                }

                int totalSpawns = 0;

                try
                {
                    List<MapObject> newObjects = new List<MapObject>();
                    foreach (MapSpawn spawn in grid.Spawns)
                    {
                        SpawnObject(rand, spawn);
                        totalSpawns++;
                    }
                }
                catch (Exception e)
                {
                    _logService.Exception(e, "SpawnObject");
                }
                grid.SpawnedObjects = true;
            }
            return true;
        }

        public virtual MapObject SpawnObject(IRandom rand, IMapSpawn spawn)
        {

            if (GetObject(spawn.ObjId, out MapObject currObj))
            {
                return currObj;
                }

            if (!GetFactory(spawn.EntityTypeId, out IMapObjectFactory fact))
            {
                return null;
            }

            MapObject obj = fact.Create(rand, spawn);
            AfterSpawns(rand, obj);
            if (obj != null)
            {
                AddObject(rand, obj, spawn);
            }

            return obj;
        }

        protected void AfterSpawns(IRandom rand, MapObject obj)
        {
            if (obj is Unit unit)
            {
                AIUpdate update = new AIUpdate();

                _messageService.SendMessage(unit, update, MathUtils.FloatRange(0, _gameData.Get<AISettings>(obj).UpdateSeconds, rand));
            }
        }

        protected virtual void SendGridItemsToClient(IRandom rand, Character ch, int gx, int gz)
        {
            if (SpawnGridObjects(rand, gx, gz))
            {
                return;
            }
            IReadOnlyList<MapObject> items = GetObjectsFromGridCell(gx, gz);

            if (items.Count < 1)
            {
                return;
            }

            foreach (MapObject wo in items)
            {
                _messageService.SendMessage(ch, new OnSpawn(wo));
            }
        }

        protected bool GetFactory(long entityTypeId, out IMapObjectFactory factory)
        {
            return _factories.TryGetValue(entityTypeId, out factory);
        }

        protected bool GetObjectFilter(long filterTypeId, out IObjectFilter filter)
        {
            return _filters.TryGetValue(filterTypeId, out filter);
        }

        protected void OnAddObjectToGrid(IRandom rand, MapObject obj, int gx, int gz)
        {
            if (!(obj is Character ch))
            {
                return;
            }

            if (obj.IsDeleted())
            {
                return;
            }

            obj.AddMessage(new OnAddToGrid() { GridX = gx, GridZ = gz, UserId = obj.Id });
            for (int x = gx - UnitConstants.PlayerObjectVisRadius; x <= gx + UnitConstants.PlayerObjectVisRadius; x++)
            {
                if (x < 0 || x >= _gridSize)
                {
                    continue;
                }
                for (int z = gz - UnitConstants.PlayerObjectVisRadius; z <= gz + UnitConstants.PlayerObjectVisRadius; z++)
                {
                    if (z < 0 || z >= _gridSize)
                    {
                        continue;
                    }

                    if (Math.Abs(gz - z) == UnitConstants.PlayerObjectVisRadius &&
                        Math.Abs(gx - x) == UnitConstants.PlayerObjectVisRadius)
                    {
                        continue;
                    }

                    PointXZ currentCell = ch.NearbyGridsSeen.FirstOrDefault(p => p.X == x && p.Z == z);
                    if (currentCell != null)
                    {
                        continue;
                    }

                    ch.NearbyGridsSeen.Add(new PointXZ(x, z));
                    SendGridItemsToClient(rand, ch, x, z);
                }
            }
            ch.NearbyGridsSeen =
                ch.NearbyGridsSeen.Where(p =>
                Math.Abs(p.X - gx) <= UnitConstants.PlayerObjectVisRadius &&
                Math.Abs(p.Z - gz) <= UnitConstants.PlayerObjectVisRadius).ToList();
        }

        private void UpdateZone(MapObject obj)
        {
            if (!(obj is Character ch))
            {
                return;
            }

            if (ch.PrevZoneId == ch.ZoneId)
            {
                return;
            }

            if (_zoneDict.TryGetValue(obj.PrevZoneId, out ConcurrentDictionary<string, Character> prevDict))
            {
                if (prevDict.TryGetValue(ch.Id, out Character prevChar))
                {
                    prevDict.TryRemove(ch.Id, out Character delChar);
                }
            }
            if (_zoneDict.TryGetValue(obj.ZoneId, out ConcurrentDictionary<string, Character> currDict))
            {
                currDict.TryAdd(ch.Id, ch);
            }
        }

        public List<Character> GetAllCharacters()
        {
            List<Character> retval = new List<Character>();
            foreach (ConcurrentDictionary<string,Character> dict in _zoneDict.Values)
            {
                retval.AddRange(dict.Values);
            }

            return retval.Distinct().ToList();
        }
    }
}
