
using System;
using System.IO;


using Genrpg.Shared.MapServer.Entities;
using System.Threading;
using UnityEngine; // Needed
using UI.Screens.Constants;
using System.Threading.Tasks;

public class ClearMapData : BaseZoneGenerator
{
    private IPlayerManager _playerManager;
    public override async Awaitable Generate (CancellationToken token)
    {
        await base.Generate(token);

        _playerManager.SetUnit(null);

        await _objectManager.Initialize(token);

        _terrainManager.Clear();

        RenderSettings.fog = false;
        RenderSettings.ambientIntensity = 1.0f;

        _assetService.ClearBundleCache(token);

        AwaitableUtils.ForgetAwaitable(CleanUpOldMapFolders(token));


        await Task.CompletedTask;
    }



    private async Awaitable CleanUpOldMapFolders(CancellationToken token)
    {
        if (_mapProvider.GetMap() == null)
        {
            return;
        }

        if (string.IsNullOrEmpty(UnityZoneGenService.LoadedMapId))
        {
            return;
        }


        string worldId = _mapProvider.GetMap().Id;
        int worldVersion = _mapProvider.GetMap().MapVersion;

        _logService.Info("Current Map: " + _mapProvider.GetMap().Id + " Version: " + _mapProvider.GetMap().MapVersion);

        string folder = MapUtils.GetMapFolder(worldId, worldVersion);

        folder = folder.Substring(0, folder.Length - 1);

        if (string.IsNullOrEmpty(folder))
        {
            return;
        }

        string desiredFolder = AssetUtils.GetPerisistentDataPath() + "/Data/" + folder;


        string parentFolder = desiredFolder.Substring(0, desiredFolder.Length - 5);

        string[] dirs = new string[0];

        try
        {
            dirs = Directory.GetDirectories(parentFolder);
        }
        catch (Exception e)
        {
            _logService.Exception(e, "ClearMap");
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
                    _logService.Exception(e, "NoDeleteOnClearMap");
                }
            }
            await Awaitable.NextFrameAsync(cancellationToken: token);
        }
    }
}