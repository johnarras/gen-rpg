using Genrpg.Shared.MVC.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.MVC
{

    [Serializable]
    public class ViewElement
    {
        public UnityEngine.Object Obj;
        public string Name;
    }

    public class BaseView : MonoBehaviour, IView
    {
        [SerializeField]
        private List<ViewElement> _elements = new List<ViewElement>();


        public T Get<T>(string name) where T : class
        {
            if (name == "Root" || name == "root")
            {
                return gameObject as T;
            }

            ViewElement element = _elements.FirstOrDefault(x => x.Name == name);
            if (element != null)
            {
                if (element.Obj is GameObject go)
                {
                    if (typeof(T) != typeof(GameObject) && typeof(T) != typeof(object))
                    {
                        MonoBehaviour[] mbs = go.GetComponents<MonoBehaviour>();    

                        foreach (MonoBehaviour mb in mbs)
                        {
                            if (mb is T t)
                            {
                                return t;
                            }
                        }
                    }
                    return element.Obj as T;
                }
                else
                {
                    return element.Obj as T;
                }
            }
            return default(T);
        }

        public object LocalPosition()
        {
            return gameObject.transform.localPosition;
        }

        public object Position()
        {
            return gameObject.transform.position;
        }

        public void SetLocalPosition(object localPosition)
        {
            if (localPosition is Vector3 vec)
            {
                gameObject.transform.localPosition = vec;   
            }
        }

        public void SetPosition(object position)
        {
            if (position is Vector3 vec)
            {
                gameObject.transform.position = vec;
            }
        }
    }
}
