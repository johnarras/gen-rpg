﻿
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
using Genrpg.Shared.Reflection.Services;
using Genrpg.Shared.MapServer.Constants;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Characters.Entities;
using Genrpg.Shared.Spawns.Entities;
using Genrpg.Shared.Entities.Constants;
using Genrpg.MapServer.Maps.Messaging;
using Genrpg.MapServer.Maps.Filters;
using Genrpg.MapServer.MapMessaging.Interfaces;
using Genrpg.Shared.AI.Entities;
using Genrpg.Shared.MapServer.Messages;
using Genrpg.Shared.MapObjects.Messages;

namespace Genrpg.MapServer.Maps
{

    public interface IMapObjectManager : ISetupService, IMessageTarget
    {
        MapObject GetMessageTarget();
        MapObjectCounts GetCounts();
        void Init(GameState gs, CancellationToken token);
        void DelayedRemoveFromGrid(MapObject obj, MapObjectGridData gridData, MapObjectGridItem item);
        void UpdatePosition(MapObject obj);
        MapObject SpawnObject(IMapSpawn spawn);
        bool GetChar(string id, out Character ch, GetMapObjectParams objParams = null);
        bool GetUnit(string id, out Unit unit, GetMapObjectParams objParams = null);
        bool GetObject(string id, out MapObject item, GetMapObjectParams objParams = null);
        void FinalRemoveFromGrid(MapObject obj, MapObjectGridData gridData, MapObjectGridItem item);
        MapObjectGridItem RemoveObject(string objId, float delaySeconds = 0);
        MapObjectGridItem AddObject(MapObject obj, IMapSpawn spawn);
        List<T> GetTypedObjectsNear<T>(float wx, float wz, MapObject filterObject,
            float distance = MessageConstants.DefaultGridDistance,
            bool enforceDistance = false, List<long> filters = null) where T : MapObject;
        List<MapObject> GetObjectsNear(float wx, float wz,
            MapObject filterObj,
            float distance = MessageConstants.DefaultGridDistance,
            bool enforceDistance = false,
            List<long> filters = null);
    }

    public class MapObjectManager : IMapObjectManager
    {

        protected const int IdHashSize = 10000;

        protected MapObjectGridData[,] _objectGrid = null;

        protected Dictionary<int, ConcurrentDictionary<string, MapObjectGridItem>> _idDict =
            new Dictionary<int, ConcurrentDictionary<string, MapObjectGridItem>>();

        protected Dictionary<long, Dictionary<string, Character>> _zoneDict =
            new Dictionary<long, Dictionary<string, Character>>();

        protected GameState _gs = null;

        protected bool _didSetupOnce = false;

        protected int _gridSize = 0;

        CancellationToken _token;

#if DEBUG
        protected long _objectCount = 0;
        protected long _unitCount = 0;
        protected long _totalQueries = 0;
        protected long _totalGridReads = 0;
        protected long _totalGridLocks = 0;
        protected long _unitsAdded = 0;
        protected long _unitsRemoved = 0;
#endif

        protected long _totalUnits = 0;
        protected long _totalObjects = 0;

        private Dictionary<long, IMapObjectFactory> _factories = new Dictionary<long, IMapObjectFactory>();
        private Dictionary<long, IObjectFilter> _filters = new Dictionary<long, IObjectFilter>();

        private IMapMessageService _messageService = null;
        private IReflectionService _reflectionService;

        private MapObject _messageTarget = new MapObject() { Id = typeof(MapObjectManager).Name };
        public MapObject GetMessageTarget()
        {
            return _messageTarget;
        }



