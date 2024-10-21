using Genrpg.Shared.Logging.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using UnityEngine.Networking;
using UnityEngine;
using System.Threading.Tasks;
using Genrpg.Shared.Utils;
using Genrpg.Shared.DataStores.DataGroups;
using Genrpg.Shared.Client.Assets.Services;
using Assets.Scripts.Awaitables;

public class DownloadFileData
{
    public bool IsImage { get; set; }

    public bool ForceDownload { get; set; }

    public bool IsText { get; set; }

    public EDataCategories Category { get; set; }

    public OnDownloadHandler Handler { get; set; }

    public object Data { get; set; }

    public byte[] StartBytes { get; set; }

    public byte[] UncompressedBytes { get; set; }

    public string URLPrefixOverride { get; set; }
}

public class TypedDownloadFileData<T> : DownloadFileData
{
    public Action<T, DownloadFileData> OnDownloadAction { get; set; }
}


public class FileDownloadService : IFileDownloadService
{
    private IClientAppService _appService;
    private IModTextureService _modTextureService;
    protected IAwaitableService _awaitableService;
    public async Task Initialize(CancellationToken token)
    {
        if (!_appService.IsPlaying)
        {
            return;
        }

        await Task.CompletedTask;
    }

    private class InternalFileDownload
    {
        public string URLPrefix { get; private set; }
        public string FilePath { get; private set; }
        public string FullURL { get; private set; }
        public DownloadFileData DownloadData { get; private set; }
        public CancellationToken Token { get; private set; }

        public InternalFileDownload(DownloadFileData downloadData, string urlPrefix, string filePath, CancellationToken token)
        {
            URLPrefix = urlPrefix;
            FilePath = filePath;
            FullURL = URLPrefix + FilePath;
            DownloadData = downloadData;
            Token = token;
        }

    }

    private ILogService _logService;
    private IAssetService _assetService;
    private IBinaryFileRepository _binaryFileRepo = null;

    private int _fileDownloadingCount = 0;

    private const int _maxConcurrentDownloads = 2;
    private const int _retryTimes = 3;

    protected bool _isInitialized = false;

    private Dictionary<string, List<InternalFileDownload>> _downloading = new Dictionary<string, List<InternalFileDownload>>();

    protected HashSet<string> _failedDownloads = new HashSet<string>();
   
    // If it's in the cache, return it.
    // If it's a failed download, return nothing.
    // If it's in downloading, add the handler to the queue.	
    // If it's none of those, start the download.

    public void DownloadFile(string filePath, DownloadFileData downloadData, CancellationToken token)
    {
        if (!TokenUtils.IsValid(token))
        {
            return;
        }
        if (downloadData == null)
        {
            downloadData = new DownloadFileData();
        }

        if (string.IsNullOrEmpty(filePath))
        {
            if (downloadData.Handler != null)
            {
                downloadData.Handler(null, downloadData.Data, token);
            }
            return;
        }

        if (_failedDownloads.Contains(filePath))
        {
            if (downloadData.Handler != null)
            {
                downloadData.Handler(null, downloadData.Data, token);
            }
            return;
        }

        string contentRootURL = downloadData.URLPrefixOverride;

        if (string.IsNullOrEmpty(contentRootURL))
        {
            contentRootURL = _assetService.GetContentRootURL(downloadData.Category);
            if (string.IsNullOrEmpty(contentRootURL))
            {
                return;
            }
        }

        InternalFileDownload fileDownLoad = new InternalFileDownload(downloadData, contentRootURL, filePath, token);

        if (_downloading.ContainsKey(filePath))
        {
            List<InternalFileDownload> list = _downloading[filePath];
            if (list != null)
            {
                list.Add(fileDownLoad);
            }
            return;
        }
        else
        {
            List<InternalFileDownload> list = new List<InternalFileDownload>();
            list.Add(fileDownLoad);
            _downloading[filePath] = list;
            _awaitableService.ForgetAwaitable(DownloadFileInternal(fileDownLoad, token));
        }
    }

