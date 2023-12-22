using Genrpg.Shared.Constants;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.Entities.Interfaces;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Entities.Utils;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Utils.Data;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Genrpg.Shared.Entities.Settings;
using MessagePack.Formatters;
using Genrpg.Shared.ProcGen.Settings.Names;

namespace Genrpg.Shared.Reflection.Services
{

    public interface IReflectionService : IService
    {
        bool MemberIsMultiType(MemberInfo mem);
        bool IsMultiType(Type type);
        bool MemberIsGenericList(MemberInfo mem);
        bool IsGenericList(Type mtype);
        Dictionary<K, T> SetupDictionary<K, T>(GameState gs, List<string> extraAssemblies = null) where T : ISetupDictionaryItem<K>;
        object GetObjectValue(object obj, string name);
        List<Type> GetTypesImplementing(Type interfaceType);
        void InitializeObjectData(object data);
        bool MemberIsPrimitive(MemberInfo mem);
        List<MemberInfo> GetMembers(object obj);
        Type GetMemberType(MemberInfo mem);
        List<NameValue> CreateDataList(GameState gs, string listName);
        string GetMemberName(object parent, object child);
        List<NameValue> GetDropdownList(GameState gs, MemberInfo mem, object obj);
        void SetObjectValue(object obj, string name, object val);
        object AddItems(object obj, object parent, IRepositorySystem repoSystem, out List<object> newItems,
            object copyFrom = null, int maxCount = 0);
        object GetItemWithId(object list, int id, string idMemberName = GameDataConstants.IdKey);
        object GetLastItem(object list);
        IList CreateGenericList(Type type);
        MemberInfo GetMemberInfo(object obj, string name);
        string GetIdString(string txt);
        Type GetUnderlyingType(object obj);
        string GetOnClickDropdownName(GameState gs, object obj, MemberInfo mem);
        object DeleteItem(object obj, object parent, object item);
        IEnumerable SortOnParameter(IEnumerable elist, bool ascending = true);
        void ReplaceIndexedItems(GameState gs, object list, List<IIdName> newList);
        object GetItemWithIndex(object list, int index);
        object GetObjectValue(object obj, MemberInfo mem);
        void SetObjectValue(object obj, MemberInfo mem, object val);
    }

    public class ReflectionService : IReflectionService
    {
        public bool MemberIsMultiType(MemberInfo mem)
        {
            return IsMultiType(GetMemberType(mem));
        }

        public bool IsMultiType(Type type)
        {
            if (type == null)
            {
                return false;
            }

            if (type.IsArray || type.GetMethod("ToArray") != null || type.GetMethod("ToList") != null)
            {
                return true;
            }

            return false;
        }

        public bool MemberIsGenericList(MemberInfo mem)
        {
            return IsGenericList(GetMemberType(mem));
        }

        public bool IsGenericList(Type mtype)
        {
            return mtype != null &&
                mtype.IsGenericType &&
                mtype.GetGenericArguments().Length > 0 &&
                (mtype.FullName.IndexOf("System.Collections.Generic.List`1") == 0 ||
                mtype.FullName.IndexOf("System.Collections.Generic.List`1") == 0);
        }

        public bool MemberIsArray(MemberInfo mem)
        {
            return IsArray(GetMemberType(mem));
        }

        public bool IsArray(Type mtype)
        {
            return mtype != null && mtype.IsArray;
        }

        public bool MemberIsPrimitive(MemberInfo mem)
        {
            return IsPrimitiveType(GetMemberType(mem));
        }

        public bool IsPrimitiveType(Type type)
        {

            if (type == null)
            {
                return false;
            }

            if (IsMultiType(type))
            {
                return false;
            }

            if (type.IsPrimitive || type.IsValueType || type.IsEnum || type.FullName == "System.String" || type.FullName == "System.Guid" ||
                type.FullName == "System.DateTime")
            {
                return true;
            }

            return false;
        }
        public MemberInfo GetMemberInfo(object obj, string name)
        {
            return EntityUtils.GetMemberInfo(obj, name);
        }

        public object GetObjectValue(object obj, string name)
        {
            return EntityUtils.GetObjectValue(obj, name);

        }

        public Type GetUnderlyingType(object obj)
        {
            return EntityUtils.GetUnderlyingType(obj);
        }


        public object GetObjectValue(object obj, MemberInfo mem)
        {
            return EntityUtils.GetObjectValue(obj, mem);
        }

        public void SetObjectValue(object obj, string name, object val)
        {
            EntityUtils.SetObjectValue(obj, name, val);
        }


        public void SetObjectValue(object obj, MemberInfo mem, object val)
        {
            EntityUtils.SetObjectValue(obj, mem, val);
        }


        public bool HasMethod(object obj, string name)
        {
            if (obj == null || string.IsNullOrEmpty(name))
            {
                return false;
            }

            List<MethodInfo> methods = GetMethods(obj);

            if (methods == null)
            {
                return false;
            }

            foreach (MethodInfo meth in methods)
            {
                if (meth.Name == name)
                {
                    return true;
                }
            }

            return false;
        }

        public List<MethodInfo> GetMethods(object obj)
        {
            if (obj == null)
            {
                return new List<MethodInfo>();
            }

            Type type = obj as Type;
            if (type == null)
            {
                type = obj.GetType();
            }

            MethodInfo[] methods = type.GetMethods();

            List<MethodInfo> list = new List<MethodInfo>();

            foreach (MethodInfo meth in methods)
            {
                list.Add(meth);
            }
            return list;
        }

