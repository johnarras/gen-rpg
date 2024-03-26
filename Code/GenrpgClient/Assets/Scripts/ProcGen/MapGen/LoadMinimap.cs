
using System;
using Cysharp.Threading.Tasks;

using Genrpg.Shared.MapServer.Entities;
using System.Threading;
using UnityEngine;

public class LoadMinimap : BaseZoneGenerator
{

    public override async UniTask Generate(UnityGameState gs, CancellationToken token)
    {
        await base.Generate(gs, token);
        try
        {
            BinaryFileRepository repo = new BinaryFileRepository(_logService);
            string filename = MapUtils.GetMapObjectFilename(gs, MapConstants.MapFilename, gs.map.Id, gs.map.MapVersion);
            byte[] bytes = repo.LoadBytes(filename);
            if (bytes != null)
            {
                Texture2D tex = new Texture2D(2, 2);
                tex.LoadImage(bytes);
                OnDownloadMinimap(gs, tex, null, token);
            }
            else
            {
                DownloadFileData ddata = new DownloadFileData() { IsImage = true, Handler= OnDownloadMinimap };
                _fileDownloadService.DownloadFile(gs, filename, ddata, true, token);
            }
        }
        catch (Exception e)
        {
            _logService.Exception(e, "LoadMinimap");
        }
    }

    private void OnDownloadMinimap (UnityGameState gs, object obj, object data, CancellationToken token)
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
