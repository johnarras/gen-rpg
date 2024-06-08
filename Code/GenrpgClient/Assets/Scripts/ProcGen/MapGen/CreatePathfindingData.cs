
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
using System;
using System.Xml.Schema;
using System.Reflection.Emit;
using Genrpg.Shared.Buildings.Settings;

public class CreatePathfindingData : BaseZoneGenerator
{

    protected IPathfindingService _pathfindingService;
    public override async UniTask Generate(CancellationToken token)
    {
        await base.Generate(token);
        try
        {
            int blockSize = PathfindingConstants.BlockSize;
            int pxsize = _mapProvider.GetMap().GetHwid() / blockSize;
            int pzsize = _mapProvider.GetMap().GetHhgt() / blockSize;

            bool[,] blockedCells = new bool[pxsize, pzsize];
            bool[,] nearBlockedCells = new bool[pxsize, pzsize];

            for (int x = 0; x < _mapProvider.GetMap().GetHwid(); x++)
            {
                if (x < MapConstants.MapEdgeSize || x >= _mapProvider.GetMap().GetHwid()-MapConstants.MapEdgeSize-1)
                {
                    continue;
                }
                int px = (x+1) / blockSize;
                if (px < 0 || px >= pxsize)
                {
                    continue;
                }
                for (int z = 0; z < _mapProvider.GetMap().GetHhgt(); z++)
                {
                    if (z < MapConstants.MapEdgeSize || z >= _mapProvider.GetMap().GetHhgt() - MapConstants.MapEdgeSize-1)
                    {
                        continue;
                    }
                    int pz = (z+1) / blockSize;
                    if (pz < 0 || pz >= pzsize)
                    {
                        continue;
                    }

                    float steepness = _terrainManager.GetSteepness(x, z);

                    if (steepness > PathfindingConstants.MaxSteepness)
                    {
                        blockedCells[px, pz] = true;
                    }

                    long worldObject = _md.mapObjects[x, z];

                    if (worldObject > 0 &&
                        (worldObject < MapConstants.GrassMinCellValue ||
                        worldObject > MapConstants.GrassMaxCellValue))
                    {
                        if (worldObject >= MapConstants.TreeObjectOffset && worldObject <
                            MapConstants.TreeObjectOffset + MapConstants.MapObjectOffsetMult)
                        {
                            long treeTypeId = worldObject - MapConstants.TreeObjectOffset;
                            TreeType ttype = _gameData.Get<TreeTypeSettings>(_gs.ch).Get(treeTypeId);
                            if (ttype != null && !ttype.HasFlag(TreeFlags.IsBush))
                            {
                                blockedCells[pz,px] = true;
                            }                           
                        }
                        else
                        {
                            blockedCells[pz, px] = true;
                        }
                    }
                }
            }

            // Now block out all building spawns.
            foreach (MapSpawn spawn in _mapProvider.GetSpawns().Data)
            {
                if (spawn.EntityTypeId == EntityTypes.Building)
                {
                    BuildingType btype = _gameData.Get<BuildingSettings>(null).Get(spawn.EntityId);
                    if (btype != null)
                    {
                        int buildingRadius = Math.Max(1, (btype.Radius + 1) / 2);
                        for (int x = (int)spawn.X - buildingRadius+1; x <= spawn.X + buildingRadius; x++)
                        {
                            int px = x / blockSize;
                            if (px < 0 || px >= pxsize)
                            {
                                continue;
                            }
                            for (int z = (int)spawn.Z - buildingRadius+1; z <= spawn.Z + buildingRadius; z++)
                            {
                                int pz = z / blockSize;
                                if (pz < 0 || pz >= pzsize)
                                {
                                    continue;
                                }

                                blockedCells[px, pz] = true;
                            }
                        }
                    }
                }
            }

            for (int x = 0; x < pxsize; x++)
            {
                for (int z = 0; z < pzsize; z++)
                {
                    if (blockedCells[x,z]
                        //|| (x > 0 && blockedCells[x - 1, z]) 
                        // || (x < pxsize-1 && blockedCells[x + 1, z]) 
                        //|| (z > 0 && blockedCells[x, z - 1])
                        // || (z < pzsize - 1  && blockedCells[x, z + 1])
                        )
                    {
                        nearBlockedCells[x, z] = true;
                    }
                }
            }

            byte[] output = _pathfindingService.ConvertGridToBytes(nearBlockedCells);

            int startLength = output.Length;

            output = CompressionUtils.CompressBytes(output);

            int endLength = output.Length;

            BinaryFileRepository repo = new BinaryFileRepository(_logService);

            string filename = MapUtils.GetMapObjectFilename(PathfindingConstants.Filename, _mapProvider.GetMap().Id, _mapProvider.GetMap().MapVersion);
            repo.SaveBytes(filename, output);

            string localPath = repo.GetPath(filename);
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
        catch (Exception e)
        {
            _logService.Exception(e, "Pathfinding");
        }
    }
}
