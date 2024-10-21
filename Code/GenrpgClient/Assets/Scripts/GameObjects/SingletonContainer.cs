using Assets.Scripts.Core.Interfaces;
using Genrpg.Shared.Interfaces;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.GameObjects
{
    public interface ISingletonContainer : IInitializable
    {
        public GameObject GetSingleton(string name);
    }

    public class SingletonContainer : ISingletonContainer
    {
        private GameObject _root = null;
        private Dictionary<string, GameObject> _objectDict = new Dictionary<string, GameObject>();

        private IClientEntityService _clientEntityService;

        string containerName = "InitClient";

        public async Task Initialize(CancellationToken token)
        {
            if (_root == null)
            {
                _root = GameObject.Find(containerName);

                if (_root == null)
                {
                    _root = new GameObject() { name = containerName };
                }
            }
            await Task.CompletedTask;
        }


        public GameObject GetSingleton(string name)
        {
           

            if (_objectDict.TryGetValue(name, out GameObject go))
            {
                return go;
            }
            
            go = new GameObject(name);
            _clientEntityService.AddToParent(go, _root);
            _objectDict[name] = go;
            return go;
        }

    }
}
