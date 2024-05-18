using GEntity = UnityEngine.GameObject;
using GObject = UnityEngine.Object;
using System.Collections.Generic;
using UnityEngine;
using System;
using Assets.Scripts.Core.Interfaces;
using Genrpg.Shared.Core.Entities;
using System.Linq; // Needed

public class GEntityUtils
{


    public static void SetActive(GEntity go, bool value)
    {
        if (go == null)
        {
            return;
        }

        if (go.activeSelf != value)
        {
            go.SetActive(value);
        }
    }

    public static void SetActive(Component comp, bool value)
	{
        if (comp != null)
        {
            SetActive(comp.entity(), value);
        }
    }

    public static void Destroy<T>(T obj) where T : GObject
    {
#if UNITY_EDITOR
        if (!AppUtils.IsPlaying)
        { 
            GEntity.DestroyImmediate(obj);
        }
        else
        {
            GEntity.Destroy(obj);
        }
#else
        GEntity.Destroy(obj);
#endif
    }

	
	// Use this for things that will never change once created and found.
	private static IDictionary<string,GEntity> _objCache = new Dictionary<string,GEntity>();
	public static GEntity FindSingleton (string name, bool createIfNotExist = false)
	{
		if (string.IsNullOrEmpty(name))
        {
            return null;
        }

        if (_objCache.ContainsKey(name))
        {
            return _objCache[name];
        }

        GEntity go = GEntity.Find (name);
		if (go == null)
        {
            if (createIfNotExist)
            {
                go = new GEntity();
                go.name = name;
            }
            else
            {
                return null;
            }
        }
		_objCache[name] = go;
		return go;
	}
	
	
	
	public static GEntity FindChild (GEntity obj, string name)
	{
		if (obj == null || string.IsNullOrEmpty(name))
        {
            return null;
        }

        if (obj.name == name)
        {
            return obj;
        }

        for (int t = 0; t < obj.transform().childCount; t++)
 		{

            GEntity obj2 = obj.transform().GetChild(t).entity();
			if (obj2.name == name)
			{
				return obj2;
			}
		}
		for (int t = 0; t < obj.transform().childCount; t++)
		{
			GEntity obj2 = FindChild (obj.transform().GetChild (t).entity(), name);
			if (obj2 != null)
            {
                return obj2;
            }
        }
		return null;
	}
	
	
	
	public static List<T> GetComponents<T> (GEntity go) where T : Component
	{
		List<T> comps = new List<T>();
		
		if (go == null)
        {
            return comps;
        }

		T[] arr = go.GetComponentsInChildren<T>();
		if (arr != null && arr.Length > 0)
		{
			foreach (T comp in arr)
			{
				comps.Add(comp);
			}
		}
		
		return comps;
		
	}

	
	public static T GetComponent<T> (GEntity go) where T : Component
	{
		if (go == null)
        {
            return default(T);
        }

        List<T> list = new List<T>();
		
		T comp = go.GetComponent<T>();

        if (comp != null)
        {
            return comp;
        }

        T[] comps = go.GetComponentsInChildren<T>();
		

        if (comps != null && comps.Length> 0)
        {
            return comps[0];
        }
		return default(T);
	}
		
	static public T FindInParents<T> (GEntity go) where T : Component
	{
		if (go == null)
        {
            return null;
        }

        T comp = go.GetComponent<T>() as T;

		if (comp == null)
		{
			Transform t = go.transform().parent;

			while (t != null && comp == null)
			{
				comp = t.entity().GetComponent<T>();
				t = t.parent;
			}
		}
		return (T)comp;
	}


    public static void DestroyAllChildren (string name)
    {
        DestroyAllChildren(FindSingleton(name));
    }
	
	public static void DestroyAllChildren (GEntity go)
	{
		if (go == null)
        {
            return;
        }

        List<GEntity> list = new List<GEntity>();
	
		foreach (Transform t in go.transform())
		{
			if (t.entity() != null)
			{
				list.Add (t.entity());		
			}
		}
		
		for (int l = 0; l < list.Count; l++)
		{
			GEntity item = list[l];
			
			DestroyAllChildren (item);
			GEntityUtils.Destroy (item);
			
		}
		
	}
	
	
	
	
	public static void SetLayer (GEntity go, string layerName)
    {
        SetLayer(go, LayerUtils.NameToLayer(layerName));
      
	}

	public static void SetLayer (GEntity go, int layer)
	{
        if (go == null || layer < 0 || layer >= 32)
        {
            return;
        }

        InnerSetLayerRecursive(go, layer);
	}

	private static void InnerSetLayerRecursive(GEntity go, int layer)
	{
        if (go == null)
        {
            return;
        }

        go.layer = layer;
		foreach (Transform tr in go.transform())
		{
			InnerSetLayerRecursive(tr.entity(), layer);
		}
	}


