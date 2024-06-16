using Genrpg.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public interface IGameObjectService : IInjectable
{
    T GetOrAddComponent<T>() where T : MonoBehaviour;
    GameObject FullInstantiateAndSet(GameObject go);
    C FullInstantiate<C>(C c) where C : Component;
    GameObject FullInstantiate(GameObject go);
    void InitializeHierarchy(GameObject go);
    C GetOrAddComponent<C>(GameObject go) where C : Component;
}