        public Type GetMemberType(MemberInfo mem)
        {
            return EntityUtils.GetMemberType(mem);
        }


        public List<MemberInfo> GetMembers(object obj)
        {
            if (obj == null)
            {
                return new List<MemberInfo>();
            }

            List<MemberInfo> list = new List<MemberInfo>();
            Type type = obj as Type;
            if (type == null)
            {
                type = obj.GetType();
            }


            PropertyInfo[] publicProps = type.GetProperties();
            PropertyInfo[] privateProps = type.GetProperties(BindingFlags.Instance | BindingFlags.NonPublic);

            List<PropertyInfo> allProps = publicProps.Concat(privateProps).ToList();

            for (int p = 0; p < allProps.Count; p++)
            {
                if (Attribute.IsDefined(allProps[p], typeof(JsonIgnoreAttribute)))
                {
                    continue;
                }
                list.Add(allProps[p]);
            }
            FieldInfo[] publicFields = type.GetFields();
            FieldInfo[] privateFields = type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic);
            List<FieldInfo> allFields = publicFields.Concat(privateFields).ToList();
            for (int f = 0; f < allFields.Count; f++)
            {
                if (Attribute.IsDefined(allFields[f], typeof(JsonIgnoreAttribute)))
                {
                    continue;
                }
                list.Add(allFields[f]);
            }

            list = ListUtils.OrderOn(list, "Name");

            MemberInfo idval = null;

            foreach (MemberInfo member in list)
            {
                if (member.Name == GameDataConstants.IdKey)
                {
                    list.Remove(member);
                    list.Insert(0, member);
                    break;
                }
            }

            if (idval != null)
            {
                list.Remove(idval);
                list.Insert(0, idval);
            }
            return list;
        }

        protected List<NameValue> CreateEnumList(Type type)
        {
            List<NameValue> retval = new List<NameValue>();
            if (type == null || !type.IsEnum)
            {
                return retval;
            }

            Array values = Enum.GetValues(type);

            foreach (object val in values)
            {
                retval.Add(new NameValue() { Name = val.ToString(), IdKey = (int)val });
            }
            return retval;
        }

        public int GetIdValue(object obj)
        {
            if (obj == null)
            {
                return 0;
            }

            object idObj = GetObjectValue(obj, GameDataConstants.IdKey);
            if (idObj == null)
            {
                return 0;
            }

            int num = 0;
            int.TryParse(idObj.ToString(), out num);
            return num;
        }


        public string GetIdString(string txt)
        {
            if (string.IsNullOrEmpty(txt))
            {
                return "";
            }


            int idx = txt.IndexOf("[#");
            if (idx < 0)
            {
                return txt;
            }

            int idx2 = txt.IndexOf("]");
            if (idx2 < 0 || idx2 <= idx)
            {
                return txt;
            }

            string numStr = txt.Substring(idx + 2, idx2 - idx - 2);

            return numStr;

        }


        public object CreateGenericArray(Type type, int size = 0)
        {
            if (size < 0)
            {
                size = 0;
            }

            if (type == null)
            {
                return new object[size];
            }

            if (type.IsArray)
            {
                type = GetUnderlyingType(type);
            }

            if (type == null)
            {
                return new object[size];
            }

            Array arr = Array.CreateInstance(type, size);
            return arr;
        }

        public IList CreateGenericList(Type type)
        {
            if (type == null)
            {
                return new List<object>();
            }

            if (IsGenericList(type))
            {
                type = GetUnderlyingType(type);
            }

            if (type == null)
            {
                return new List<object>();
            }

            Type listType = typeof(List<>);
            Type[] args = new Type[] { type };
            Type type2 = listType.MakeGenericType(args);

            object obj = EntityUtils.DefaultConstructor(type2);
            return (IList)obj;
        }

        public object CreateObjectOfType(Type type)
        {
            if (type == null || type.IsArray || type.IsGenericType)
            {
                return null;
            }

            object obj = EntityUtils.DefaultConstructor(type);
            return obj;
        }


        public void InitializeObjectData(object data)
        {
            if (data == null)
            {
                return;
            }

            List<MemberInfo> members = GetMembers(data);

            foreach (MemberInfo mem in members)
            {
                Type mtype = GetMemberType(mem);
                object mval = GetObjectValue(data, mem.Name);
                if (mval != null)
                {
                    continue;
                }

                if (mtype.IsArray)
                {
                    SetObjectValue(data, mem.Name, CreateGenericArray(mtype));
                }
                else if (IsGenericList(mtype))
                {
                    Type mtype2 = GetUnderlyingType(mtype);
                    SetObjectValue(data, mem.Name, CreateGenericList(mtype2));

                }
                else if (!MemberIsPrimitive(mem))
                {
                    SetObjectValue(data, mem.Name, CreateObjectOfType(mtype));
                }
            }

        }

        public MemberInfo FindParentMemberWithValue(object parent, object obj)
        {
            if (parent == null || obj == null)
            {
                return null;
            }


            List<MemberInfo> members = GetMembers(parent);

            foreach (MemberInfo mem in members)
            {
                object obj2 = GetObjectValue(parent, mem);

                if (obj2 == obj)
                {
                    return mem;
                }
            }

            return null;
        }



