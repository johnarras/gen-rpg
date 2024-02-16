

using Cysharp.Threading.Tasks;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.Shared.Pathfinding.Services;
using Genrpg.Shared.Pathfinding.Constants;
using System.Threading;
using Genrpg.Shared.Utils;

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
            OnDownloadPathfinding(gs, bytes, null, token);
        }
        else
        {
            DownloadData ddata = new DownloadData() { IsImage = false, Handler= OnDownloadPathfinding };
            _assetService.DownloadFile(gs, filename, ddata, true, token);
        }
    }

    private void OnDownloadPathfinding (UnityGameState gs, object obj, object data, CancellationToken token)
    {

        byte[] compressedBytes = obj as byte[];

        if (compressedBytes == null)
        {
            return;
        }
        byte[] decompressedBytes = CompressionUtils.DecompressBytes(compressedBytes);

        gs.pathfinding = _pathfindingService.ConvertBytesToGrid(gs, decompressedBytes);

    }
}
