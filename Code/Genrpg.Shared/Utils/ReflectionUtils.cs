using MessagePack;
using Genrpg.Shared.Constants;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Entities.Utils;
using Genrpg.Shared.Interfaces;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.DataStores.Interfaces;
using Genrpg.Shared.DataStores.Entities;

namespace Genrpg.Shared.Utils
{
    [MessagePackObject]
    public class ReflectionUtils
    {

        private static List<Assembly> _searchAssemblies = new List<Assembly>();

        public static void AddAllowedAssembly(Assembly assembly)
        {
            if (!_searchAssemblies.Contains(assembly))
            {
                _searchAssemblies.Add(assembly);
            }
        }

        public static List<Type> GetTypesImplementing(Type interfaceType)
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

            if (!_searchAssemblies.Contains(Assembly.GetExecutingAssembly()))
            {
                Assembly currentAssembly = Assembly.GetExecutingAssembly();
                int dotIndex = currentAssembly.FullName.IndexOf(".");

                if (dotIndex > 0)
                {
                    string prefix = currentAssembly.FullName.Substring(0, dotIndex);

                    Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
                    foreach (Assembly assembly in assemblies)
                    {
                        if (assembly.FullName.IndexOf(prefix) == 0)
                        {
                            AddAllowedAssembly(assembly);
                        }
                    }
                }
            }

            foreach (Assembly assembly in _searchAssemblies)
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
            return retval;
        }

        public static Dictionary<K, T> SetupDictionary<K, T>(IServiceLocator loc) where T : ISetupDictionaryItem<K>
        {
            Dictionary<K, T> dict = new Dictionary<K, T>();
            Type ttype = typeof(T);

            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (Assembly assembly in assemblies)
            {
                if (assembly.FullName.IndexOf(Game.Prefix) < 0
                    && assembly.FullName.IndexOf(Game.DefaultPrefix) < 0
                    && !_searchAssemblies.Contains(assembly))
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

                    if (t.ContainsGenericParameters)
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
                        loc.StoreDictionaryItem(inst);
                        loc.Resolve(inst);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("EXC: " + e.Message + " " + e.StackTrace);
                    }
                }
            }
            return dict;
        }

        public static async Task<object> CreateInstanceFromType(IServiceLocator loc, Type t, CancellationToken token)
        {
            object obj = Activator.CreateInstance(t);

            loc.Resolve(obj);

            if (obj is IInitializable service)
            {
                await InitializeServiceList(loc, new List<IInjectable> { service }, token);
            }

            return obj;
        }

        public static async Task InitializeServiceList(IServiceLocator loc, List<IInjectable> services, CancellationToken token)
        {

            List<IInitializable> setupServices = new List<IInitializable>();

            List<IPriorityInitializable> priorityServices = new List<IPriorityInitializable>();

            foreach (IInjectable service in services)
            {
                if (service is IInitializable setupService)
                {
                    setupServices.Add(setupService);
                }

                if (service is IPriorityInitializable prioritySetupService)
                {
                    priorityServices.Add(prioritySetupService);
                }
            }

            List<IGrouping<int,IPriorityInitializable>> groupedServices = priorityServices.GroupBy(x => x.SetupPriorityAscending()).OrderBy(x=>x.Key).ToList();  

            foreach (IGrouping<int,IPriorityInitializable> group in groupedServices)
            {

                List<Task> priorityTasks = new List<Task>();

                List<IPriorityInitializable> currentPriorityServices = group.ToList();

                foreach (IPriorityInitializable service in currentPriorityServices)
                {
                    priorityTasks.Add(service.PrioritySetup(token));
                }

                await Task.WhenAll(priorityTasks);

            }

            List<Task> setupTasks = new List<Task>();

            foreach (IInitializable setupService in setupServices)
            {
                setupTasks.Add(setupService.Initialize(token));
            }

            await loc.InitializeDictionaryItems(token);

            await Task.WhenAll(setupTasks);

        }
    }
}