        public string GetMemberName(object parent, object child)
        {
            if (parent == null || child == null)
            {
                return "";
            }

            if (parent is IEnumerable parentEnum &&
                child.GetType().GenericTypeArguments.Length > 0)
            {
                return "List<" + child.GetType().GenericTypeArguments[0].Name + ">";
            }

            if (child is IEnumerable childEnum &&
                child.GetType().GenericTypeArguments.Length > 0)
            {
                return "List<" + child.GetType().GenericTypeArguments[0].Name + ">";
            }
            


            List<MemberInfo> members = GetMembers(parent);

            if (members == null)
            {
                return "";
            }

            foreach (MemberInfo member in members)
            {
                object val = GetObjectValue(parent, member);
                if (val == child)
                {
                    return member.Name;
                }
            }
            return "";
        }

        public bool AddItem(object collection, object parent, object itemToAdd, IRepositorySystem repoSystem)
        {
            if (collection == null || parent == null || itemToAdd == null)
            {
                return false;
            }

            Type objType = collection.GetType();
            Type underlyingType = GetUnderlyingType(objType);

            if (underlyingType == null || !IsGenericList(objType))
            {
                return false;
            }


            MethodInfo addMethod = objType.GetMethod("Add", new[] { underlyingType });
            if (addMethod == null)
            {
                return false;
            }
            addMethod.Invoke(collection, new[] { itemToAdd });

            return true;
        }

        public object AddItems(object obj, object parent, IRepositorySystem repoSystem, out List<object> newItems,
            object copyFrom = null, int maxCount = 0)
        {
            newItems = new List<object>();

            if (obj == null || parent == null)
            {
                return obj;
            }

            Type objType = obj.GetType();

            int numToAdd = 1;
            int currSize = 0;
            object newObj = null;
            if (objType.IsArray)
            {
                currSize = (int)GetObjectValue(obj, "Length");
            }
            else if (IsGenericList(objType))
            {
                currSize = (int)GetObjectValue(obj, "Count");
            }
            else
            {
                return obj;
            }

            long smallestIdAllowed = 0;
            long largestIdAllowed = GameData.IdBlockSize;

            if (copyFrom != null)
            {
                object IdStr = GetObjectValue(copyFrom, GameDataConstants.IdKey);
                if (IdStr != null)
                {
                    long copyFromId = 0;
                    long.TryParse(IdStr.ToString(), out copyFromId);
                    if (copyFromId > 0)
                    {
                        smallestIdAllowed = copyFromId;
                        smallestIdAllowed -= smallestIdAllowed % GameData.IdBlockSize;
                        largestIdAllowed = smallestIdAllowed + GameData.IdBlockSize;
                    }
                }
            }

            int newSize = currSize + numToAdd;

            Type underlyingType = GetUnderlyingType(objType);

            if (underlyingType == null)
            {
                return obj;
            }

            if (objType.IsArray)
            {
                newObj = CreateGenericArray(objType, newSize);

                if (newObj == null)
                {
                    return obj;
                }

                Type newType = newObj.GetType();

                MethodInfo readMethod = objType.GetMethod("GetValue", new[] { typeof(int) });
                MethodInfo writeMethod = newType.GetMethod("SetValue", new[] { underlyingType, typeof(int) });

                long largestId = 0;

                if (largestIdAllowed > 0)
                {
                    largestId = largestIdAllowed;
                }
                if (readMethod != null && writeMethod != null)
                {
                    for (int c = 0; c < currSize; c++)
                    {
                        object oldval = readMethod.Invoke(obj, new object[] { c });
                        int newId = GetIdValue(oldval);
                        if (newId > largestId && newId >= smallestIdAllowed && newId < largestIdAllowed)
                        {
                            largestId = newId;
                        }
                        writeMethod.Invoke(newObj, new object[] { oldval, c });
                    }

                    for (int c = currSize; c < newSize; c++)
                    {
                        object obj2 = null;
                        if (copyFrom == null)
                        {
                            obj2 = CreateObjectOfType(underlyingType);
                        }
                        else
                        {
                            obj2 = SerializationUtils.SafeMakeCopy(copyFrom);
                            if (copyFrom is IComplexCopy complexFrom && obj2 is IComplexCopy complexObj)
                            {
                                complexObj.DeepCopyFrom(complexFrom);
                            }
                        }
                        largestId++;
                        SetObjectValue(obj2, GameDataConstants.IdKey, largestId);
                        newItems.Add(obj2);
                        writeMethod.Invoke(newObj, new object[] { obj2, c });
                    }

                }

                MemberInfo parentMember = FindParentMemberWithValue(parent, obj);

                if (parentMember != null)
                {
                    SetObjectValue(parent, parentMember, newObj);
                }
                return newObj;
            }
            else if (IsGenericList(objType))
            {
                MethodInfo arrMethod = objType.GetMethod("ToArray");
                if (arrMethod == null)
                {
                    return obj;
                }

                object arr2 = arrMethod.Invoke(obj, new object[0]);

                if (arr2 == null)
                {
                    return obj;
                }

                Type arrType = arr2.GetType();

                MethodInfo readMethod = arrType.GetMethod("GetValue", new[] { typeof(int) });

                int largestId = 0;

                if (readMethod != null)
                {
                    for (int c = 0; c < currSize; c++)
                    {
                        object oldval = readMethod.Invoke(arr2, new object[] { c });
                        int newId = GetIdValue(oldval);
                        if (newId > largestId && newId >= smallestIdAllowed && newId < largestIdAllowed)
                        {
                            largestId = newId;
                        }
                    }
                }

                MethodInfo addMethod = objType.GetMethod("Add", new[] { underlyingType });
                if (addMethod != null)
                {
                    for (int c = 0; c < numToAdd; c++)
                    {
                        object obj2 = null;
                        if (copyFrom == null)
                        {
                            obj2 = CreateObjectOfType(underlyingType);
                        }
                        else
                        {
                            obj2 = SerializationUtils.SafeMakeCopy(copyFrom);

                            if (copyFrom is IComplexCopy complexFrom && obj2 is IComplexCopy complexObj)
                            {
                                complexObj.DeepCopyFrom(complexFrom);
                            }
                        }
                        newItems.Add(obj2);
                        largestId++;
                        try
                        {
                            SetObjectValue(obj2, GameDataConstants.IdKey, largestId);
                        }
                        catch (Exception)
                        {
                            // Need this for the case where the Id is a string.
                        }
                        addMethod.Invoke(obj, new[] { obj2 });

                    }


                }
                return obj;

            }
            else
            {
                return obj;
            }

        }

