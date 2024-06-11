
using System.Collections.Generic;
using UnityEngine;
using GEntity = UnityEngine.GameObject;

public class InitialPrefabLoader : BaseBehaviour
{
    public List<string> Prefabs;


    public async Awaitable LoadPrefabs(IUnityGameState gs)
    {
        if (Prefabs == null)
        {
            return;
        }

        List<GEntity> entities = new List<GEntity>();
        foreach (string prefab in Prefabs)
        {
            GEntity prefabObj = AssetUtils.LoadResource<GEntity>("Prefabs/" + prefab);
            if (prefabObj == null)
            {
                continue;
            }
            entities.Add(prefabObj);

            GEntity newPrefab = GEntityUtils.FullInstantiateAndSet(gs, prefabObj);
            newPrefab.name = newPrefab.name.Replace("(Clone)", "");
        }

        
    }
}