

using System.Threading;
using UnityEngine; // Needed

public class SetTerrainTextures : BaseZoneGenerator
{

    private ITerrainTextureManager _terrainTextureManager;
    public override async Awaitable Generate(CancellationToken token)
    {
        await base.Generate(token);

        await _terrainTextureManager.DownloadAllTerrainTextures(token);

        for (int gx = 0; gx < _mapProvider.GetMap().BlockCount; gx++)
        {
            for (int gy = 0; gy < _mapProvider.GetMap().BlockCount; gy++)
            {
                AwaitableUtils.ForgetAwaitable(_terrainTextureManager.SetOneTerrainPatchLayers(_terrainManager.GetTerrainPatch(gx, gy, true), token, true));
            }
            await Awaitable.NextFrameAsync(cancellationToken: token);
        }
        await Awaitable.WaitForSecondsAsync(2.0f, cancellationToken: token);
    }
}