        public void ReplaceIndexedItems(GameState gs, object list, List<IIdName> newList)
        {
            if (list == null || newList == null)
            {
                return;
            }

            Type ltype = list.GetType();
            Type utype = GetUnderlyingType(ltype);
            if (utype == null)
            {
                return;
            }
            MethodInfo cmethod = ltype.GetMethod("Clear");
            MethodInfo amethod = ltype.GetMethod("Add", new[] { utype });
            if (cmethod == null || amethod == null)
            {
                return;
            }
            cmethod.Invoke(list, new object[0]);

            foreach (IIdName item in newList)
            {
                amethod.Invoke(list, new[] { item });
            }

        }

        public object DeleteItem(object obj, object parent, object item)
        {
            if (obj == null || parent == null || item == null)
            {
                return obj;
            }

            Type objType = obj.GetType();

            int currSize = 0;
            object newObj = null;
            if (objType.IsArray)
            {
                currSize = (int)GetObjectValue(obj, "Length");
            }
            else if (IsGenericList(objType))
            {
                currSize = (int)GetObjectValue(obj, "Count");
            }
            else
            {
                return obj;
            }

            int newSize = currSize - 1;

            if (newSize < 0)
            {
                return obj;
            }

            Type underlyingType = GetUnderlyingType(objType);

            if (underlyingType == null)
            {
                return obj;
            }

            if (objType.IsArray)
            {
                newObj = CreateGenericArray(objType, newSize);

                if (newObj == null)
                {
                    return obj;
                }

                Type newType = newObj.GetType();

                MethodInfo readMethod = objType.GetMethod("GetValue", new[] { typeof(int) });
                MethodInfo writeMethod = newType.GetMethod("SetValue", new[] { underlyingType, typeof(int) });

                if (readMethod != null && writeMethod != null)
                {
                    int readPos = 0;
                    int writePos = 0;
                    for (int c = 0; c < currSize; c++)
                    {
                        object oldval = readMethod.Invoke(obj, new object[] { readPos });
                        readPos++;
                        if (oldval == item)
                        {
                            continue;
                        }

                        writeMethod.Invoke(newObj, new object[] { oldval, writePos });
                        writePos++;
                    }
                }

                MemberInfo parentMember = FindParentMemberWithValue(parent, obj);

                if (parentMember != null)
                {
                    SetObjectValue(parent, parentMember, newObj);
                }
                return newObj;
            }
            else if (IsGenericList(objType))
            {
                MethodInfo removeMethod = objType.GetMethod("Remove", new[] { underlyingType });
                if (removeMethod == null)
                {
                    return obj;
                }

                removeMethod.Invoke(obj, new[] { item });

                return obj;
            }
            else
            {
                return obj;
            }
        }


        public void AddOrUpdateItem<T>(List<T> list, T t) where T : IIdName, new()
        {
            if (list == null || t == null)
            {
                return;
            }

            bool foundOldItem = false;
            int largerIdIndex = -1;
            for (int i = 0; i < list.Count; i++)
            {
                T olditem = list[i];
                if (olditem.IdKey == t.IdKey)
                {
                    if (string.IsNullOrEmpty(olditem.Name))
                    {
                        olditem.Name = t.Name;
                    }

                    foundOldItem = true;
                }
                if (olditem.IdKey > t.IdKey && largerIdIndex < 0)
                {
                    largerIdIndex = i;
                }
            }

            if (!foundOldItem)
            {
                if (largerIdIndex < 0)
                {
                    list.Add(t);
                }
                else
                {
                    list.Insert(largerIdIndex, t);
                }
            }

        }

        public List<NameValue> GetConstantsList(Type t)
        {
            List<NameValue> retval = new List<NameValue>();

            List<int> ints = GetStaticInts(t);
            if (ints == null)
            {
                return retval;
            }

            foreach (int item in ints)
            {
                string name = GetStaticNameFromValue(t, item);
                if (string.IsNullOrEmpty(name))
                {
                    continue;
                }

                NameValue nv = new NameValue() { IdKey = item, Name = name };
                retval.Add(nv);
            }
            return retval;
        }

