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

        public static Dictionary<K, T> SetupDictionary<K, T>(GameState gs) where T : ISetupDictionaryItem<K>
        {
            Dictionary<K, T> dict = new Dictionary<K, T>();
            Type ttype = typeof(T);

            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (Assembly assembly in assemblies)
            {
                if (assembly.FullName.IndexOf(Game.Prefix) < 0  && !_searchAssemblies.Contains(assembly))
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

        public static async Task<object> CreateInstanceFromType(GameState gs, Type t, CancellationToken token)
        {
            object obj = Activator.CreateInstance(t);
            gs.loc.Resolve(obj);

            if (obj is IService service)
            {
                await SetupServices(gs, new List<IService> { service }, token);
            }

            return obj;
        }

        public static async Task SetupServices(GameState gs, List<IService> services, CancellationToken token)
        {

            List<ISetupService> setupServices = new List<ISetupService>();

            List<IPrioritySetupService> priorityServices = new List<IPrioritySetupService>();

            foreach (IService service in services)
            {
                if (service is ISetupService setupService)
                {
                    setupServices.Add(setupService);
                }

                if (service is IPrioritySetupService prioritySetupService)
                {
                    priorityServices.Add(prioritySetupService);
                }
            }

            List<IGrouping<int,IPrioritySetupService>> groupedServices = priorityServices.GroupBy(x => x.SetupPriorityAscending()).OrderBy(x=>x.Key).ToList();  

            foreach (IGrouping<int,IPrioritySetupService> group in groupedServices)
            {

                List<Task> priorityTasks = new List<Task>();

                List<IPrioritySetupService> currentPriorityServices = group.ToList();

                foreach (IPrioritySetupService service in currentPriorityServices)
                {
                    priorityTasks.Add(service.PrioritySetup(gs, token));
                }

                await Task.WhenAll(priorityTasks);

            }

            List<Task> setupTasks = new List<Task>();

            foreach (ISetupService setupService in setupServices)
            {
                setupTasks.Add(setupService.Setup(gs, token));
            }

            await Task.WhenAll(setupTasks);

        }
    }
}
