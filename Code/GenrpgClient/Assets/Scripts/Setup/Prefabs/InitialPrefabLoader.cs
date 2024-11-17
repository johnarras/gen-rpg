
using Assets.Scripts.Assets;
using Genrpg.Shared.Client.Assets.Services;
using Genrpg.Shared.Client.Core;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class InitialPrefabLoader : MonoBehaviour
{
    public List<string> Prefabs;

    public async Awaitable LoadPrefabs(IClientGameState gs, IClientEntityService entityService, ILocalLoadService localLoadService,
        GameObject parent)
    {
        if (Prefabs == null)
        {
            return;
        }
        

        List<GameObject> entities = new List<GameObject>();
        foreach (string prefab in Prefabs)
        {
            GameObject prefabObj =  localLoadService.LocalLoad<GameObject>("Prefabs/" + prefab);
            if (prefabObj == null)
            {
                continue;
            }
            entities.Add(prefabObj);

            GameObject newPrefab = (GameObject)entityService.FullInstantiateAndSet(prefabObj);
            newPrefab.name = newPrefab.name.Replace("(Clone)", "");
            entityService.AddToParent(newPrefab, parent);
        }


        await Task.CompletedTask;
    }
}