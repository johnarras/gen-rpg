using MessagePack;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.MapMessages.Interfaces;
using Genrpg.Shared.MapObjects.Interfaces;
using Genrpg.Shared.Spawns.Interfaces;
using Genrpg.Shared.Utils.Data;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Genrpg.Shared.Networking.Interfaces;
using Genrpg.Shared.GameSettings.Entities;
using Genrpg.Shared.DataStores.Categories.PlayerData;

namespace Genrpg.Shared.MapObjects.Entities
{
    [MessagePackObject]
    public class MapObject : BasePlayerData, IMapObject
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public string Name { get; set; }
        [Key(2)] public long EntityTypeId { get; set; }
        [Key(3)] public long EntityId { get; set; }
        [Key(4)] public float X { get; set; }
        [Key(5)] public float Y { get; set; }
        [Key(6)] public float Z { get; set; }
        [Key(7)] public float Rot { get; set; }
        [Key(8)] public float Speed { get; set; }
        [JsonIgnore]
        [IgnoreMember] public long ZoneId { get; set; }
        [JsonIgnore]
        [IgnoreMember] public long PrevZoneId { get; set; }
        [Key(9)] public long Level { get; set; }
        [Key(10)] public long FactionTypeId { get; set; }

        [JsonIgnore]
        [IgnoreMember] public DateTime LastGridChange { get; set; }
        [JsonIgnore]
        [IgnoreMember] public float ToRot { get; set; }
        [JsonIgnore]
        [IgnoreMember] public float ToX { get; set; }
        [JsonIgnore]
        [IgnoreMember] public float ToZ { get; set; }
        [JsonIgnore]
        [IgnoreMember] public bool Moving { get; set; }

        [JsonIgnore]
        [IgnoreMember] public string TargetId { get; set; }


        [JsonIgnore]
        [IgnoreMember] public object OnActionLock = new object();

        [JsonIgnore]
        [IgnoreMember] public IMapApiMessage OnActionMessage { get; set; }
        [JsonIgnore]
        [IgnoreMember] public IMapApiMessage ActionMessage { get; set; }
        [JsonIgnore]
        [IgnoreMember] public IMapSpawn Spawn { get; set; }

        private bool _isDeleted { get; set; }

        public MapObject()
        {
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

        public virtual string GetGameDataName(string typeName)
        {
            return GameDataConstants.DefaultFilename;
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
        [JsonIgnore]
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

        public void CopyDataFromMapSpawn(IMapSpawn spawn)
        {
            Id = spawn.MapObjectId;
            X = spawn.X;
            Z = spawn.Z;
            Rot = 0;
            EntityTypeId = spawn.EntityTypeId;
            EntityId = spawn.EntityId;
            ZoneId = spawn.ZoneId;
            Speed = 0;
            Moving = false;
            FactionTypeId = spawn.FactionTypeId;

        }
        private bool _isDirty = false;
        public void SetDirty(bool val)
        {
            _isDirty = val;
        }

        public bool IsDirty()
        {
            return _isDirty;
        }

        public float DistanceTo(MapObject other)
        {
            float dx = X - other.X;
            float dz = Z - other.Z;

            return (float)Math.Sqrt(dx * dx + dz * dz);
        }

        public virtual void SetSessionOverrides(SessionOverrideList list) { }
    }
}
