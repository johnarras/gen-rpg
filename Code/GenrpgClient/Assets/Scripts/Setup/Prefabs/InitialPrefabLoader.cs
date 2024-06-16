
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using GEntity = UnityEngine.GameObject;

public class InitialPrefabLoader : MonoBehaviour
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

            GEntity newPrefab = gs.loc.Get<IGameObjectService>().FullInstantiateAndSet(prefabObj);
            newPrefab.name = newPrefab.name.Replace("(Clone)", "");
        }


        await Task.CompletedTask;
    }
}