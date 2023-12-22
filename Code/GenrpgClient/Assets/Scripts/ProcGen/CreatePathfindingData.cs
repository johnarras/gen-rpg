
using Cysharp.Threading.Tasks;
using Genrpg.Shared.Constants;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.Shared.Pathfinding.Constants;
using Genrpg.Shared.Pathfinding.Services;
using Genrpg.Shared.ProcGen.Entities;
using System.Threading;
using Genrpg.Shared.Utils;
using Genrpg.Shared.ProcGen.Settings.Trees;
using MessagePack.Formatters;
using System.Collections.Concurrent;
using Genrpg.Shared.Spawns.WorldData;
using Genrpg.Shared.Entities.Constants;

public class CreatePathfindingData : BaseZoneGenerator
{

    protected IPathfindingService _pathfindingService;
    public override async UniTask Generate(UnityGameState gs, CancellationToken token)
    {
        await base.Generate(gs, token);

        int blockSize = PathfindingConstants.BlockSize;
        int xsize = gs.map.GetHwid() / blockSize;
        int zsize = gs.map.GetHhgt() / blockSize;
        
        bool[,] blockedCells = new bool[xsize,zsize];
        bool[,] nearBlockedCells = new bool[xsize,zsize];

        for (int x = 0; x < xsize; x += blockSize)
        {
            for (int z = 0; z < zsize; z += blockSize)
            {
                int gx = x / blockSize;
                int gz = z / blockSize;

                for (int xx = gx; xx < gx+blockSize; xx++)
                {
                    for (int zz = gz; zz < gz+blockSize; zz++)
                    {
                        float steepness = gs.md.GetSteepness(gs, xx, zz);

                        if (steepness > PathfindingConstants.MaxSteepness)
                        {
                            blockedCells[gx, gz] = true;
                        }

                        long worldObject = gs.md.mapObjects[xx,zz];

                        if (worldObject > 0 &&
                            (worldObject < MapConstants.GrassMinCellValue ||
                            worldObject > MapConstants.GrassMaxCellValue))
                        {
                            if (worldObject >= MapConstants.TreeObjectOffset && worldObject <
                                MapConstants.TreeObjectOffset + MapConstants.MapObjectOffsetMult)
                            {
                                long treeTypeId = worldObject - MapConstants.TreeObjectOffset;
                                TreeType ttype = gs.data.GetGameData<TreeTypeSettings>(gs.ch).GetTreeType(treeTypeId);
                                if (ttype == null || !ttype.HasFlag(TreeFlags.IsBush))
                                {
                                    blockedCells[gz,gx] = true;
                                }
                            }
                            else
                            {
                                blockedCells[gz, gx] = true;
                            }
                        }
                    }
                }
            }
        }

        // Now block out all building spawns.
        foreach (MapSpawn spawn in gs.spawns.Data)
        {
           
            if (spawn.EntityTypeId == EntityTypes.Building)
            {
                int buildingRadius = 3;
                for (int xx = (int)spawn.X-buildingRadius; xx <= spawn.X + buildingRadius; xx++)
                {
                    if (xx < 0 || xx >= gs.map.GetHwid())
                    {
                        continue;
                    }
                    for (int yy = (int)spawn.Z-buildingRadius; yy <= spawn.Z + buildingRadius; yy++)
                    {
                        if (yy < 0 || yy >= gs.map.GetHhgt())
                        {
                            continue;
                        }

                        blockedCells[xx/blockSize, yy/blockSize] = true;
                    }
                }
            }
        }

        for (int x = 0; x < xsize; x++)
        {
            for (int z = 0; z < zsize; zsize++)
            {
                if ((x > 0 && blockedCells[x-1,z]) ||
                    (x < gs.map.GetHwid()-1 && blockedCells[x+1,z]) ||
                    (z > 0 && blockedCells[x,z-1]) ||
                    (z < gs.map.GetHhgt()-1 && blockedCells[x,z+1]))
                {
                    nearBlockedCells[x, z] = true;
                }
            }
        }
        byte[] output = _pathfindingService.ConvertGridToBytes(gs, nearBlockedCells);

        int startLength = output.Length;

        output = CompressionUtils.CompressBytes(output);

        int endLength = output.Length;

        LocalFileRepository repo = new LocalFileRepository(gs.logger);

        string filename = MapUtils.GetMapObjectFilename(gs, PathfindingConstants.Filename, gs.map.Id, gs.map.MapVersion);
        repo.SaveBytes(filename, output);

        string localPath = LocalFileRepository.GetPath(filename);
        string remotePath = filename;
        FileUploadData fdata = new FileUploadData();
        fdata.GamePrefix = Game.Prefix;
        fdata.Env = _assetService.GetWorldDataEnv();
        fdata.LocalPath = localPath;
        fdata.RemotePath = remotePath;
        fdata.IsWorldData = true;

        FileUploader.UploadFile(fdata);
        await UniTask.CompletedTask;
    }
}
