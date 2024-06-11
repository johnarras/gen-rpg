


using Genrpg.Shared.MapServer.Entities;
using Genrpg.Shared.Pathfinding.Services;
using Genrpg.Shared.Pathfinding.Constants;
using System.Threading;
using Genrpg.Shared.Utils;
using UnityEngine;

public class LoadPathfinding : BaseZoneGenerator
{

    protected IPathfindingService _pathfindingService;
    public override async Awaitable Generate(CancellationToken token)
    {
        await base.Generate(token);
        BinaryFileRepository repo = new BinaryFileRepository(_logService);
        string filename = MapUtils.GetMapObjectFilename(PathfindingConstants.Filename, _mapProvider.GetMap().Id, _mapProvider.GetMap().MapVersion);
        byte[] bytes = repo.LoadBytes(filename);
        if (bytes != null)
        {
            OnDownloadPathfinding(bytes, null, token);
        }
        else
        {
            DownloadFileData ddata = new DownloadFileData() { IsImage = false, Handler= OnDownloadPathfinding };
            _fileDownloadService.DownloadFile(filename, ddata, true, token);
        }
    }

    private void OnDownloadPathfinding (object obj, object data, CancellationToken token)
    {

        byte[] compressedBytes = obj as byte[];

        if (compressedBytes == null)
        {
            return;
        }
        byte[] decompressedBytes = CompressionUtils.DecompressBytes(compressedBytes);

        _mapProvider.SetPathfinding(_pathfindingService.ConvertBytesToGrid(decompressedBytes));

    }
}
