using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Text;
using System.Reflection;

using Genrpg.Shared.Core.Entities;
using Services;
public class GameObjectUtils
{


    public static void SetActive(GameObject go, bool value)
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
            SetActive(comp.gameObject, value);
        }
    }
		
	
	// Use this for things that will never change once created and found.
	private static IDictionary<string,GameObject> _objCache = new Dictionary<string,GameObject>();
	public static GameObject FindSingleton (string name, bool createIfNotExist = false)
	{
		if (string.IsNullOrEmpty(name))
        {
            return null;
        }

        if (_objCache.ContainsKey(name))
        {
            return _objCache[name];
        }

        GameObject go = GameObject.Find (name);
		if (go == null)
        {
            if (createIfNotExist)
            {
                go = new GameObject();
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
	
	
	
	public static GameObject FindChild (GameObject obj, string name)
	{
		if (obj == null || string.IsNullOrEmpty(name))
        {
            return null;
        }

        if (obj.name == name)
        {
            return obj;
        }

        for (int t = 0; t < obj.transform.childCount; t++)
 		{

            GameObject obj2 = obj.transform.GetChild(t).gameObject;
			if (obj2.name == name)
			{
				return obj2;
			}
		}
		for (int t = 0; t < obj.transform.childCount; t++)
		{
			GameObject obj2 = FindChild (obj.transform.GetChild (t).gameObject, name);
			if (obj2 != null)
            {
                return obj2;
            }
        }
		return null;
	}
	
	
	
	public static List<T> GetComponents<T> (GameObject go) where T : Component
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

	
	public static T GetComponent<T> (GameObject go) where T : Component
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
		
	static public T FindInParents<T> (GameObject go) where T : Component
	{
		if (go == null)
        {
            return null;
        }

        T comp = go.GetComponent<T>() as T;

		if (comp == null)
		{
			Transform t = go.transform.parent;

			while (t != null && comp == null)
			{
				comp = t.gameObject.GetComponent<T>();
				t = t.parent;
			}
		}
		return (T)comp;
	}


    public static void DestroyAllChildren (string name)
    {
        DestroyAllChildren(FindSingleton(name));
    }
	
	public static void DestroyAllChildren (GameObject go)
	{
		if (go == null)
        {
            return;
        }

        List<GameObject> list = new List<GameObject>();
	
		foreach (Transform t in go.transform)
		{
			if (t.gameObject != null)
			{
				list.Add (t.gameObject);		
			}
		}
		
		for (int l = 0; l < list.Count; l++)
		{
			GameObject item = list[l];
			
			DestroyAllChildren (item);
			GameObject.Destroy (item);
			
		}
		
	}
	
	
	
	
	public static void SetLayer (GameObject go, string layerName)
    {
        SetLayer(go, LayerMask.NameToLayer(layerName));
      
	}

	public static void SetLayer (GameObject go, int layer)
	{
        if (go == null || layer < 0 || layer >= 32)
        {
            return;
        }

        InnerSetLayerRecursive(go, layer);
	}

	private static void InnerSetLayerRecursive(GameObject go, int layer)
	{
        if (go == null)
        {
            return;
        }

        go.layer = layer;
		foreach (Transform tr in go.transform)
		{
			InnerSetLayerRecursive(tr.gameObject, layer);
		}
	}


    public static C GetOrAddComponent<C> (UnityGameState gs, GameObject go) where C: Component
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
            GameObjectUtils.InitializeHierarchy(gs, go);
        }
        return c;
    }

	public static void AddToParent (GameObject child, GameObject parent)
	{
		if (parent == null)
		{
            GameObject.Destroy(child);
			return;
        }
        child.transform.SetParent(parent.transform);
        child.transform.localPosition = Vector3.zero;
        child.transform.localEulerAngles = Vector3.zero;
        child.transform.localScale = Vector3.one;
        SetLayer(child, parent.layer);
	}

    public static GameObject InstantiateIntoParent(object child, GameObject parent)
    {
        GameObject go = child as GameObject;
        if (go == null)
        {
            return null;
        }
        go = GameObject.Instantiate<GameObject>(go);

        go.name = go.name.Replace("(Clone)", "");
        go.name = go.name.Replace(UnityAssetService.ArtFileSuffix, "");

        if (parent != null)
        {
            AddToParent(go, parent);
        }
        return go;
    }


	public static void FullDestroy (UnityEngine.Object obj)
	{
		GameObject.Destroy(obj);
	}
	
	public static C SetupGlobalComponent<C> (string name) where C : Component
    {
        if (string.IsNullOrEmpty(name))
        {
            return null;
        }

        GameObject obj = GameObjectUtils.FindSingleton(name);
        if (obj == null)
        {
            obj = new GameObject();
            obj.name = name;
        }

        C comp = obj.GetComponent<C>();
        if (comp == null)
        {
            comp = obj.AddComponent<C>();
        }
        return comp;
    }

    public static bool SetStatic(GameObject go, bool value)
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
        for (int c =0; c < go.transform.childCount; c++)
        {
            Transform child = go.transform.GetChild(c);
            if (child.gameObject != null)
            {
                if (SetStatic(child.gameObject, value))
                {
                    didChangeSomething = true;
                }
            }
        }
        return didChangeSomething;
    }

    public static void IgnoreCollisions(GameObject go1, GameObject go2)
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

    public static T FindComponentInParents<T>(GameObject go) where T : Component
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

        if (go.transform.parent != null)
        {
            return FindComponentInParents<T>(go.transform.parent.gameObject);
        }
        return default(T);
    }

    public static C FullInstantiate<C>(UnityGameState gs, C c) where C : UnityEngine.Component
    {
        C cdupe = GameObject.Instantiate<C>(c);
        InitializeHierarchy(gs, cdupe.gameObject);
        return cdupe;
    }

    public static GameObject FullInstantiate(UnityGameState gs, GameObject go)
    {
        GameObject dupe = GameObject.Instantiate(go);
        InitializeHierarchy(gs, dupe);
        return dupe;
    }

    public static void InitializeHierarchy(UnityGameState gs, GameObject go)
    {
        List<BaseBehaviour> allBehaviours = GameObjectUtils.GetComponents<BaseBehaviour>(go);

        foreach (BaseBehaviour behaviour in allBehaviours)
        {
            behaviour.Initialize(gs);
        }
    }
}

