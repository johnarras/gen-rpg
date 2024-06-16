using MessagePack;
using Genrpg.Shared.Entities.Utils;
using Genrpg.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Reflection;
using Genrpg.Shared.Logging.Interfaces;
using Genrpg.Shared.Analytics.Services;
using Genrpg.Shared.GameSettings;

namespace Genrpg.Shared.Core.Entities
{

    /// <summary>
    /// This is a DI/IOC object that I implemented myself to 
    /// be able to have more than one of these within a given program for 
    /// different contexts (Such as in an editor with multiple games or
    /// multiple environments open)
    /// </summary>
    [MessagePackObject]
    public class ServiceLocator : IServiceLocator
    {

        public ServiceLocator(ILogService logService, IAnalyticsService analyticsService, IGameData gameData)
        {
            _logger = logService;
            Set(logService);
            Set(analyticsService);
            Set(gameData);
        }

        private ILogService _logger = null;

        /// <summary>
        /// Internal storage indexed by type
        /// </summary>
        private Dictionary<Type, IInjectable> _typeDict = new Dictionary<Type, IInjectable>();
        /// <summary>
        /// Internal storage indexed by the name of the type
        /// </summary>
        private Dictionary<string, IInjectable> _nameDict = new Dictionary<string, IInjectable>();

        private List<object> _storedDictionaryItems = new List<object>();

        /// <summary>
        /// Returns an instance of type T
        /// </summary>
        /// <typeparam name="T">The type to be returned. An IFoo can return a FooImpl as long as FooImpl is IFoo</typeparam>
        /// <returns>An object of Type T</returns>
        public T Get<T>() where T : IInjectable
        {
            if (!typeof(T).IsInterface)
            {
                _logger.Error("ServiceLocator only allows interface lookups not: " + typeof(T).Name);
                throw new Exception("ServiceLocator: Attempted to Get non-interface type: " + typeof(T).Name);
            }

            if (!_typeDict.ContainsKey(typeof(T)))
            {
                return default;
            }

            return (T)_typeDict[typeof(T)];
        }

        /// <summary>
        /// Get alist of all keys
        /// </summary>
        /// <returns>Returns a list of all keys from the Type dictionary.</returns>
        public List<Type> GetKeys()
        {
            List<Type> list = new List<Type>();
            if (_typeDict == null)
            {
                return list;
            }

            foreach (Type type in _typeDict.Keys)
            {
                list.Add(type);
            }
            return list;
        }

        /// <summary>
        /// Get a list of all services in the ObjectFactory
        /// </summary>
        /// <returns></returns>
        public List<IInjectable> GetVals()
        {
            List<IInjectable> list = new List<IInjectable>();
            if (_typeDict == null)
            {
                return list;
            }
            foreach (Type type in _typeDict.Keys)
            {
                list.Add(_typeDict[type]);
            }
            return list;
        }

        /// <summary>
        /// Returns an object based on a type name
        /// </summary>
        /// <param name="typeName">The name of the type</param>
        /// <returns>An object which may or may not be of the correct type</returns>
        private object GetByName(string typeName)
        {
            if (string.IsNullOrEmpty(typeName))
            {
                return null;
            }

            if (!_nameDict.ContainsKey(typeName))
            {
                return null;
            }

            return _nameDict[typeName];
        }

        /// <summary>
        /// Add an object to the dictionaries
        /// </summary>
        /// <typeparam name="T">The type to add</typeparam>
        /// <param name="obj">The object of type T to add</param>
        public void Set<T>(T obj) where T : IInjectable
        {
            if (obj == null)
            {
                return;
            }

            if (!typeof(T).IsInterface)
            {
                _logger.Message("ServiceLocator can only set Interfaces Not: " + typeof(T).Name + " ");
                throw new Exception("ServiceLocator: Attempted to Get non-interface type. " + typeof(T).Name);               
            }

            if (_typeDict.ContainsKey(typeof(T)))
            {
                _typeDict.Remove(typeof(T));
            }
            _typeDict[typeof(T)] = obj;

            Type type = typeof(T);

            if (_nameDict.ContainsKey(type.Name))
            {
                _nameDict.Remove(type.Name);
            }

            _nameDict[type.Name] = obj;
        }

        public void Resolve(object obj)
        {
            if (obj == null)
            {
                return;
            }
            Type type = obj.GetType();
            if (!type.IsClass)
            {
                return;
            }

            string serviceTypeName = typeof(IInjectable).Name;
            string serviceLocatorTypeName = typeof(IServiceLocator).Name;
            string initOnResolveTypeName = typeof(IInitOnResolve).Name;

            while (true)
            {
                FieldInfo[] fields = type.GetFields(BindingFlags.NonPublic | BindingFlags.Instance);

                foreach (FieldInfo field in fields)
                {
                    if (field.IsPublic || field.IsStatic)
                    {
                        continue;
                    }

                    Type fieldType = field.FieldType;


                    if (fieldType.GetInterface(initOnResolveTypeName) != null)
                    {
                        Resolve(EntityUtils.GetObjectValue(obj, field));
                    }

                    if (fieldType.Name == serviceLocatorTypeName)
                    {
                        EntityUtils.SetObjectValue(obj, field, this);
                        continue;
                    }

                    Type serviceType = fieldType.GetInterface(serviceTypeName);
                    if (serviceType == null)
                    {
                        continue;
                    }

                    if (!fieldType.IsInterface)
                    {
                        _logger.Message("ServiceLocator Concrete service type " + fieldType.Name + " in " + obj.GetType().Name);
                        throw new Exception("ServiceLocator: Attempted to resolve non-interface type: " + fieldType.Name + " in " + obj.GetType().Name);
                    }

                    object serviceObject = GetByName(fieldType.Name);
                    if (serviceObject == null)
                    {
                        continue;
                    }

                    EntityUtils.SetObjectValue(obj, field, serviceObject);

                }
                type = type.BaseType;
                if (!type.IsClass || type == typeof(object))
                {
                    break;
                }
            }

            if (obj.GetType().GetInterface(initOnResolveTypeName) != null)
            {
                IInitOnResolve initOnResolve = (IInitOnResolve)obj;
                initOnResolve.Init();
            }
        }

        public void ResolveSelf()
        {
            foreach (object val in _typeDict.Values)
            {
                Resolve(val);
            }

            foreach (object val in _storedDictionaryItems)
            {
                Resolve(val);
            }
        }

        public void StoreDictionaryItem(object obj)
        {
            _storedDictionaryItems.Add(obj);
        }
    }
}
