


using Genrpg.Shared.MapServer.Entities;
using Genrpg.Shared.Pathfinding.Services;
using Genrpg.Shared.Pathfinding.Constants;
using System.Threading;
using Genrpg.Shared.Utils;
using UnityEngine;
using Genrpg.Shared.DataStores.Categories;
using Genrpg.Shared.DataStores.DataGroups;

public class LoadPathfinding : BaseZoneGenerator
{
    private IBinaryFileRepository _binaryFileRepo;
    public override async Awaitable Generate(CancellationToken token)
    {
        await base.Generate(token);
        string filename = MapUtils.GetMapObjectFilename(PathfindingConstants.Filename, _mapProvider.GetMap().Id, _mapProvider.GetMap().MapVersion);
        byte[] bytes = _binaryFileRepo.LoadBytes(filename);
        if (bytes != null)
        {
            OnDownloadPathfinding(bytes, null, token);
        }
        else
        {
            DownloadFileData ddata = new DownloadFileData()
            {
                IsImage = false,
                Handler = OnDownloadPathfinding,
                Category = EDataCategories.Worlds,
            };
            _fileDownloadService.DownloadFile(filename, ddata, token);
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

        _pathfindingService.SetPathfinding(_pathfindingService.ConvertBytesToGrid(decompressedBytes));

    }
}
