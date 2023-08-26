
using Cysharp.Threading.Tasks;
using Genrpg.Shared.Constants;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.Shared.Pathfinding.Constants;
using Genrpg.Shared.Pathfinding.Services;
using Genrpg.Shared.ProcGen.Entities;
using System.Threading;

public class CreatePathfindingData : BaseZoneGenerator
{

    protected IPathfindingService _pathfindingService;
    public override async UniTask Generate(UnityGameState gs, CancellationToken token)
    {
        await base.Generate(gs, token);
        bool[,] blockedCells = new bool[gs.map.GetHwid(), gs.map.GetHhgt()];

        bool[,] nearBlockedCells = new bool[gs.map.GetHwid(), gs.map.GetHhgt()];


        for (int x = 0; x < gs.map.GetHwid(); x++)
        {
            for (int y = 0; y < gs.map.GetHhgt(); y++)
            {

                float steepness = gs.md.GetSteepness(gs, x, y);

                if (steepness > PathfindingConstants.MaxSteepness)
                {
                    blockedCells[x, y] = true;
                    continue;
                }

                long worldObject = gs.md.mapObjects[x, y];

                if (worldObject > 0 && 
                    (worldObject < MapConstants.GrassMinCellValue ||
                    worldObject > MapConstants.GrassMaxCellValue))
                {
                    if (worldObject >= MapConstants.TreeObjectOffset && worldObject <
                        MapConstants.TreeObjectOffset + MapConstants.MapObjectOffsetMult)
                    {
                        long treeTypeId = worldObject - MapConstants.TreeObjectOffset;
                        TreeType ttype = gs.data.GetGameData<ProcGenSettings>().GetTreeType (treeTypeId);
                        if (ttype == null || !ttype.HasFlag(TreeFlags.IsBush))
                        {
                            blockedCells[y,x] = true;
                        }
                    }
                    else
                    {
                        blockedCells[y,x] = true;
                    }
                }
            }
        }

        for (int x = 0; x < gs.map.GetHwid(); x++)
        {
            for (int y = 0; y < gs.map.GetHhgt(); y++)
            {
                if ((x > 0 && blockedCells[x-1,y]) ||
                    (x < gs.map.GetHwid()-1 && blockedCells[x+1,y]) ||
                    (y > 0 && blockedCells[x,y-1]) ||
                    (y < gs.map.GetHhgt()-1 && blockedCells[x,y+1]))
                {
                    nearBlockedCells[x, y] = true;
                }
            }
        }
        byte[] output = _pathfindingService.ConvertGridToBytes(gs, nearBlockedCells);

        LocalFileRepository repo = new LocalFileRepository(gs.logger);

        string filename = MapUtils.GetMapObjectFilename(gs, PathfindingConstants.Filename, gs.map.Id, gs.map.MapVersion);
        repo.SaveBytes(filename, output);



        string localPath = LocalFileRepository.GetPath(filename);
        string remotePath = filename;
        FileUploadData fdata = new FileUploadData();
        fdata.GamePrefix = Game.Prefix;
        fdata.Env = gs.Env;
        if (fdata.Env != EnvNames.Prod)
        {
            fdata.Env = EnvNames.Dev;
        }
        fdata.LocalPath = localPath;
        fdata.RemotePath = remotePath;

        FileUploader.UploadFile(fdata);
        await UniTask.CompletedTask;
    }
}
