using Assets.Scripts.Core.Interfaces;
using Genrpg.Shared.Interfaces;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

namespace Assets.Scripts.GameObjects
{
    public interface ISingletonContainer : IInjectable
    {
        public GameObject GetSingleton(string name);
    }

    public class SingletonContainer : BaseBehaviour, IInjectOnLoad<ISingletonContainer>, ISingletonContainer
    {
        private Dictionary<string, GameObject> _objectDict = new Dictionary<string, GameObject>();
        public GameObject GetSingleton(string name)
        {
            if (_objectDict.TryGetValue(name, out GameObject go))
            {
                return go;
            }
            
            go = new GameObject(name);
            _gameObjectService.AddToParent(go, gameObject);
            _objectDict[name] = go;
            return go;
        }
    }
}
