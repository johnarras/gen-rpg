using MessagePack;
using Genrpg.Shared.Constants;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Genrpg.Shared.Utils
{
    [MessagePackObject]
    public class ReflectionUtils
    {
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
    }
}
