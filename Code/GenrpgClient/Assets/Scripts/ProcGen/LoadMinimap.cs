
using System;
using System.Threading.Tasks;

using Genrpg.Shared.MapServer.Entities;
using System.Threading;
using UnityEngine; // Needed

public class LoadMinimap : BaseZoneGenerator
{

    public override async Task Generate(UnityGameState gs, CancellationToken token)
    {
        await base.Generate(gs, token);
        try
        {
            LocalFileRepository repo = new LocalFileRepository(gs.logger);
            string filename = MapUtils.GetMapObjectFilename(gs, MapConstants.MapFilename, gs.map.Id, gs.map.MapVersion);
            byte[] bytes = repo.LoadBytes(filename);
            if (bytes != null)
            {
                Texture2D tex = new Texture2D(2, 2);
                tex.LoadImage(bytes);
                OnDownloadMinimap(gs, "", tex, null, token);
            }
            else
            {
                DownloadData ddata = new DownloadData() { IsImage = true, Handler= OnDownloadMinimap };
                _assetService.DownloadFile(gs, filename, ddata, token);
            }
        }
        catch (Exception e)
        {
            gs.logger.Exception(e, "LoadMinimap");
        }
    }

    private void OnDownloadMinimap (UnityGameState gs, string url, object obj, object data, CancellationToken token)
    {
        Texture2D tex = obj as Texture2D;
        if (tex == null)
        {
            return;
        }
        CreateMinimap cm = new CreateMinimap();
        cm.ShowMinimap(gs, tex);
    }
}