        public List<T> GenDataList<S, T>(List<T> list)
            where T : IIdName, new()
            where S : T, IIdName, new()
        {
            if (list == null)
            {
                list = new List<T>();
            }
            List<int> ints = GetStaticInts(typeof(S));
            if (ints == null)
            {
                return list;
            }

            List<T> list2 = new List<T>();
            foreach (int item in ints)
            {
                string name = GetStaticNameFromValue(typeof(S), item);
                if (string.IsNullOrEmpty(name))
                {
                    continue;
                }

                // Don't put the name for the max value in there if there is one.
                if (name == "Max" || name == "MaxSize")
                {
                    continue;
                }
                // Make a T here for consistency.
                // If you really need more data in a derived class 
                // in a game, make a new list maybe and access
                // that within special services.
                T t = new T();
                t.IdKey = item;
                t.Name = name;
                AddOrUpdateItem(list, t);
            }
            return list;
        }
        public IDictionary<long, IEntityHelper> _entityDict = null;
        public string GetDataTableName(GameState gs, object obj)
        {
            if (gs.data == null || obj == null || string.IsNullOrEmpty(obj.ToString()))
            {
                return "";
            }

            if (obj.ToString() == "Entity")
            {
                return null;
            }


            int entityTypeId = 0;
            string etypeName = obj.ToString();
            int.TryParse(obj.ToString(), out entityTypeId);
            string tableName = StrUtils.MakePlural(etypeName);

            if (entityTypeId > 0)
            {
                EntityType etype = gs.data.GetGameData<EntitySettings>(null).GetEntityType(entityTypeId);
                if (etype != null && !string.IsNullOrEmpty(etype.Name))
                {
                    etypeName = etype.Name;
                }
                if (_entityDict == null)
                {
                    _entityDict = SetupDictionary<long, IEntityHelper>(gs);
                }
                if (_entityDict.ContainsKey(entityTypeId))
                {
                    tableName = _entityDict[entityTypeId].GetDataPropertyName();
                }
            }

            List<IIdName> list = gs.data.GetList(etypeName);

            if (list != null && list.Count > 0)
            {
                return etypeName;
            }

            list = gs.data.GetList(tableName);
            if (list != null && list.Count > 0)
            {
                return tableName;
            }

            do
            {
                MemberInfo mem = GetMemberInfo(gs.data, tableName);
                if (mem != null)
                {
                    return tableName;
                }

                tableName = tableName.Substring(1);
            }
            while (tableName.Length > 1);



            if (etypeName != null && etypeName != "Zone" && etypeName.IndexOf("Type") < 0)
            {
                return GetDataTableName(gs, etypeName + "Type");
            }

            List<IIdName> items = gs.data.GetList(etypeName);


            return items != null && items.Count > 0 ? etypeName : "";

        }


        public string GetStaticNameFromValue(Type t, object val)
        {
            if (t == null || val == null)
            {
                return "";
            }

            FieldInfo[] fields = t.GetFields();
            foreach (FieldInfo field in fields)
            {
                if (!field.IsStatic)
                {
                    continue;
                }

                object fieldval = field.GetValue(null);
                if (fieldval == null)
                {
                    continue;
                }

                if (fieldval.ToString() == val.ToString())
                {
                    return field.Name;
                }
            }
            if (t.BaseType != typeof(object))
            {
                return GetStaticNameFromValue(t.BaseType, val);
            }
            return "";
        }

        public List<int> GetStaticInts(Type t)
        {
            List<int> list = new List<int>();
            if (t == null)
            {
                return list;
            }

            FieldInfo[] fields = t.GetFields();
            foreach (FieldInfo field in fields)
            {

                if (!field.IsStatic ||
                    field.FieldType.FullName != "System.Int32" && field.FieldType.FullName != "System.Int64")
                {
                    continue;
                }

                object intval = field.GetValue(null);
                if (intval != null)
                {
                    int val = -1;
                    int.TryParse(intval.ToString(), out val);
                    if (val >= 0)
                    {
                        list.Add(val);
                    }
                }
            }
            if (t.BaseType != typeof(object))
            {
                List<int> list2 = GetStaticInts(t.BaseType);
                if (list2.Count > 0)
                {
                    foreach (int item in list2)
                    {
                        list.Add(item);
                    }
                }
            }
            return list;
        }

        public List<string> GetStaticStrings(Type t)
        {
            return EntityUtils.GetStaticStrings(t);
        }

        public string GetObjId(object obj, string IdName = GameDataConstants.IdKey)
        {
            return EntityUtils.GetObjId(obj, IdName);
        }

        public List<NameValue> CreateDataList(GameState gs, string listName)
        {
            List<NameValue> list = new List<NameValue>();
            if (gs.data == null || string.IsNullOrEmpty(listName))
            {
                return list;
            }

            List<IIdName> itemList = gs.data.GetList(listName);

            if (itemList == null || itemList.Count < 1)
            {
                return list;
            }

            itemList = itemList.OrderBy(x => x.IdKey).ToList();

            foreach (IIdName item in itemList)
            {
                list.Add(new NameValue() { IdKey = item.IdKey, Name = item.Name, });
            }

            return list;
        }