        public MapObjectCounts GetCounts()
        {
            MapObjectCounts counts = new MapObjectCounts()
            {
#if DEBUG
                CurrentObjectCount = _objectCount,
                UnitsAdded = _unitsAdded,
                UnitsRemoved = _unitsRemoved,
                CurrentUnitCount = _unitCount,
                TotalQueries = _totalQueries,
                TotalGridReads = _totalGridReads,
                TotalGridLocks = _totalGridLocks,
#endif
                UnitTotal = _totalUnits,
                ObjectTotal = _totalObjects,
            };
            return counts;
        }
        public async Task Setup(GameState gs, CancellationToken token)
        {
            _factories = _reflectionService.SetupDictionary<long, IMapObjectFactory>(gs);
            foreach (IMapObjectFactory mapObjFact in _factories.Values)
            {
                mapObjFact.Setup(gs);
            }
            _filters = _reflectionService.SetupDictionary<long, IObjectFilter>(gs);

            for (int i = 0; i < 256; i++)
            {
                _zoneDict[i] = new Dictionary<string, Character>();
            }
            await Task.CompletedTask;
        }

        public virtual void Init(GameState gs, CancellationToken token)
        {
            _token = token;
            _gs = gs;

            _gridSize = (int)Math.Ceiling(1.0 * gs.map.GetMapSize(gs) / SharedMapConstants.MapObjectGridSize);

            _objectGrid = new MapObjectGridData[_gridSize, _gridSize];

            for (int x = 0; x < _gridSize; x++)
            {
                for (int z = 0; z < _gridSize; z++)
                {
                    _objectGrid[x, z] = new MapObjectGridData() { GX = x, GZ = z };
                }
            }

            SetupGridSpawns(gs);

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
                        SpawnGridObjects(x, z);
                    }
                }
                _didSetupOnce = true;
#if DEBUG
                _unitsAdded = 0;           
#endif
            }
        }

        public virtual void DelayedRemoveFromGrid(MapObject obj, MapObjectGridData gridData, MapObjectGridItem item)
        {
            RemoveObjectFromGridCell delayed = new RemoveObjectFromGridCell()
            {
                Data = gridData,
                Item = item,
            };
            // This is requeued but the assumption is that the queue is long enough that it won't get
            // processed until all current searches are completed. If it ends up that the queue
            // is too fast, this could be changed to a 0.01 second delay or something.
            _messageService.SendMessage(_gs, obj, delayed);
        }

        public void FinalRemoveFromGrid(MapObject obj, MapObjectGridData gridData, MapObjectGridItem item)
        {
            gridData.RemoveObj(item.Obj);
#if DEBUG
            _totalGridLocks++;
#endif
        }

        public void UpdatePosition(MapObject obj)
        {
            PointXZ newGridPos = GetGridCoordinates(obj);

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
#if DEBUG
                    _totalGridLocks++;
#endif
                    gridItem.GX = newGridPos.X;
                    gridItem.GZ = newGridPos.Z;
                    OnAddObjectToGrid(obj, newGridPos.X, newGridPos.Z);

                    DelayedRemoveFromGrid(obj, oldGrid, gridItem);
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
            int gxmin = GetIndexFromCoord(wx - distance, false);
            int gxmax = GetIndexFromCoord(wx + distance, true);
            int gzmin = GetIndexFromCoord(wz - distance, false);
            int gzmax = GetIndexFromCoord(wz + distance, true);

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
                        retval = objFilter.Filter(_gs, filterObj, retval);
                    }
                }
            }

            return retval;

        }

        protected List<MapObject> GetObjectsFromGrid(int gx, int gz)
        {
            return GetObjectsFromGridBox(gx, gx, gz, gz);
        }

        protected List<MapObject> GetObjectsFromGridBox(int gxmin, int gxmax, int gzmin, int gzmax)
        {
#if DEBUG
            _totalQueries++;
#endif
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
#if DEBUG
                    _totalGridReads++;
#endif
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

        protected int GetIndexFromCoord(double mapPos, bool useCeiling)
        {
            if (!useCeiling)
            {
                return MathUtils.Clamp(0, (int)(mapPos / SharedMapConstants.MapObjectGridSize), _gridSize - 1);
            }
            else
            {
                return MathUtils.Clamp(0, (int)Math.Ceiling(mapPos / SharedMapConstants.MapObjectGridSize), _gridSize - 1);
            }
        }

        protected PointXZ GetGridCoordinates(MapObject obj)
        {
            return new PointXZ(GetIndexFromCoord(obj.X, false), GetIndexFromCoord(obj.Z, false));
        }

        public virtual MapObjectGridItem AddObject(MapObject obj, IMapSpawn spawn)
        {

            PointXZ pt = GetGridCoordinates(obj);

            if (GetGridItem(obj.Id, out MapObjectGridItem currentGridItem))
            {
                return currentGridItem;
            }

            MapObjectGridItem newGridItem = CreateGridItem(obj, pt.X, pt.Z);

            _objectGrid[pt.X, pt.Z].AddObj(newGridItem.Obj);
#if DEBUG
            _totalGridLocks++;
#endif

            _idDict[StrUtils.GetIdHash(obj.Id) % IdHashSize].TryAdd(obj.Id, newGridItem);
            obj.Spawn = spawn;
            if (obj is Unit unit)
            {
#if DEBUG
                _unitCount++;
                _unitsAdded++;
#endif
                if (obj is Character ch)
                {
                    OnAddObjectToGrid(ch, pt.X, pt.Z);
                }
            }
            else
            {
#if DEBUG
                _objectCount++;
#endif
            }

            if (_didSetupOnce && _messageService != null)
            {
                _messageService.SendMessageNear(_gs, obj, new OnSpawn(obj));
            }

            UpdateZone(obj);

            return newGridItem;
        }

        public virtual MapObjectGridItem RemoveObject(string objId, float delaySeconds = 0)
        {
            if (!GetGridItem(objId, out MapObjectGridItem currentItem))
            {
                return null;
            }
            if (delaySeconds > 0)
            {
                DelayedRemoveObject delayed = new DelayedRemoveObject()
                {
                    ObjectId = objId,
                };

                _messageService.SendMessage(_gs, currentItem.Obj, delayed, delaySeconds);
                return currentItem;
            }

            if (!_idDict[StrUtils.GetIdHash(objId) % IdHashSize].TryRemove(objId, out MapObjectGridItem gridItem))
            {
                return null;
            }
            gridItem.Obj.SetDeleted(true);
#if DEBUG
            if (gridItem.Obj is Unit unit)
            {
                _unitCount--;
                _unitsRemoved++;
            }
            else
            {
                _objectCount--;
            }
#endif
            if (gridItem.GX >= 0 && gridItem.GX < _gridSize &&
                gridItem.GZ >= 0 && gridItem.GZ < _gridSize)
            {

                _objectGrid[gridItem.GX, gridItem.GZ].RemoveObj(gridItem.Obj);
#if DEBUG
                _totalGridLocks++;
#endif
            }

            if (gridItem.Obj is Character ch)
            {
                if (_zoneDict.TryGetValue(ch.ZoneId, out Dictionary<string, Character> currDict))
                {
                    if (currDict.TryGetValue(ch.Id, out Character currChar))
                    {
                        currDict.Remove(ch.Id);
                    }
                }
            }

            OnRemove(gridItem);
            return gridItem;
        }

        protected virtual void OnRemove(MapObjectGridItem gridItem)
        {

            if (gridItem.Obj.Spawn != null)
            {
                gridItem.Obj.Spawn.SpawnSeconds = 20;
                _messageService.SendMessage(_gs, GetMessageTarget(), new RespawnObject() { Spawn = gridItem.Obj.Spawn },
                    MathUtils.IntRange(gridItem.Obj.Spawn.SpawnSeconds,gridItem.Obj.Spawn.SpawnSeconds*2,_gs.rand));
            }

            DespawnObject despawn = new DespawnObject()
            {
                ObjId = gridItem.Obj.Id,
            };

            _messageService.SendMessageNear(_gs, gridItem.Obj, despawn, MessageConstants.DefaultGridDistance * 2);

        }

        protected virtual MapObjectGridItem CreateGridItem(MapObject obj, int gx, int gz)
        {
            MapObjectGridItem item = new MapObjectGridItem()
            {
                Obj = obj,
                GX = gx,
                GZ = gz,
            };
            return item;
        }
        protected virtual void SetupGridSpawns(GameState gs)
        {

            for (int x = 0; x < _gridSize; x++)
            {
                for (int y = 0; y < _gridSize; y++)
                {
                    _objectGrid[x, y].Spawns = new List<MapSpawn>();
                }
            }
            _totalUnits = gs.spawns.Data
                .Where(x => x.EntityTypeId == EntityType.Unit || x.EntityTypeId == EntityType.NPC)
                .Count();
            _totalObjects = gs.spawns.Data.Count - _totalUnits;

            List<MapSpawn> copySpawns = new List<MapSpawn>();
            foreach (MapSpawn spawn in gs.spawns.Data)
            {
                if (spawn.EntityTypeId == EntityType.Unit || spawn.EntityTypeId== EntityType.ZoneUnit)
                {
                    int maxTimes = 1;// + _gs.rand.Next() % 3;
                    for (int times = 0; times < maxTimes; times++)
                    {
                        MapSpawn copySpawn = SerializationUtils.FastMakeCopy(spawn);
                        if (times > 0)
                        {
                            int delta = 25;
                            copySpawn.X += MathUtils.IntRange(-delta, delta, _gs.rand);
                            copySpawn.Z += MathUtils.IntRange(-delta, delta, _gs.rand);
                            copySpawn.MapObjectId += "." + times.ToString();
                        }
                        copySpawn.FactionTypeId = MathUtils.IntRange(1, 20, gs.rand);
                        copySpawns.Add(copySpawn);
                    }
                }
                else
                {
                    copySpawns.Add(spawn);
                }
            }

            gs.spawns.Data = copySpawns;

            foreach (MapSpawn spawn in gs.spawns.Data)
            {
                if (gs.rand.NextDouble() > gs.data.GetGameData<SpawnSettings>().MapSpawnChance)
                {
                    continue;
                }

                int gx = GetIndexFromCoord(spawn.X, false);
                int gz = GetIndexFromCoord(spawn.Z, false);

                _objectGrid[gx, gz].Spawns.Add(spawn);
            }
        }

        protected virtual bool SpawnGridObjects(int gx, int gy)
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
                        SpawnObject(spawn);
                        totalSpawns++;
                    }
                }
                catch (Exception e)
                {
                    _gs.logger.Exception(e, "SpawnObject");
                }
                grid.SpawnedObjects = true;
            }
            return true;
        }

        public virtual MapObject SpawnObject(IMapSpawn spawn)
        {

            if (GetObject(spawn.MapObjectId, out MapObject currObj))
            {
                return currObj;
            }

            if (!GetFactory(spawn.EntityTypeId, out IMapObjectFactory fact))
            {
                return null;
            }

            MapObject obj = fact.Create(_gs, spawn);
            AfterSpawns(obj);
            if (obj != null)
            {
                AddObject(obj, spawn);
            }

            return obj;
        }

        protected void AfterSpawns(MapObject obj)
        {
            if (obj is Unit unit)
            {
                AIUpdate update = new AIUpdate();

                _messageService.SendMessage(_gs, unit, update, MathUtils.FloatRange(0, _gs.data.GetGameData<AISettings>().UpdateSeconds, _gs.rand));
            }
        }

        protected virtual void SendGridItemsToClient(Character ch, int gx, int gz)
        {
            if (SpawnGridObjects(gx, gz))
            {
                return;
            }
            List<MapObject> items = GetObjectsFromGrid(gx, gz);

            if (items.Count < 1)
            {
                return;
            }

            foreach (MapObject wo in items)
            {
                _messageService.SendMessage(_gs, ch, new OnSpawn(wo));
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

        protected void OnAddObjectToGrid(MapObject obj, int gx, int gz)
        {
            if (!(obj is Character ch))
            {
                return;
            }

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
                    SendGridItemsToClient(ch, x, z);
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

            if (_zoneDict.TryGetValue(obj.PrevZoneId, out Dictionary<string, Character> prevDict))
            {
                if (prevDict.TryGetValue(ch.Id, out Character prevChar))
                {
                    prevDict.Remove(ch.Id);
                }
            }
            if (_zoneDict.TryGetValue(obj.ZoneId, out Dictionary<string, Character> currDict))
            {
                currDict.Add(ch.Id, ch);
            }
        }
    }
}