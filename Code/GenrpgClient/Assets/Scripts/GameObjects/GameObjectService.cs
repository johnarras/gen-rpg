using Assets.Scripts.Core.Interfaces;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.Scripts.GameObjects
{
    public class GameObjectService : IGameObjectService
    {
        protected IServiceLocator _loc = null;
        protected IInitClient _initClient = null;
        public GameObject FullInstantiateAndSet(GameObject go)
        {
            GameObject dupe = FullInstantiate(go);

            List<BaseBehaviour> allBehaviours = GEntityUtils.GetComponents<BaseBehaviour>(dupe);

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
        public C FullInstantiate<C>(C c) where C : Component
        {
            if (c == null)
            {
                return null;
            }
            C cdupe = GameObject.Instantiate<C>(c);
            cdupe.name = cdupe.name.Replace("(Clone)", "");
            InitializeHierarchy(cdupe.entity());
            return cdupe;
        }

        public GameObject FullInstantiate(GameObject go)
        {
            GameObject dupe = GameObject.Instantiate(go);
            InitializeHierarchy(dupe);
            return dupe;
        }

        public void InitializeHierarchy(GameObject go)
        {
            GEntityUtils.SetActive(go, true);
            List<BaseBehaviour> allBehaviours = GEntityUtils.GetComponents<BaseBehaviour>(go);

            foreach (BaseBehaviour behaviour in allBehaviours)
            {
                _loc.Resolve(behaviour);
            }
        }

        public C GetOrAddComponent<C>(GameObject go) where C : Component
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
                InitializeHierarchy(go);
            }
            return c;
        }

        public T GetOrAddComponent<T>() where T : MonoBehaviour
        {
            T comp = _initClient?.go.GetComponent<T>();
            if (comp == null)
            {
                comp = _initClient?.go.AddComponent<T>();
            }

            return comp;
        }
    }
}