        public List<NameValue> CreateDataList(GameState gs, object prop)
        {
            List<NameValue> list = new List<NameValue>();

            if (prop == null)
            {
                return list;
            }

            Type propType = prop.GetType();

            MethodInfo arrMethod = propType.GetMethod("ToArray");

            if (arrMethod == null)
            {
                return list;
            }

            object prop2 = arrMethod.Invoke(prop, new object[0]);

            if (prop2 == null)
            {
                return list;
            }

            Type propType2 = prop2.GetType();

            int length = (int)GetObjectValue(prop2, "Length");

            MethodInfo readMethod = propType2.GetMethod("GetValue", new[] { typeof(int) });

            if (readMethod == null)
            {
                return list;
            }


            NameValue zeroValue = new NameValue() { IdKey = 0, Name = "   [#0] None" };
            list.Add(zeroValue);

            for (int i = 0; i < length; i++)
            {
                object val = readMethod.Invoke(prop2, new object[] { i });
                if (val == null)
                {
                    continue;
                }

                int id = 0;
                object idObj = GetObjectValue(val, GameDataConstants.IdKey);
                if (idObj == null)
                {
                    continue;
                }

                int.TryParse(idObj.ToString(), out id);
                object nameObj = GetObjectValue(val, "Name");
                if (nameObj == null)
                {
                    nameObj = "";
                }

                string name = nameObj.ToString();
                if (id == 0)
                {
                    list.Remove(zeroValue);
                }

                string prefix = "";
                if (id < 10)
                {
                    prefix += " ";
                }

                if (id < 100)
                {
                    prefix += " ";
                }

                if (id < 1000)
                {
                    prefix += " ";
                }

                NameValue nv1 = new NameValue() { IdKey = id, Name = prefix + "[#" + id + "] " + name };
                NameValue nv2 = new NameValue() { IdKey = id, Name = name + " [#" + id + "]" };

                list.Add(nv1);
                list.Add(nv2);

            }

            list = ListUtils.OrderOn(list, "Name");
            return list;
        }

        public string GetOnClickDropdownName(GameState gs, object obj, MemberInfo mem)
        {
            if (obj == null || mem == null)
            {
                return "";
            }

            Type mType = GetMemberType(mem);

            string dataListName = GetGameDataListName(gs, obj, mem);
            if (string.IsNullOrEmpty(dataListName))
            {
                return "";
            }

            object prop = GetObjectValue(gs.data, dataListName);
            if (prop == null)
            {
                return "";
            }

            return dataListName;
        }


        // Get the list used int the game object to that's put into this member
        // at this time. It's either of the form XYZFooId for an item of type Foo,
        // or it's XYZEntityId where XYZEntityTypeId is of type EntityTypes.Foo.
        protected string GetGameDataListName(GameState gs, object obj, MemberInfo mem)
        {
            if (gs.data == null || obj == null || mem == null || string.IsNullOrEmpty(mem.Name))
            {
                return "";
            }

            Type mType = GetMemberType(mem);

            if (mType == null || mType.FullName != "System.Int32" && mType.FullName != "System.Int64")
            {
                return "";
            }


            string nm = mem.Name;
            string subs = nm.Substring(nm.Length - 2);

            // Check for XYZFooId.
            if (nm != null && nm.Length >= 2 && nm.Substring(nm.Length - 2) == GameDataConstants.IdSuffix)
            {
                string currname = nm.Substring(0, nm.Length - 2);

                while (currname.Length > 0)
                {
                    string nm2 = GetDataTableName(gs, currname);

                    object prop = GetObjectValue(gs, nm2);
                    if (prop != null)
                    {
                        return nm2;
                    }

                    currname = currname.Substring(1);
                }
            }

            nm = mem.Name;
            if (nm.IndexOf("EntityId") >= 0)
            {
                string etypeName = nm.Replace("EntityId", "EntityTypeId");

                MemberInfo etypeMem = GetMemberInfo(obj, etypeName);
                if (etypeMem == null)
                {
                    return "";
                }

                object etypeValObj = GetObjectValue(obj, etypeMem);
                return GetDataTableName(gs, etypeValObj);
            }

            return "";
        }

        public List<NameValue> GetDropdownList(GameState gs, MemberInfo mem, object obj)
        {
            if (gs.data == null || mem == null)
            {
                return new List<NameValue>();
            }

            if (mem.Name == "EquipSlotId")
            {
                Console.WriteLine("EquipSlotId");
            }

            Type memType = mem.GetType();
            if (memType.IsEnum)
            {
                return CreateEnumList(memType);
            }

            List<NameValue> mapDataList = GetMapDropdownList(gs, mem);

            if (mapDataList != null && mapDataList.Count > 0)
            {
                return mapDataList;
            }

            if (mem.Name.IndexOf("EntityId") >= 0 && obj != null)
            {
                return GetEntityIdDropdownList(gs, mem, obj);
            }


            return GetEntityTypeDropdownList(gs, mem);
        }

        protected List<NameValue> GetMapDropdownList(GameState gs, MemberInfo mem)
        {        

            if (gs.map == null)
            {
                return null;
            }

            object dataObject = gs.map.GetEditorListFromName(mem.Name);

            if (dataObject == null)
            {
                return null;
            }

            return CreateDataList(gs, dataObject);

        }


