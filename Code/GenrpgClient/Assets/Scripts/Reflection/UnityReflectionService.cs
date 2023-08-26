using System;
using System.Collections.Generic;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Reflection.Services;

public class UnityReflectionService : ReflectionService
{
    public override Dictionary<K, T> SetupDictionary<K, T>(GameState gs,List<string> extraAssemblies = null)
    {
        if (extraAssemblies == null)
        {
            extraAssemblies = new List<string>();
        }
        extraAssemblies.Add(GetType().Assembly.GetName().Name);
        return base.SetupDictionary<K, T>(gs, extraAssemblies);
    }

    public override Dictionary<Type, object> SetupTypedDictionary(GameState gs, Type genericInterfaceType, List<string> extraAssemblies = null)
    {
        if (extraAssemblies == null)
        {
            extraAssemblies = new List<string>();
        }
        extraAssemblies.Add(GetType().Assembly.GetName().Name);
        return base.SetupTypedDictionary(gs, genericInterfaceType, extraAssemblies);
    }
}
