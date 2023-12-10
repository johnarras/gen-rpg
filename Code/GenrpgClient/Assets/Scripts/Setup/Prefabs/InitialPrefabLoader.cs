using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using GEntity = UnityEngine.GameObject;

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
            GEntity prefabObj = AssetUtils.LoadResource<GEntity>("Prefabs/" + prefab);
            if (prefabObj == null)
            {
                continue;
            }

            GEntity newPrefab = GEntityUtils.FullInstantiate(gs, prefabObj);
            newPrefab.name = newPrefab.name.Replace("(Clone)", "");
            _gs.loc.ResolveSelf();
            _gs.loc.Resolve(newPrefab);
        }
        await UniTask.CompletedTask;
    }
}