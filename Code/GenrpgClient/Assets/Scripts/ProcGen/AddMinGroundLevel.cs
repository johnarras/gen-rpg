
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;


using Genrpg.Shared.Core.Entities;



using GEntity = UnityEngine.GameObject;
using Cysharp.Threading.Tasks;

using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.MapServer.Entities;
using System.Threading;

// Settings for YES reflection
// Fres Int: 0.1, Pow: 1.08 Bias: 0

// Settings for NO reflection
// Fres: Int 0.1, Pow: 3.5, Bias: -3.0


public class AddMinGroundLevel : BaseZoneGenerator
{
    public override async UniTask Generate (UnityGameState gs, CancellationToken token)
    {
        await base.Generate(gs, token);
        AddKillCollider(gs);
        //AddMapOcean(gs);

	}


    private static GEntity _killCollider = null;
    public void AddKillCollider(UnityGameState gs)
    {
        if (_killCollider != null)
        {
            return;
        }
        _assetService.LoadAsset(gs, AssetCategoryNames.Prefabs, MapConstants.KillColliderName, OnLoadKillCollider, null, null, _token);
    }

    private void OnLoadKillCollider (UnityGameState gs, string url, object obj, object data, CancellationToken token)
    {
        _killCollider = obj as GEntity;
    }

}
	
