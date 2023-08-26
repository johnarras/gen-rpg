using MessagePack;
using Genrpg.Shared.Spawns.Entities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using TColl = System.Collections.Generic.List<Genrpg.Shared.MapObjects.Entities.MapObject>;
//using TColl = System.Collections.Generic.HashSet<Entities.MapObjects.MapObject>;

namespace Genrpg.Shared.MapObjects.Entities
{

    [MessagePackObject]
    public class MapObjectGridData
    {
        [Key(0)] public int GX { get; set; }
        [Key(1)] public int GZ { get; set; }
        /// <summary>
        /// List of objects in a grid. This is handled in an immutable fashion so
        /// that it gets locked on Add and Remove, but any number of threads can read from it to get nearby objects.
        /// 
        /// Do we need a volatile field instead?
        /// </summary>
        protected TColl _objs { get; set; }
        [Key(2)] public List<MapSpawn> Spawns { get; set; }
        [Key(3)] public bool SpawnedObjects { get; set; }

        public object SpawnLock = new object();
        public object ObjsLock = new object();

        public volatile int PlayerCount;
        public MapObjectGridData()
        {
            _objs = new TColl();
            Spawns = new List<MapSpawn>();
            PlayerCount = 0;
        }

        public void AddObj(MapObject obj)
        {
            lock (ObjsLock)
            {
                TColl set = new TColl(_objs);
                set.Add(obj);
                _objs = set;
            }
        }

        public void RemoveObj(MapObject obj)
        {
            lock (ObjsLock)
            {
                TColl set = new TColl(_objs);
                set.Remove(obj);
                _objs = set;
            }
        }

        public TColl GetObjects()
        {
            return _objs;
        }
    }
}
