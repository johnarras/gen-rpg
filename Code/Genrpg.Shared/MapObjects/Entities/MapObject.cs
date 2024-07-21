using Genrpg.Shared.MapMessages.Interfaces;
using Genrpg.Shared.MapObjects.Interfaces;
using Genrpg.Shared.Spawns.Interfaces;
using Genrpg.Shared.Utils.Data;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Genrpg.Shared.Networking.Interfaces;
using Genrpg.Shared.GameSettings.PlayerData;
using Genrpg.Shared.MapObjects.MapObjectAddons.Entities;
using Genrpg.Shared.Pathfinding.Entities;
using Genrpg.Shared.Spells.Interfaces;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.Shared.Utils;
using Genrpg.Shared.DataStores.Entities;

namespace Genrpg.Shared.MapObjects.Entities
{
    // MessagePackIgnore
    public class MapObject : IMapObject, IDisposable
    {
        protected IRepositoryService _repoService;

        public string Id { get; set; }
        public string Name { get; set; }
        public long EntityTypeId { get; set; }
        public long EntityId { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        public float Rot { get; set; }
        public float Speed { get; set; }

        public long ZoneId { get; set; }
        public string LocationId { get; set; }
        public string LocationPlaceId { get; set; }

        public long PrevZoneId { get; set; }
        public long Level { get; set; }
        public long FactionTypeId { get; set; }
        public long AddonBits { get; set; }

        public DateTime LastGridChange { get; set; }

        public float FinalRot { get; set; }

        public float FinalX { get; set; } = -1;

        public float FinalZ { get; set; } = -1;

        public WaypointList Waypoints { get; private set; } = new WaypointList();

        public List<IDisplayEffect> Effects { get; set; } = new List<IDisplayEffect>();

        public SmallIndexBitList StatusEffects { get; set; } = new SmallIndexBitList();

        public GameDataOverrideList DataOverrides { get; set; } = new GameDataOverrideList();

        public MapObject(IRepositoryService repositoryService)
        {
            _repoService = repositoryService;
        }

        protected int _idHash { get; set; } = -1;
        public int GetIdHash()
        {
            if (_idHash < 0)
            {
                _idHash = StrUtils.GetIdHash(Id);
            }
            return _idHash;
        }

        /// <summary>
        /// For some reason using a ConcurrentBag here slows things down the way I am using it. Idk, it works
        /// but it's not really the proper data structure.
        /// </summary>
        private ConcurrentQueue<MapMessagePackage> _packageCache = new ConcurrentQueue<MapMessagePackage>();

        public MapMessagePackage TakePackage()
        {
            if (_packageCache.TryDequeue(out MapMessagePackage package))
            {
                return package;
            }
            return new MapMessagePackage();
        }

        public void ReturnPackage(MapMessagePackage package)
        {
            package.Clear();           
            _packageCache.Enqueue(package);
        }

        public void AddEffect(IDisplayEffect effect)
        {
            if (effect.EntityTypeId == EntityTypes.StatusEffect)
            {
                StatusEffects.SetBit(effect.EntityId);
            }
        }
        public virtual void Dispose()
        {
            _messageCache.Clear();
            OnActionMessage = null;
            Waypoints.Clear();
            Effects.Clear();
        }

        public void RemoveEffect(IDisplayEffect effect)
        {
            if (!Effects.Contains(effect))
            {
                return;
            }

            lock (this)
            {
                Effects.Remove(effect);
            }

            if (effect.EntityTypeId == EntityTypes.StatusEffect)
            {
                if (!Effects.Any(x=>x.EntityTypeId == EntityTypes.StatusEffect && x.EntityId == effect.EntityId))
                {
                    StatusEffects.RemoveBit(effect.EntityId);
                }
            }
        }

        public void RemoveStatusBit(long statusBitId)
        {
            StatusEffects.RemoveBit(statusBitId);
            if (Effects.Any(x=>x.EntityTypeId == EntityTypes.StatusEffect && x.EntityId == statusBitId))
            {
                lock (this)
                {
                    Effects = Effects.Where(x => x.EntityTypeId != EntityTypes.StatusEffect || x.EntityId != statusBitId).ToList();
                }
            }
        }

        public float GetNextXPos()
        {
            if (Waypoints != null && Waypoints.Waypoints.Count > 0)
            {
                return Waypoints.Waypoints[0].X;
            }
            return FinalX;
        }

        public float GetNextZPos()
        {
            if (Waypoints != null && Waypoints.Waypoints.Count > 0)
            {
                return Waypoints.Waypoints[0].Z;
            }
            return FinalZ;
        }

        public bool Moving { get; set; }
        
        public string TargetId { get; set; }
        
        public object OnActionLock = new object();
        
        public IMapApiMessage OnActionMessage { get; set; }
        
        public IMapApiMessage ActionMessage { get; set; }
        
        public IMapSpawn Spawn { get; set; }
        
        private bool _isDeleted { get; set; }

        private bool _isDirty = false;
        public bool IsDirty() { return _isDirty; }
        public void SetDirty(bool dirty) { _isDirty = dirty; }

        public TAddon GetAddon<TAddon>() where TAddon : IMapObjectAddon
        {
            return (TAddon)Spawn?.GetAddons()?.FirstOrDefault(x => x.GetType() == typeof(TAddon));
        }

        public List<IMapObjectAddon> GetAddons()
        {
            return Spawn?.GetAddons() ?? new List<IMapObjectAddon>();
        }

        public bool HasAddon(long addonTypeId)
        {
            return (AddonBits & (long)(1 << (int)addonTypeId)) != 0;
        }

        public bool HasTarget()
        {
            return !string.IsNullOrEmpty(TargetId);
        }

        public bool IsDeleted()
        {
            return _isDeleted;
        }

        public void SetDeleted(bool val)
        {
            _isDeleted = val;
        }

        protected ConcurrentDictionary<Type, object> _messageCache = new ConcurrentDictionary<Type, object>();
        public virtual T GetCachedMessage<T>(bool unCancel) where T : IMapApiMessage, new()
        {
            if (_messageCache.TryGetValue(typeof(T), out object message))
            {
                T tcurr = (T)message;
                if (unCancel)
                {
                    tcurr.SetCancelled(false);
                }
                return tcurr;
            }
            T t = new T();
            _messageCache.TryAdd(typeof(T), t);
            return t;
        }

        public virtual bool IsPlayer() { return false; }
        public virtual bool IsUnit() { return false; }
        public virtual bool IsGroundObject() { return false; }

        // This exists here, but it is only set up for players for now
        
        protected IConnection _conn = null;
        public virtual void AddMessage(IMapApiMessage message)
        {
            IConnection conn = _conn;
            if (conn != null)
            {
                conn.AddMessage(message);
            }
        }

        public MyPointF GetPos()
        {
            return new MyPointF(X, Y, Z);
        }

        public void CopyDataToMapObjectFromMapSpawn(IMapSpawn spawn)
        {
            Id = spawn.ObjId;
            X = spawn.X;
            Z = spawn.Z;
            Rot = spawn.Rot;
            EntityTypeId = spawn.EntityTypeId;
            EntityId = spawn.EntityId;
            ZoneId = spawn.ZoneId;
            LocationId = spawn.LocationId;
            LocationPlaceId = spawn.LocationPlaceId;
            Speed = 0;
            Moving = false;
            FactionTypeId = spawn.FactionTypeId;
            AddonBits = spawn.GetAddonBits();
            if (!string.IsNullOrEmpty(spawn.Name))
            {
                Name = spawn.Name;
            }
        }

        public float DistanceTo(MapObject other)
        {
            float dx = X - other.X;
            float dz = Z - other.Z;

            return (float)Math.Sqrt(dx * dx + dz * dz);
        }


    }
}
