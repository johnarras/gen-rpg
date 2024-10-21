using Assets.Scripts.Core.Interfaces;
using Genrpg.Shared.Client.Core;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Logging.Interfaces;
using Genrpg.Shared.UI.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.GameObjects
{
    public class ClientEntityService : IClientEntityService
    {
        protected IServiceLocator _loc = null;
        protected IInitClient _initClient = null;
        protected IUIService _uiService = null;
        protected ILogService _logService = null;
        private IClientAppService _clientAppService = null;
        public object FullInstantiateAndSet(object obj)
        {
            if (!(obj is GameObject go))
            {
                return null;
            }

            GameObject dupe = (GameObject)FullInstantiate(go);

            List<BaseBehaviour> allBehaviours = GetComponents<BaseBehaviour>(dupe);

            foreach (BaseBehaviour behaviour in allBehaviours)
            {
                Type setType = behaviour.GetType().GetInterfaces()
                    .FirstOrDefault(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IInjectOnLoad<>));

                if (setType != null)
                {
                    var setMethod = typeof(ServiceLocator).GetMethod("Set");
                    var genericMethod = setMethod.MakeGenericMethod(setType.GenericTypeArguments[0]);
                    genericMethod.Invoke(_loc, new object[] { behaviour });
                }
            }
            return dupe;
        }


        public C FullInstantiate<C>(C obj) where C : class
        {
            if (!(obj is UnityEngine.Object cobj))
            {
                return null;
            }

            UnityEngine.Object cdupe = GameObject.Instantiate(cobj);

            cdupe.name = cdupe.name.Replace("(Clone)", "");
            if (cdupe is Component cdupeComp)
            {
                InitializeHierarchy(cdupeComp.gameObject);
            }
            return cdupe as C;
        }

        public object FullInstantiate(object obj)
        {
            if (!(obj is GameObject go))
            {
                if (obj is UnityEngine.Object uobj)
                {
                    return FullInstantiate(uobj);
                }  
                return null;
            }

            GameObject dupe = GameObject.Instantiate(go);
            dupe.name = dupe.name.Replace("(Clone)", "");
            InitializeHierarchy(dupe);
            return dupe;
        }

        public void InitializeHierarchy(object obj)
        {
            if (!(obj is GameObject go))
            {
                return;
            }

            SetActive(go, true);
            List<MonoBehaviour> allBehaviours = GetComponents<MonoBehaviour>(go);

            foreach (MonoBehaviour behaviour in allBehaviours)
            {
                if (behaviour is BaseBehaviour baseBehaviour)
                {
                    try
                    {
                        _loc.Resolve(baseBehaviour);
                    }
                    catch (Exception e)
                    {
                        _logService.Exception(e, "InitializeHierarchy");
                    }
                }
                else if (behaviour is GText gtext && !string.IsNullOrEmpty(gtext.text))
                {
                    _uiService.SetText(gtext, gtext.text);
                }
            }
        }

        public C GetOrAddComponent<C>(object obj) where C : class
        {
            if (!(obj is GameObject go))
            {
                return null;
            }

            if (!go.TryGetComponent<C>(out C c))
            {
                c = go.AddComponent(typeof(C)) as C;
            }
#if UNITY_EDITOR
            go.hideFlags = 0;
#endif

            if (c is BaseBehaviour bb)
            {
                InitializeHierarchy(go);
            }
            return c;
        }

        public void SetActive(object obj, bool value)
        {
            GameObject go = obj as GameObject;
            if (go == null)
            {
                if (obj is Component comp)
                {
                    go = comp.gameObject;
                }
            }

            if (go != null && go.activeSelf != value)
            {
                go.SetActive(value);
            }
        }

        public void Destroy(object obj)
        {
            if (!(obj is UnityEngine.Object unityObject))
            {
                return;
            }
#if UNITY_EDITOR
            if (!_clientAppService.IsPlaying)
            {
                GameObject.DestroyImmediate(unityObject);
            }
            else
            {
                GameObject.Destroy(unityObject);
            }
#else
        GameObject.Destroy(unityObject);
#endif
        }

        public object FindChild(object objIn, string name)
        {
            GameObject go = objIn as GameObject;
            if (go == null)
            {
                return null;
            }

            if (go == null || string.IsNullOrEmpty(name))
            {
                return null;
            }

            if (go.name == name)
            {
                return go;
            }

            for (int t = 0; t < go.transform.childCount; t++)
            {

                GameObject obj2 = go.transform.GetChild(t).gameObject;
                if (obj2.name == name)
                {
                    return obj2;
                }
            }
            for (int t = 0; t < go.transform.childCount; t++)
            {
                GameObject obj2 = (GameObject)FindChild(go.transform.GetChild(t).gameObject, name);
                if (obj2 != null)
                {
                    return obj2;
                }
            }
            return null;
        }

        public List<T> GetComponents<T>(object obj)
        {
            List<T> comps = new List<T>();

            if (!(obj is GameObject go))
            {
                return new List<T>();
            }

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

        public T GetComponent<T>(object obj) where T : class
        {
            if (!(obj is GameObject go))
            {
                return default(T);
            }

            go.TryGetComponent<T>(out T comp);

            if (comp != null)
            {
                return comp;
            }

            T[] comps = go.GetComponentsInChildren<T>();

            if (comps != null && comps.Length > 0)
            {
                return comps[0];
            }
            return default(T);
        }

        public T FindInParents<T>(object obj) where T : class
        {
            GameObject go = obj as GameObject;
            if (go == null)
            {
                return null;
            }

            if (go == null)
            {
                return null;
            }

            if (!go.TryGetComponent<T>(out T comp))
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

        public void DestroyAllChildren(object obj)
        {
            if (!(obj is GameObject go))
            {
                return;
            }

            List<GameObject> list = new List<GameObject>();

            foreach (Transform t in go.transform)
            {
                if (t.gameObject != null)
                {
                    list.Add(t.gameObject);
                }
            }

            for (int l = 0; l < list.Count; l++)
            {
                GameObject item = list[l];

                DestroyAllChildren(item);
                Destroy(item);

            }

        }

        public void SetLayer(object obj, string layerName)
        {
            SetLayer(obj, LayerUtils.NameToLayer(layerName));

        }

        public void SetLayer(object obj, int layer)
        {
            if (!(obj is GameObject go))
            {
                return;
            }

            if (go == null || layer < 0 || layer >= 32)
            {
                return;
            }

            InnerSetLayerRecursive(go, layer);
        }

        private void InnerSetLayerRecursive(object obj, int layer)
        {
            if (!(obj is GameObject go))
            {
                return;
            }

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



        public void AddToParent(object childObj, object parentObj)
        {

            if (!(childObj is GameObject child) ||
                !(parentObj is GameObject parent))
            {
                return;
            }

            if (parent == null)
            {
                Destroy(child);
                return;
            }
            child.transform.SetParent(parent.transform);
            child.transform.localPosition = Vector3.zero;
            child.transform.localEulerAngles = Vector3.zero;
            child.transform.localScale = Vector3.one;
            SetLayer(child, parent.layer);
        }

        public object GetEntity(object obj)
        {
            if (obj is MonoBehaviour mb)
            {
                return mb.gameObject;
            }
            return null;
        }
    }
}