        protected List<NameValue> GetEntityIdDropdownList(GameState gs, MemberInfo mem, object obj)
        {
            if (obj == null || mem == null)
            {
                return null;
            }

            List<NameValue> list = new List<NameValue>();
            string prefix = mem.Name.Replace("EntityId", "");
            string etypeName = prefix + "EntityTypeId";
            MemberInfo etypeMem = GetMemberInfo(obj, etypeName);
            if (etypeMem == null)
            {
                return null;
            }

            object etypeVal = GetObjectValue(obj, etypeMem);


            if (etypeVal != null && gs.map != null)
            {
                string etypeValStr = etypeVal.ToString();
                int etypeId = -1;
                if (int.TryParse(etypeValStr, out etypeId))
                {
                    object dataObject = gs.map.GetEditorListFromEntityTypeId(etypeId);

                    if (dataObject != null)
                    {
                        return CreateDataList(gs, dataObject);
                    }
                }
            }

            string datalistname = GetDataTableName(gs, etypeVal);
            if (string.IsNullOrEmpty(datalistname))
            {
                return list;
            }

            List<NameValue> datalist = CreateDataList(gs, datalistname);
            if (datalist == null || datalist.Count < 1)
            {
                return list;
            }

            return datalist;
        }


        // This adds an entity list dropdown for things that are named XXXId where XXXs is something in the game data.

        protected List<NameValue> GetEntityTypeDropdownList(GameState gs, MemberInfo mem)
        {
            List<NameValue> badList = new List<NameValue>();
            if (gs.data == null || mem == null)
            {
                return badList;
            }

            Type mType = GetMemberType(mem);

            if (mType == null || mType.FullName != "System.Int64")
            {
                return badList;
            }

            // See if the last 2 letters are "Id".
            string nm = mem.Name;
            if (nm.Length < 2)
            {
                return badList;
            }

            string subs = nm.Substring(nm.Length - 2);
            if (subs != GameDataConstants.IdSuffix)
            {
                return badList;
            }

            nm = nm.Substring(0, nm.Length - 2);

            // Now add the s.

            string currname = nm;

            while (currname.Length > 0)
            {
                string nm2 = GetDataTableName(gs, currname);

                List<NameValue> list = CreateDataList(gs, nm2);

                if (list != null && list.Count > 0)
                {
                    return list;
                }

                currname = currname.Substring(1);

            }

            return badList;

        }

        public object InvokeMethod(object obj, MethodInfo info, object[] args)
        {
            if (obj == null || info == null)
            {
                return null;
            }

            if (args == null)
            {
                args = new object[0];
            }

            object retval = null;

            retval = info.Invoke(obj, args);
            return retval;
        }


        // Get an object with an Id 
        public object GetItemWithId(object list, int id, string idMemberName = GameDataConstants.IdKey)
        {
            if (list == null || id <= 0)
            {
                return null;
            }

            if (string.IsNullOrEmpty(idMemberName))
            {
                idMemberName = GameDataConstants.IdKey;
            }

            Type listType = list.GetType();

            MethodInfo arrMethod = listType.GetMethod("ToArray");

            if (arrMethod == null)
            {
                return null;
            }

            object arr = InvokeMethod(list, arrMethod, new object[0]);

            if (arr == null)
            {
                return null;
            }

            Type arrType = arr.GetType();


            MethodInfo readMethod = arrType.GetMethod("GetValue", new[] { typeof(int) });

            if (readMethod == null)
            {
                return null;
            }

            int sz = -1;
            object szObj = GetObjectValue(arr, "Length");
            if (szObj == null)
            {
                return null;
            }

            int.TryParse(szObj.ToString(), out sz);

            if (sz < 1)
            {
                return null;
            }

            for (int idx = 0; idx < sz; idx++)
            {

                object currObj = InvokeMethod(arr, readMethod, new object[1] { idx });
                if (currObj == null)
                {
                    continue;
                }

                object idObj = GetObjectValue(currObj, idMemberName);

                if (idObj == null)
                {
                    continue;
                }

                int newId = -1;

                int.TryParse(idObj.ToString(), out newId);

                if (newId == id)
                {
                    return currObj;
                }

            }
            return null;

        }

        public int GetListSize(object list)
        {
            if (list == null)
            {
                return 0;
            }

            Type objType = list.GetType();

            int currSize = 0;
            if (objType.IsArray)
            {
                currSize = (int)GetObjectValue(list, "Length");
            }
            else if (IsGenericList(objType))
            {
                currSize = (int)GetObjectValue(list, "Count");
            }

            return currSize;
        }


        public object GetLastItem(object list)
        {
            int sz = GetListSize(list);
            if (sz < 1)
            {
                return null;
            }

            return GetItemWithIndex(list, sz - 1);
        }

        public object GetItemWithIndex(object list, int index)
        {
            if (list == null || index < 0)
            {
                return null;
            }

            Type listType = list.GetType();

            MethodInfo arrMethod = listType.GetMethod("ToArray");

            if (arrMethod == null)
            {
                return null;
            }

            object arr = InvokeMethod(list, arrMethod, new object[0]);

            if (arr == null)
            {
                return null;
            }

            Type arrType = arr.GetType();


            MethodInfo readMethod = arrType.GetMethod("GetValue", new[] { typeof(int) });

            if (readMethod == null)
            {
                return null;
            }

            int sz = -1;
            object szObj = GetObjectValue(arr, "Length");
            if (szObj == null)
            {
                return null;
            }

            int.TryParse(szObj.ToString(), out sz);

            if (sz < 0)
            {
                return null;
            }

            if (index < 0)
            {
                index = 0;
            }

            if (index >= sz)
            {
                index = sz - 1;
            }

            object currObj = InvokeMethod(arr, readMethod, new object[1] { index });

