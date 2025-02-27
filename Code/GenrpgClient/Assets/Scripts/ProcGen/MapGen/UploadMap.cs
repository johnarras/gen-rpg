using Assets.Scripts.MapTerrain;

using Genrpg.Shared.Constants;
using Genrpg.Shared.MapServer.WebApi.UploadMap;
using System.Threading;
using UnityEngine;

public class UploadMap : BaseZoneGenerator
{
    private IClientWebService _webNetworkService;
    private IClientAppService _clientAppService;
    private IBinaryFileRepository _binaryFileRepository;
    public override async Awaitable Generate(CancellationToken token)
    {

        await base.Generate(token);
        for (int gx = 0; gx < _mapProvider.GetMap().BlockCount; gx++)
        {
            for (int gy = 0; gy < _mapProvider.GetMap().BlockCount; gy++)
            {
                UploadOneTerrainPatch(gx, gy);
            }
        }

        await DelaySendMapSizes(token);
    }

    private void UploadOneTerrainPatch(int gx, int gy)
    {
        TerrainPatchData patch = _terrainManager.GetTerrainPatch(gx, gy, false);

        if (patch == null)
        {
            return;
        }

        if (patch.FullZoneIdList == null || patch.FullZoneIdList.Count < 1)
        {
            return;
        }

        string localFilePath = patch.GetFilePath(true);
        string remoteFilePath = patch.GetFilePath(false);

        string localPath = _binaryFileRepository.GetPath(localFilePath);
        FileUploadData fdata = new FileUploadData();
        fdata.GamePrefix = Game.Prefix;
        fdata.Env = _assetService.GetWorldDataEnv();
        fdata.IsWorldData = true;

        fdata.LocalDataPath = _clientAppService.DataPath;
        fdata.LocalPath = localPath;
        fdata.RemotePath = remoteFilePath;
        if (gx % 10 == 0)
        {
            fdata.WaitForComplete = true;
        }
        FileUploader.UploadFile(fdata);

    }


    private async Awaitable DelaySendMapSizes(CancellationToken token)
    {
        await Awaitable.WaitForSecondsAsync(2.0f, cancellationToken: token);
        UploadMapRequest update = new UploadMapRequest()
        {
            Map = _mapProvider.GetMap(),
            SpawnData = _mapProvider.GetSpawns(),
            WorldDataEnv = _assetService.GetWorldDataEnv()
        };

        string oldMapId = _mapProvider.GetMap().Id;
        _mapProvider.GetMap().Id = "UploadedMap";
        await _repoService.Save(_mapProvider.GetMap());
        _mapProvider.GetMap().Id = oldMapId;
        _mapProvider.GetSpawns().Id = "UploadedSpawns";
        await _repoService.Save(_mapProvider.GetSpawns());
        _mapProvider.GetSpawns().Id = oldMapId;
        _webNetworkService.SendClientUserWebRequest(update, _token);
        
    }
}
	