    private async Awaitable DownloadFileInternal(InternalFileDownload fileDownload, CancellationToken token)
    {

        if (fileDownload == null)
        {
            return;
        }
        Texture2D tex = null;
        string txt = null;

        System.Object obj = null;

        if (!fileDownload.DownloadData.ForceDownload)
        {
            fileDownload.DownloadData.StartBytes = _binaryFileRepo.LoadBytes(fileDownload.FilePath);
        }
        if (fileDownload.DownloadData.StartBytes == null || fileDownload.DownloadData.StartBytes.Length < 1)
        {
            for (int i = 0; i < _retryTimes; i++)
            {
                while (_fileDownloadingCount >= _maxConcurrentDownloads)
                {
                    await Awaitable.NextFrameAsync(cancellationToken: token);
                }
                _fileDownloadingCount++;
                using (UnityWebRequest request = UnityWebRequest.Get(fileDownload.FullURL))
                {
                    if (request == null)
                    {
                        _downloading.Remove(fileDownload.FilePath);
                        _fileDownloadingCount--;
                        return;
                    }
                    try
                    {
                        UnityWebRequestAsyncOperation asyncOp = request.SendWebRequest();

                        while (!asyncOp.isDone)
                        {
                            await Awaitable.NextFrameAsync(cancellationToken: token);
                        }
                    }
                    catch (Exception e)
                    {
                        _logService.Exception(e, "DownloadFile :" + fileDownload.FullURL);
                    }

                    DownloadHandler handler = request.downloadHandler;
                    fileDownload.DownloadData.StartBytes = handler.data;

                    txt = handler.text;
                    if (fileDownload.DownloadData.StartBytes != null &&
                        fileDownload.DownloadData.StartBytes.Length < 300)
                    {
                        try
                        {
                            txt = Encoding.UTF8.GetString(fileDownload.DownloadData.StartBytes);
                        }
                        catch (Exception e)
                        {
                            _logService.Exception(e, "DownloadToBytesToText");
                        }
                        if (txt == null || txt.IndexOf("BlobNotFound") >= 0)
                        {
                            fileDownload.DownloadData.StartBytes = null;
                            request.Dispose();
                            if (i < _retryTimes - 1)
                            {
                                _downloading.Remove(fileDownload.FilePath);
                                _fileDownloadingCount--;
                                await Awaitable.WaitForSecondsAsync(1.0f, cancellationToken: token);
                            }
                        }
                    }

                    if (fileDownload.DownloadData.StartBytes != null)
                    {
                        byte[] finalBytes = fileDownload.DownloadData.StartBytes;

                        await Awaitable.NextFrameAsync(cancellationToken: token);
                        _binaryFileRepo.SaveBytes(fileDownload.FilePath, finalBytes);
                        await Awaitable.NextFrameAsync(cancellationToken: token);
                        fileDownload.DownloadData.UncompressedBytes = finalBytes;

                        request.Dispose();
                        _fileDownloadingCount--;
                        break;
                    }
                    else
                    {
                        _fileDownloadingCount--;
                    }
                }
            }
        }
        else
        {
            fileDownload.DownloadData.UncompressedBytes = fileDownload.DownloadData.StartBytes;
        }

        if (fileDownload.DownloadData.UncompressedBytes != null)
        {
            if (fileDownload.DownloadData.IsImage)
            {
                await Awaitable.NextFrameAsync(cancellationToken: token);
                tex = new Texture2D(2, 2);
                tex.LoadImage(fileDownload.DownloadData.UncompressedBytes);
                _modTextureService.SetupTexture(tex);
                obj = tex;
            }
            else if (fileDownload.DownloadData.IsText ||
                fileDownload.FilePath.IndexOf(".txt") > 0)
            {
                if (string.IsNullOrEmpty(txt))
                {
                    txt = System.Text.Encoding.UTF8.GetString(fileDownload.DownloadData.UncompressedBytes);
                }
                obj = txt;
            }
            else
            {
                obj = fileDownload.DownloadData.UncompressedBytes;
            }
        }
        else
        {
            _failedDownloads.Add(fileDownload.FilePath);
        }

        if (_downloading.ContainsKey(fileDownload.FilePath))
        {
            List<InternalFileDownload> allFileDownloads = _downloading[fileDownload.FilePath];
            _downloading.Remove(fileDownload.FilePath);
            foreach (InternalFileDownload fd in allFileDownloads)
            {
                if (fd.DownloadData == null || fd.DownloadData.Handler == null || !TokenUtils.IsValid(token))
                {
                    continue;
                }

                fd.DownloadData.Handler(obj, fd.DownloadData.Data, token);
            }
        }
    }

    public void DownloadTypedFile<T>(string url, Action<T> handler, EDataCategories category, CancellationToken token)
    {

        string newUrl = typeof(T).Name.ToLower() + "/" + url;
        DownloadFileData fileData = new DownloadFileData()
        {
            Data = handler,
            IsText = true,
            ForceDownload = false,
            Category = category,
            Handler =
              (obj, data, token) =>
              {
                  if (obj == null || string.IsNullOrEmpty(obj.ToString()))
                  {
                      handler(default(T));
                      return;
                  }

                  T t = SerializationUtils.Deserialize<T>(obj.ToString());
                  handler(t);
              },
        };

        DownloadFile(newUrl, fileData, token);
    }
}