            return currObj;


        }

        public List<Type> GetTypesImplementing(Type interfaceType)
        {

            List<Type> retval = new List<Type>();
            if (interfaceType == null || !interfaceType.IsInterface)
            {
                return retval;
            }
            if (!interfaceType.IsInterface)
            {
                return retval;
            }

            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (Assembly assembly in assemblies)
            {
                if (assembly.FullName.IndexOf(Game.Prefix) >= 0)
                {

                    foreach (Type t in assembly.GetExportedTypes())
                    {
                        if (!t.IsClass)
                        {
                            continue;
                        }

                        if (t.IsAbstract)
                        {
                            continue;
                        }

                        if (t.IsGenericType)
                        {
                            continue;
                        }

                        Type inter = t.GetInterface(interfaceType.Name);
                        if (inter == null)
                        {
                            continue;
                        }
                        retval.Add(t);
                    }
                }
            }
            return retval;
        }

        public virtual Dictionary<Type, object> SetupTypedDictionary(GameState gs, Type genericInterfaceType, List<string> extraAssemblies = null)
        {
            Dictionary<Type, object> dict = new Dictionary<Type, object>();

            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

            if (genericInterfaceType == null || !genericInterfaceType.IsInterface || !genericInterfaceType.IsGenericType)
            {
                return dict;
            }

            foreach (Assembly assembly in assemblies)
            {
                bool okAssembly = true;
                if (assembly.FullName.IndexOf(Game.Prefix) >= 0)
                {
                    okAssembly = true;
                }

                if (extraAssemblies != null)
                {
                    foreach (string name in extraAssemblies)
                    {
                        if (assembly.FullName.Contains(name))
                        {
                            okAssembly = true;
                            break;
                        }
                    }
                }

                if (!okAssembly)
                {
                    continue;
                }

                foreach (Type t in assembly.GetExportedTypes())
                {
                    if (!t.IsClass)
                    {
                        continue;
                    }

                    if (t.IsAbstract)
                    {
                        continue;
                    }

                    Type inter = t.GetInterface(genericInterfaceType.Name);
                    if (inter == null)
                    {
                        continue;
                    }

                    object inst = EntityUtils.DefaultConstructor(t);

                    dict[t] = inst;
                    gs.loc.Resolve(inst);
                }
            }
            return dict;
        }

        public virtual Dictionary<K, T> SetupDictionary<K, T>(GameState gs, List<string> extraAssemblies = null) where T : ISetupDictionaryItem<K>
        {
            Dictionary<K, T> dict = new Dictionary<K, T>();
            Type ttype = typeof(T);

            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();


            foreach (Assembly assembly in assemblies)
            {
                bool okAssembly = false;
                if (assembly.FullName.IndexOf(Game.Prefix) >= 0)
                {
                    okAssembly = true;
                }

                if (extraAssemblies != null)
                {
                    foreach (string name in extraAssemblies)
                    {
                        if (assembly.FullName.Contains(name))
                        {
                            okAssembly = true;
                            break;
                        }
                    }
                }

                if (!okAssembly)
                {
                    continue;
                }
                foreach (Type t in assembly.GetExportedTypes())
                {
                    if (!t.IsClass)
                    {
                        continue;
                    }

                    if (t.IsAbstract)
                    {
                        continue;
                    }

                    Type inter = t.GetInterface(ttype.Name);
                    if (inter == null)
                    {
                        continue;
                    }

                    T inst = (T)EntityUtils.DefaultConstructor(t);

                    if (inst == null || inst.GetKey() == null)
                    {
                        continue;
                    }

                    if (dict.ContainsKey(inst.GetKey()))
                    {
                        dict.Remove(inst.GetKey());
                    }

                    dict[inst.GetKey()] = inst;
                    try
                    {
                        gs.loc.Resolve(inst);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("EXC: " + e.Message + " " + e.StackTrace);
                    }
                }
            }
            return dict;
        }



        public IEnumerable SortOnParameter(IEnumerable elist, bool ascending = true)
        {
            try
            {
                if (elist == null)
                {
                    return elist;
                }

                Type utype = GetUnderlyingType(elist);
                if (utype == null)
                {
                    return elist;
                }
                PropertyInfo[] props = utype.GetProperties();


                PropertyInfo prop = props.FirstOrDefault(x => x.IsDefined(typeof(EditorOrderBy), true));

                if (prop == null)
                {
                    return elist;
                }

                List<SortHelper> olist = new List<SortHelper>();

                foreach (object item in elist)
                {
                    SortHelper sh = new SortHelper();
                    sh.SortVal = item;
                    sh.SortKey = GetObjectValue(item, prop);
                    olist.Add(sh);
                }

                if (ascending)
                {
                    olist = olist.OrderBy(x => x.SortKey).ToList();
                }
                else
                {
                    olist = olist.OrderByDescending(x => x.SortKey).ToList();
                }

                IList nlist = CreateGenericList(utype);
                Type listType = typeof(List<>);
                Type[] args = new Type[] { utype };
                Type ltype = listType.MakeGenericType(args);

                MethodInfo addMethod = ltype.GetMethod("Add", new[] { utype });

                foreach (SortHelper item in olist)
                {
                    addMethod.Invoke(nlist, new[] { item.SortVal });
                }

                return nlist;
            }
            catch (Exception)
            {
                return elist;
            }

        }
    }
}