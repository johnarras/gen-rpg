
using System;


using Genrpg.Shared.MapServer.Entities;
using System.Threading;
using UnityEngine;
using ClientEvents;

public class LoadMinimap : BaseZoneGenerator
{

    public override async Awaitable Generate(CancellationToken token)
    {
        await base.Generate(token);
        try
        {
            BinaryFileRepository repo = new BinaryFileRepository(_logService);
            string filename = MapUtils.GetMapObjectFilename(MapConstants.MapFilename, _mapProvider.GetMap().Id, _mapProvider.GetMap().MapVersion);
            byte[] bytes = repo.LoadBytes(filename);
            if (bytes != null)
            {
                Texture2D tex = new Texture2D(2, 2);
                tex.LoadImage(bytes);
                OnDownloadMinimap(tex, null, token);
            }
            else
            {
                DownloadFileData ddata = new DownloadFileData() { IsImage = true, Handler= OnDownloadMinimap };
                _fileDownloadService.DownloadFile(filename, ddata, true, token);
            }
        }
        catch (Exception e)
        {
            _logService.Exception(e, "LoadMinimap");
        }
    }

    private void OnDownloadMinimap (object obj, object data, CancellationToken token)
    {
        Texture2D tex = obj as Texture2D;

        MinimapUI.SetTexture(tex);
    }
}
