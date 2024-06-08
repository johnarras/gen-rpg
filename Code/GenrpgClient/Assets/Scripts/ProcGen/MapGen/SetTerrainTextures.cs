
using System;
using System.Collections.Generic;
using GEntity = UnityEngine.GameObject;
using Cysharp.Threading.Tasks;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.Zones.Entities;
using Genrpg.Shared.ProcGen.Entities;
using System.Threading;
using Assets.Scripts.MapTerrain;
using UnityEngine; // Needed
using Genrpg.Shared.ProcGen.Settings.Texturse;
using Genrpg.Shared.Zones.WorldData;

public class SetTerrainTextures : BaseZoneGenerator
{

    private ITerrainTextureManager _terrainTextureManager;
    public override async UniTask Generate(CancellationToken token)
    {
        await base.Generate(token);

        await _terrainTextureManager.DownloadAllTerrainTextures(token);

        for (int gx = 0; gx < _mapProvider.GetMap().BlockCount; gx++)
        {
            for (int gy = 0; gy < _mapProvider.GetMap().BlockCount; gy++)
            {
                _terrainTextureManager.SetOneTerrainPatchLayers(_terrainManager.GetTerrainPatch(gx, gy, true), token, true).Forget();
            }
            await UniTask.NextFrame(cancellationToken: token);
        }
        await UniTask.Delay(TimeSpan.FromSeconds(2.0f), cancellationToken: token);
    }
}