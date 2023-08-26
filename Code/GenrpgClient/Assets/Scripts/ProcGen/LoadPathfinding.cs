
using System;
using Cysharp.Threading.Tasks;
using Genrpg.Shared.MapServer.Entities;
using Entities.Assets;
using Genrpg.Shared.Pathfinding.Services;
using Genrpg.Shared.Pathfinding.Constants;
using System.Threading;

public class LoadPathfinding : BaseZoneGenerator
{

    protected IPathfindingService _pathfindingService;
    public override async UniTask Generate(UnityGameState gs, CancellationToken token)
    {
        await base.Generate(gs, token);
        LocalFileRepository repo = new LocalFileRepository(gs.logger);
        string filename = MapUtils.GetMapObjectFilename(gs, PathfindingConstants.Filename, gs.map.Id, gs.map.MapVersion);
        byte[] bytes = repo.LoadBytes(filename);
        if (bytes != null)
        {
            OnDownloadPathfinding(gs, "", bytes, null, token);
        }
        else
        {
            DownloadData ddata = new DownloadData() { IsImage = false, Handler= OnDownloadPathfinding };
            _assetService.DownloadFile(gs, filename, ddata, token);
        }
    }

    private void OnDownloadPathfinding (UnityGameState gs, string url, object obj, object data, CancellationToken token)
    {

        byte[] bytes = obj as byte[];

        if (bytes == null)
        {
            return;
        }

        gs.pathfinding = _pathfindingService.ConvertBytesToGrid(gs, bytes);

    }
}
