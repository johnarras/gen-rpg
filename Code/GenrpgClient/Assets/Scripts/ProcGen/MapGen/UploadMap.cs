using Assets.Scripts.MapTerrain;
using Cysharp.Threading.Tasks;
using Genrpg.Shared.Constants;
using Genrpg.Shared.Login.Messages.UploadMap;
using Genrpg.Shared.ProcGen.Entities;
using System.Threading;

public class UploadMap : BaseZoneGenerator
{
    private IWebNetworkService _webNetworkService;
    public override async UniTask Generate(UnityGameState gs, CancellationToken token)
    {

        await base.Generate(gs, token);
        for (int gx = 0; gx < gs.map.BlockCount; gx++)
        {
            for (int gy = 0; gy < gs.map.BlockCount; gy++)
            {
                UploadOneTerrainPatch(gs, gx, gy);
            }
        }

        await DelaySendMapSizes(gs, token);
    }

    private void UploadOneTerrainPatch(UnityGameState gs, int gx, int gy)
    {
        string env = gs.Env;

        TerrainPatchData patch = _terrainManager.GetTerrainPatch(gs, gx, gy, false);

        if (patch == null)
        {
            return;
        }

        if (patch.FullZoneIdList == null || patch.FullZoneIdList.Count < 1)
        {
            return;
        }

        string localFilePath = patch.GetFilePath(gs, true);
        string remoteFilePath = patch.GetFilePath(gs, false);

        LocalFileRepository repo = new LocalFileRepository(gs.logger);
        string localPath = repo.GetPath(localFilePath);
        FileUploadData fdata = new FileUploadData();
        fdata.GamePrefix = Game.Prefix;
        fdata.Env = _assetService.GetWorldDataEnv();
        fdata.IsWorldData = true;

        fdata.LocalPath = localPath;
        fdata.RemotePath = remoteFilePath;

        FileUploader.UploadFile(fdata);

    }


    private async UniTask DelaySendMapSizes(UnityGameState gs, CancellationToken token)
    {
        await UniTask.Delay(2000, cancellationToken: token);
        UploadMapCommand update = new UploadMapCommand()
        {
            Map = gs.map,
            SpawnData = gs.spawns,
            WorldDataEnv = _assetService.GetWorldDataEnv()
        };

        string oldMapId = gs.map.Id;
        gs.map.Id = "UploadedMap";
        await gs.repo.Save(gs.map);
        gs.map.Id = oldMapId;
        gs.spawns.Id = "UploadedSpawns";
        await gs.repo.Save(gs.spawns);
        gs.spawns.Id = oldMapId;
        _webNetworkService.SendClientWebCommand(update, _token);
        await UniTask.CompletedTask;
    }
}
	
