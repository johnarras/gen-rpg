
using GEntity = UnityEngine.GameObject;
using System.Threading;
using UnityEngine;

// Settings for YES reflection
// Fres Int: 0.1, Pow: 1.08 Bias: 0

// Settings for NO reflection
// Fres: Int 0.1, Pow: 3.5, Bias: -3.0


public class AddMinGroundLevel : BaseZoneGenerator
{
    public override async Awaitable Generate (CancellationToken token)
    {
        await base.Generate(token);
        AddKillCollider(_gs);
        //AddMapOcean(gs);

	}


    private static GEntity _killCollider = null;
    public void AddKillCollider(IUnityGameState gs)
    {
        if (_killCollider != null)
        {
            return;
        }
        _assetService.LoadAsset(AssetCategoryNames.Prefabs, MapConstants.KillColliderName, OnLoadKillCollider, null, null, _token);
    }

    private void OnLoadKillCollider (object obj, object data, CancellationToken token)
    {
        _killCollider = obj as GEntity;
    }

}
	