    public static C GetOrAddComponent<C> (UnityGameState gs, GEntity go) where C: Component
    {
        if (go == null)
        {
            return null;
        }

        C c = go.GetComponent<C>();
        if (c == null)
        {
            c = go.AddComponent<C>();
        }
#if UNITY_EDITOR
        go.hideFlags = 0;
#endif

        if (c is BaseBehaviour bb)
        {
            GEntityUtils.InitializeHierarchy(gs, go);
        }
        return c;
    }

	public static void AddToParent (GEntity child, GEntity parent)
	{
		if (parent == null)
		{
            GEntityUtils.Destroy(child);
			return;
        }
        child.transform().SetParent(parent.transform());
        child.transform().localPosition = GVector3.zeroPlatform;
        child.transform().localEulerAngles = GVector3.zeroPlatform;
        child.transform().localScale = GVector3.onePlatform;
        SetLayer(child, parent.layer);
	}

	public static void FullDestroy (UnityEngine.Object obj)
	{
		GEntityUtils.Destroy(obj);
	}
	
	public static C SetupGlobalComponent<C> (string name) where C : Component
    {
        if (string.IsNullOrEmpty(name))
        {
            return null;
        }

        GEntity obj = GEntityUtils.FindSingleton(name);
        if (obj == null)
        {
            obj = new GEntity();
            obj.name = name;
        }

        C comp = obj.GetComponent<C>();
        if (comp == null)
        {
            comp = obj.AddComponent<C>();
        }
        return comp;
    }

    public static bool SetStatic(GEntity go, bool value)
    {
        if (go == null)
        {
            return false;
        }

        bool didChangeSomething = false;

        if (go.isStatic != value)
        {
            go.isStatic = value;
            didChangeSomething = true;
        }
        for (int c =0; c < go.transform().childCount; c++)
        {
            Transform child = go.transform().GetChild(c);
            if (child.entity() != null)
            {
                if (SetStatic(child.entity(), value))
                {
                    didChangeSomething = true;
                }
            }
        }
        return didChangeSomething;
    }

    public static void IgnoreCollisions(GEntity go1, GEntity go2)
	{
		if (go1 == null || go2 == null)
        {
            return;
        }

        Collider[] colls1 = go1.GetComponents<Collider>();
        Collider[] colls2 = go2.GetComponents<Collider>();
		
		if (colls1 == null || colls2 == null)
        {
            return;
        }

        foreach (Collider coll1 in colls1)
		{
			foreach (Collider coll2 in colls2)
			{
			Physics.IgnoreCollision(coll1,coll2);		
			}
		}		
    }


    public static void UnloadAndCollect()
    {
    }

    public static T FindComponentInParents<T>(GEntity go) where T : Component
    {
        if (go == null)
        {
            return default(T);
        }

        T tcomp = go.GetComponent<T>();
        if (tcomp != null)
        {
            return tcomp;
        }

        if (go.transform().parent != null)
        {
            return FindComponentInParents<T>(go.transform().parent.entity());
        }
        return default(T);
    }

    public static C FullInstantiate<C>(UnityGameState gs, C c) where C : UnityEngine.Component
    {
        if (c == null)
        {
            return null;
        }
        C cdupe = GEntity.Instantiate<C>(c);
        cdupe.name = cdupe.name.Replace("(Clone)", "");
        InitializeHierarchy(gs, cdupe.entity());
        return cdupe;
    }

    public static GEntity FullInstantiateAndSet (UnityGameState gs, GEntity go)
    {
        GEntity dupe = FullInstantiate(gs, go);

        List<BaseBehaviour> allBehaviours = GEntityUtils.GetComponents<BaseBehaviour>(dupe);

        foreach (BaseBehaviour behaviour in allBehaviours)
        {
            Type setType = behaviour.GetType().GetInterfaces()
                .FirstOrDefault(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IInjectOnLoad<>));
           
            if (setType != null)
            {
                var setMethod = typeof(ServiceLocator).GetMethod("Set");
                var genericMethod = setMethod.MakeGenericMethod(setType.GenericTypeArguments[0]);
                genericMethod.Invoke(gs.loc, new object[] { behaviour } );
            }
        }
        return dupe;
    }

    public static GEntity FullInstantiate(UnityGameState gs, GEntity go)
    {
        GEntity dupe = GEntity.Instantiate(go);
        InitializeHierarchy(gs, dupe);
        return dupe;
    }

    public static void InitializeHierarchy(UnityGameState gs, GEntity go)
    {
        SetActive(go, true);
        List<BaseBehaviour> allBehaviours = GEntityUtils.GetComponents<BaseBehaviour>(go);

        foreach (BaseBehaviour behaviour in allBehaviours)
        {
            behaviour.Initialize(gs);
        }
    }
}

