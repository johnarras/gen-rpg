
using System;
using System.IO;

using UnityEngine;
using Cysharp.Threading.Tasks;
using Genrpg.Shared.MapServer.Entities;
using System.Threading;

public class ClearMapData : BaseZoneGenerator
{
    public override async UniTask Generate (UnityGameState gs, CancellationToken token)
    {
        await base.Generate(gs, token);

        PlayerObject.Destroy();

        await _objectManager.Setup(gs, token);

        _terrainManager.Clear(gs);

        RenderSettings.fog = false;

        _assetService.ClearMemoryCache(gs,token);

        CleanUpOldMapFolders(gs).Forget();

        await UniTask.CompletedTask;
	}



    private async UniTask CleanUpOldMapFolders(UnityGameState gs)
    {
        if (gs.map == null)
        {
            return;
        }

        if (string.IsNullOrEmpty(UnityZoneGenService.LoadedMapId))
        {
            return;
        }


        string worldId = gs.map.Id;
        int worldVersion = gs.map.MapVersion;

        gs.logger.Info("Current Map: " + gs.map.Id + " Version: " + gs.map.MapVersion);

        string folder = MapUtils.GetMapFolder(gs, worldId, worldVersion);

        folder = folder.Substring(0, folder.Length - 1);

        if (string.IsNullOrEmpty(folder))
        {
            return;
        }

        string desiredFolder = UnityAssetService.GetPerisistentDataPath() + "/Data/" + folder;


        string parentFolder = desiredFolder.Substring(0, desiredFolder.Length - 5);

        string[] dirs = new string[0];

        try
        {
            dirs = Directory.GetDirectories(parentFolder);
        }
        catch (Exception e)
        {
            gs.logger.Exception(e, "ClearMap");
        }
        foreach (string dir in dirs)
        {
            if (dir.IndexOf(folder) >= 0)
            {
            }
            else
            {
                try
                {
                    Directory.Delete(dir, true);
                }
                catch (Exception e)
                {
                    gs.logger.Exception(e, "NoDeleteOnClearMap");
                }
            }
            await UniTask.NextFrame();
        }
    }
}