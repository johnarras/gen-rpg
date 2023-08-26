using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class InitialPrefabLoader : BaseBehaviour
{
    public List<string> Prefabs;


    public async UniTask LoadPrefabs(UnityGameState gs)
    {
        Initialize(gs);
        if (Prefabs == null)
        {
            return;
        }

        foreach (string prefab in Prefabs)
        {
            GameObject prefabObj = Resources.Load<GameObject>("Prefabs/" + prefab);
            if (prefabObj == null)
            {
                continue;
            }

            GameObject newPrefab = GameObjectUtils.FullInstantiate(gs, prefabObj);
            newPrefab.name = newPrefab.name.Replace("(Clone)", "");
            _gs.loc.ResolveSelf();
            _gs.loc.Resolve(newPrefab);
        }
        await UniTask.CompletedTask;
    }
}