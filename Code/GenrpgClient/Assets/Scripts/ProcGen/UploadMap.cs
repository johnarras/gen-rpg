using Assets.Scripts.MapTerrain;
using System.Threading.Tasks;
using Genrpg.Shared.Constants;
using Genrpg.Shared.ProcGen.Entities;
using System.Threading;

public class UploadMap : BaseZoneGenerator
{
    public override async Task Generate(UnityGameState gs, CancellationToken token)
    {

        await base.Generate(gs, token);
        for (int gx = 0; gx < gs.map.BlockCount; gx++)
        {
            for (int gy = 0; gy < gs.map.BlockCount; gy++)
            {
                UploadOneTerrainPatch(gs, gx, gy);
            }
        }
    }

    private void UploadOneTerrainPatch(UnityGameState gs, int gx, int gy)
    {
        if (gx < 0 || gy < 0 || gs.md.terrainPatches == null ||
            gs.md.heights == null ||
            gs.md.alphas == null)
        {
            return;
        }

        string env = gs.Env;

        TerrainPatchData patch = gs.md.terrainPatches[gx, gy];

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

        string localPath = LocalFileRepository.GetPath(localFilePath);
        FileUploadData fdata = new FileUploadData();
        fdata.GamePrefix = Game.Prefix;

        if (env != EnvNames.Prod)
        {
            env = EnvNames.Dev;
        }

        fdata.Env = env;

     

        fdata.LocalPath = localPath;
        fdata.RemotePath = remoteFilePath;

        FileUploader.UploadFile(fdata);

    }
}
	
