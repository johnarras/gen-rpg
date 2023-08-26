using MessagePack;
using Genrpg.Shared.DataStores.Categories;
using Genrpg.Shared.Entities.Utils;
using Genrpg.Shared.GameDatas;
using Genrpg.Shared.GameDatas.Interfaces;
using Genrpg.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Genrpg.Shared.Utils.Data
{
    [MessagePackObject]
    public class IndexedDataItemLookup
    {

        protected object _rootObject = null;
        private Dictionary<Type, Dictionary<long, IIdName>> _cache = null;
        private object _cacheLock = new object();
        public IndexedDataItemLookup(object parent)
        {
            _rootObject = parent;
        }

        public void Clear()
        {
            _cache = null;
        }

        public virtual object Get(Type type, long id)
        {
            if (type == null)
            {
                return null;
            }

            if (_cache == null)
            {
                SetupCache();
            }

            if (!_cache.ContainsKey(type))
            {
                return null;
            }
            Dictionary<long, IIdName> dict = _cache[type];
            if (!dict.ContainsKey(id))
            {
                return null;
            }
            return dict[id];

        }



        protected void SetupCache()
        {
            if (_cache != null)
            {
                return;
            }

            lock (_cacheLock)
            {
                if (_cache != null)
                {
                    return;
                }

                Dictionary<Type, Dictionary<long, IIdName>> tempCache = new Dictionary<Type, Dictionary<long, IIdName>>();
                Dictionary<Type, object> tempListCache = new Dictionary<Type, object>();
                string indexInterfacename = typeof(IIdName).Name;

                SetupCacheObjects(_rootObject, tempCache, tempListCache, indexInterfacename);
                _cache = tempCache;
            }
        }

        private void SetupCacheObjects(object obj, Dictionary<Type, Dictionary<long, IIdName>> tempCache,
            Dictionary<Type, object> tempListCache, string indexInterfaceName)
        {
            if (obj == null || obj.GetType().IsPrimitive)
            {
                return;
            }
            PropertyInfo[] props = obj.GetType().GetProperties();

            for (int p = 0; p < props.Length; p++)
            {
                PropertyInfo prop = props[p];

                Type ptype = prop.PropertyType;

                if (!ptype.IsGenericType ||
                    ptype.FullName.IndexOf("System.Collections.Generic.List`1") != 0 &&
                    ptype.FullName.IndexOf("System.Collections.Generic.List`1") != 0)
                {
                    if (ptype.FullName.IndexOf("System") < 0)
                    {
                        SetupCacheObjects(EntityUtils.GetObjectValue(obj, prop), tempCache, tempListCache, indexInterfaceName);
                    }
                    continue;
                }



                Type[] args = ptype.GetGenericArguments();
                if (args == null || args.Length != 1)
                {
                    continue;
                }

                if (!args[0].IsClass)
                {
                    continue;
                }

                Type inter = args[0].GetInterface(indexInterfaceName);
                if (inter == null)
                {
                    continue;
                }


                object listVal = EntityUtils.GetObjectValue(obj, prop);
                if (listVal == null)
                {
                    continue;
                }
                tempCache[args[0]] = new Dictionary<long, IIdName>();


                object countObj = EntityUtils.GetObjectValue(listVal, "Count");
                if (countObj == null)
                {
                    continue;
                }
                int count = 0;

                int.TryParse(countObj.ToString(), out count);
                if (count < 1)
                {
                    continue;
                }

                Type ltype = listVal.GetType();

                tempListCache[args[0]] = listVal;
                MethodInfo arrMethod = ltype.GetMethod("ToArray");
                if (arrMethod == null)
                {
                    continue;
                }

                object arr2 = arrMethod.Invoke(listVal, new object[0]);

                if (arr2 == null)
                {
                    continue;
                }

                Type arrType = arr2.GetType();

                MethodInfo readMethod = arrType.GetMethod("GetValue", new[] { typeof(int) });

                if (readMethod == null)
                {
                    continue;
                }


                for (int c = 0; c < count; c++)
                {
                    IIdName cval = readMethod.Invoke(arr2, new object[] { c }) as IIdName;
                    if (cval != null)
                    {
                        tempCache[args[0]][cval.IdKey] = cval;
                    }
                }
            }

            if (obj is GameData gameData)
            {
                foreach (IGameDataContainer cont in gameData.GetContainers())
                {
                    SetupCacheObjects(cont.GetData(), tempCache, tempListCache, indexInterfaceName);
                }
            }
        }



        public virtual T Get<T>(long id) where T : class, IIdName
        {
            Type type = typeof(T);
            return Get(type, id) as T;
        }

        public virtual T Get<T>(ulong id) where T : class, IIdName
        {
            return Get<T>((int)id);
        }

        public List<IIdName> GetList(string typeName)
        {
            if (_cache == null)
            {
                SetupCache();
            }


            foreach (Type t in _cache.Keys)
            {
                if (t.Name == typeName)
                {
                    return _cache[t].Values.ToList();
                }
            }
            return null;
        }

        public List<T> GetList<T>()
        {
            if (_cache == null)
            {
                SetupCache();
            }
            Type t = typeof(T);

            if (!_cache.ContainsKey(t))
            {
                return new List<T>();
            }

            Dictionary<long, IIdName> val = _cache[t];

            List<T> retval = new List<T>();

            foreach (IIdName iid in val.Values)
            {
                if (iid is T tItem)
                {
                    retval.Add(tItem);
                }
            }
            return retval;
        }
    }
}
