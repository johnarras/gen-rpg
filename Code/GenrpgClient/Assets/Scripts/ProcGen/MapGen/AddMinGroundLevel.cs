
using UnityEngine;
using System.Threading;
using Genrpg.Shared.Client.Core;
using Genrpg.Shared.Client.Assets.Constants;

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
	}


    private static GameObject _killCollider = null;
    public void AddKillCollider(IClientGameState gs)
    {
        if (_killCollider != null)
        {
            return;
        }
        _assetService.LoadAsset(AssetCategoryNames.Prefabs, MapConstants.KillColliderName, OnLoadKillCollider, null, null, _token);
    }

    private void OnLoadKillCollider (object obj, object data, CancellationToken token)
    {
        _killCollider = obj as GameObject;
    }

}
	
