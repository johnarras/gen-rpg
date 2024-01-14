using GEntity = UnityEngine.GameObject;
using UnityEngine.U2D;
using UnityEngine.Networking;
using System.Collections.Generic;
using System;
using System.Text;
using System.Linq;
using Cysharp.Threading.Tasks;
using Scripts.Assets.Assets.Constants;
using System.Threading;
using UnityEngine;
using System.IO;
using Genrpg.Shared.Logs.Interfaces;
using Genrpg.Shared.Utils;
using ClientEvents;
#if UNITY_EDITOR
using UnityEditor;
#endif

public delegate void OnDownloadHandler(UnityGameState gs, string url, object obj, object data, CancellationToken token);

public delegate void SpriteListDelegate (UnityGameState gs, Sprite[] sprites);

public class FileDownload
{
    public string URLPrefix { get; private set; }
    public string FilePath { get; private set; }
    public string FullURL { get; private set; }
    public DownloadData DownloadData { get; private set; }
    public CancellationToken Token { get; private set; }
    public FileDownload(DownloadData downloadData, string urlPrefix, string filePath, CancellationToken token)
    {
        URLPrefix = urlPrefix;
        FilePath = filePath;
        FullURL = URLPrefix + FilePath;
        DownloadData = downloadData;
        Token = token;
    }

}

public class AtlasSpriteDownload
{
    public string atlasName;
    public string spriteName;
    public OnDownloadHandler finalHandler;
    public object data;
}

public class BundleDownload
{
	public string url;
	public string bundleName;
	public string assetName;
	public OnDownloadHandler handler;
	public System.Object data;
    public GEntity parent;
}

public class BundleCacheData
{
    public string name;
    public AssetBundle assetBundle;
    public DateTime lastUsed = DateTime.UtcNow;
    public int LoadingCount;
    public Dictionary<string, object> LoadedAssets = new Dictionary<string, object>();
    public List<object> Instances = new List<object>();
    public bool KeepLoaded = false;
}


public class ClientAssetCounts
{
    public long BundlesLoaded = 0;
    public long BundlesUnloaded = 0;
    public long ObjectsLoaded = 0;
    public long ObjectsUnloaded = 0;

}

public class UnityAssetService : IAssetService
{
    private ILogSystem _logger;

    private static float _loadDelayChance = 0.03f;

    private UnityGameState _gs;

    private const int _maxConcurrentDownloads = 2;

    private const int _maxConcurrentExistingDownloads = 5;

    private const int _maxConcurrentBundleDownloads = 3;

    private const int _retryTimes = 3;

    protected bool _isInitialized = false;

    protected Dictionary<string, List<FileDownload>> _downloading = new Dictionary<string, List<FileDownload>>();

    protected HashSet<string> _failedDownloads = new HashSet<string>();

    protected Dictionary<string, BundleCacheData> _bundleCache = new Dictionary<string, BundleCacheData>();

    protected Dictionary<string, List<BundleDownload>> _bundleDownloading = new Dictionary<string, List<BundleDownload>>();

    protected Dictionary<string, List<BundleDownload>> _bundleDownloadQueue = new Dictionary<string, List<BundleDownload>>();


    protected HashSet<string> _bundleFailedDownloads = new HashSet<string>();

    protected HashSet<string> _failedResourceLoads = new HashSet<string>();

    protected Dictionary<string, UnityEngine.Object> _loadedResources = new Dictionary<string, UnityEngine.Object>();

#if UNITY_EDITOR
    protected HashSet<string> _failedAssetPathLoads = new HashSet<string>();
#endif
    protected Dictionary<string, SpriteAtlas> _atlasCache = new Dictionary<string, SpriteAtlas>();
    protected Dictionary<string, Sprite[]> _spriteListCache = new Dictionary<string, Sprite[]>();
    protected Dictionary<string, Sprite> _spriteCache = new Dictionary<string, Sprite>();

    protected BundleVersions _bundleVersions = null;
    protected BundleUpdateInfo _bundleUpdateInfo = null;


    private LocalFileRepository _localRepo = null;

    int _fileDownloadingCount = 0;
    private Queue<BundleDownload> _existingDownloads = new Queue<BundleDownload>();


    private string _assetURLPrefix = null;
    private string _contentDataEnv = null;
    private string _worldDataEnv = null;


    private ClientAssetCounts _assetCounts = new ClientAssetCounts();

    public ClientAssetCounts GetAssetCounts()
    {
        return _assetCounts;
    }

    public string GetWorldDataEnv() 
    { 
        return _worldDataEnv; 
    }
    public string GetContentDataEnv() 
    { 
        return _contentDataEnv; 
    }

    /// <summary>
    /// Downloads the asset bundle list.
    /// </summary>
    /// <param name="gs"></param>
	public void Init(UnityGameState gs, string assetURLPrefix, string contentDataEnv, string worldDataEnv, CancellationToken token)
    {
        if (!AppUtils.IsPlaying)
        {
            return;
        }

        _gs = gs;
        _logger = gs.logger;
        _assetURLPrefix = assetURLPrefix;
        _contentDataEnv = contentDataEnv;
        _worldDataEnv = worldDataEnv;
        _assetParent = GEntityUtils.FindSingleton(AssetConstants.GlobalAssetParent, true);
        SpriteAtlasManager.atlasRequested += DummyRequestAtlas;
        SpriteAtlasManager.atlasRegistered += DummuRegisterAtlas;
        _localRepo = new LocalFileRepository(gs.logger);
        AssetUtils.GetPerisistentDataPath();
        LoadLastSaveTimeFile(gs, token);

        for (int i = 0; i < _maxConcurrentExistingDownloads; i++)
        {
            LoadFromExistingBundles(gs, true, token).Forget();
        }
        for (int i = 0; i < _maxConcurrentBundleDownloads; i++)
        {
            DownloadNewBundles(gs, token).Forget();
        }

        IncrementalClearMemoryCache(gs, token).Forget();
    }

    public bool IsDownloading(UnityGameState g)
    {
        return _downloading.Count > 0 ||
            _existingDownloads.Count > 0 ||
            _bundleDownloading.Keys.Count > 0 ||
            _bundleDownloadQueue.Keys.Count > 0;
    }

    private void DummyRequestAtlas(string tag, System.Action<SpriteAtlas> callback)
    {

    }

    private void DummuRegisterAtlas(SpriteAtlas callback)
    {

    }

    public void ClearBundleCache(UnityGameState gs, CancellationToken token)
    {
        var newBundleCache = new Dictionary<string, BundleCacheData>();

        foreach (var item in _bundleCache.Keys)
        {
            var bdata = _bundleCache[item];

            if (bdata.assetBundle != null)
            {
                if (bdata.KeepLoaded)
                {
                    newBundleCache[bdata.name] = bdata;
                }
                else
                {
                    bdata.assetBundle.Unload(true);
                    _assetCounts.BundlesUnloaded++;
                    GEntityUtils.Destroy(bdata.assetBundle);
                }
            }
        }

        _bundleCache = newBundleCache;
        AssetUtils.UnloadUnusedAssets(token).Forget();
    }


    protected async UniTask IncrementalClearMemoryCache(UnityGameState gs, CancellationToken token)
    {
        while (true)
        {
            await UniTask.Delay(5000, cancellationToken: token);

            int removeCount = 0;
            List<string> bundleCacheKeys = _bundleCache.Keys.ToList();
            foreach (string item in bundleCacheKeys)
            {
                var bundle = _bundleCache[item];
                if (bundle.LoadingCount < 1 &&
                    bundle.assetBundle != null &&
                    !bundle.KeepLoaded &&
                    bundle.lastUsed < DateTime.UtcNow.AddSeconds(-20))
                {
                    if (bundle.Instances.Any(x => x.Equals(null)))
                    {
                        bundle.Instances = bundle.Instances.Where(x => !x.Equals(null)).ToList();
                    }
                    if (bundle.Instances.Count > 0)
                    {
                        continue;
                    }

                    bundle.assetBundle.Unload(true);
                    _assetCounts.BundlesUnloaded++;
                    GEntityUtils.Destroy(bundle.assetBundle);
                    _bundleCache.Remove(item);
                    removeCount++;
                    if (removeCount > 5)
                    {
                        break;
                    }
                    await UniTask.NextFrame( cancellationToken: token);
                }
            }
            if (removeCount > 0)
            {
                //await AsyncUnloadAssets(token);
            }
        }
    }

    private async UniTask AsyncUnloadAssets(CancellationToken token)
    {
        await AssetUtils.UnloadUnusedAssets(token);
    }


    // If it's in the cache, return it.
    // If it's a failed download, return nothing.
    // If it's in downloading, add the handler to the queue.	
    // If it's none of those, start the download.


    public void DownloadFile(UnityGameState gs, string filePath, DownloadData downloadData, bool worldData, CancellationToken token)
    {
        if (!TokenUtils.IsValid(token))
        {
            return;
        }
        if (downloadData == null)
        {
            downloadData = new DownloadData();
        }

        if (string.IsNullOrEmpty(filePath))
        {
            if (downloadData.Handler != null)
            {
                downloadData.Handler(gs, filePath, null, downloadData.Data, token);
            }
            return;
        }

        if (_failedDownloads.Contains(filePath))
        {
            if (downloadData.Handler != null)
            {
                downloadData.Handler(gs, filePath, null, downloadData.Data, token);
            }
            return;
        }

        string urlPrefix = downloadData.URLPrefixOverride;

        if (string.IsNullOrEmpty(urlPrefix))
        {
            urlPrefix = GetArtURLPrefix(worldData);
        }

        var fileDownLoad = new FileDownload(downloadData, urlPrefix, filePath, token);

        if (_downloading.ContainsKey(filePath))
        {
            var list = _downloading[filePath];
            if (list != null)
            {
                list.Add(fileDownLoad);
            }
            return;
        }
        else
        {
            var list = new List<FileDownload>();
            list.Add(fileDownLoad);
            _downloading[filePath] = list;
            StartDownloadFile(gs, fileDownLoad, token).Forget();
        }
    }

    public static string GetCachedFilenameFromUrl(string url)
    {
        if (string.IsNullOrEmpty(url))
        {
            return "";
        }

        while (url.IndexOf("/") >= 0)
        {
            url = url.Replace("/", "_");
        }
        while (url.IndexOf("\\") >= 0)
        {
            url = url.Replace("\\", "_");
        }
        return AssetUtils.GetPerisistentDataPath() + "/" + url;
    }

    private async UniTask StartDownloadFile(UnityGameState gs, FileDownload fileDownload, CancellationToken token)
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
            fileDownload.DownloadData.StartBytes = _localRepo.LoadBytes(fileDownload.FilePath);
        }
        if (fileDownload.DownloadData.StartBytes == null || fileDownload.DownloadData.StartBytes.Length < 1)
        {
            for (int i = 0; i < _retryTimes; i++)
            {
                while (_fileDownloadingCount >= _maxConcurrentDownloads)
                {
                    await UniTask.NextFrame( cancellationToken: token);
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
                            await UniTask.NextFrame( cancellationToken: token);
                        }
                    }
                    catch (Exception e)
                    {
                        gs.logger.Exception(e, "DownloadFile :" + fileDownload.FullURL);
                    }
                    var handler = request.downloadHandler;
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
                            _logger.Exception(e, "DownloadToBytesToText");
                        }
                        if (txt == null || txt.IndexOf("BlobNotFound") >= 0)
                        {
                            fileDownload.DownloadData.StartBytes = null;
                            request.Dispose();
                            if (i < _retryTimes - 1)
                            {
                                _downloading.Remove(fileDownload.FilePath);
                                _fileDownloadingCount--;
                                await UniTask.Delay(TimeSpan.FromSeconds(1.0f), cancellationToken: token);
                            }
                        }
                    }

                    if (fileDownload.DownloadData.StartBytes != null)
                    {
                        byte[] finalBytes = fileDownload.DownloadData.StartBytes;

                        await UniTask.NextFrame(cancellationToken: token);
                        _localRepo.SaveBytes(fileDownload.FilePath, finalBytes);
                        await UniTask.NextFrame(cancellationToken: token);
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
                await UniTask.NextFrame(cancellationToken: token);
                tex = new Texture2D(2, 2);
                tex.LoadImage(fileDownload.DownloadData.UncompressedBytes);
                TextureUtils.SetupTexture(tex);
                obj = tex;
            }
            else if (fileDownload.DownloadData.IsText ||
                fileDownload.FilePath.IndexOf(".txt") > 0)
            {
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
            var list = _downloading[fileDownload.FilePath];
            _downloading.Remove(fileDownload.FilePath);
            foreach (var ad2 in list)
            {
                if (ad2.DownloadData == null || ad2.DownloadData.Handler == null || !TokenUtils.IsValid(token))
                {
                    continue;
                }

                ad2.DownloadData.Handler(gs, fileDownload.FilePath, obj, ad2.DownloadData.Data, token);
            }
        }
    }

    private string GetAssetNameFromPath(AssetBundle ab, string assetName)
    {
        if (ab == null || string.IsNullOrEmpty(assetName))
        {
            return assetName;
        }

        assetName = assetName.ToLower();
        if (assetName.IndexOf("/") >= 0)
        {
            assetName = assetName.Substring(assetName.LastIndexOf("/") + 1);
        }

        string fullAssetName = assetName;
        var assetNames = ab.GetAllAssetNames();
        for (int i = 0; i < assetNames.Length; i++)
        {
            if (assetNames[i].IndexOf(assetName) >= 0)
            {
                if (assetNames[i].LastIndexOf("/") == assetNames[i].LastIndexOf(assetName) - 1)
                {
                    fullAssetName = assetNames[i];
                    break;
                }
            }
        }
        return fullAssetName;
    }

    /// <summary>
    /// Download something from an asset bundle (Async)
    /// </summary>
    /// <param name="gs"></param>
    /// <param name="assetPathSuffix">This is the category where the asset resides. It exists here so that
    /// the URLs being stored by the game aren't as long and so that we can move the categories on disk
    /// in a single spot (enforced here rather than making sure all data items use it.) It's a tradeoff
    /// and I've gone back and forth, but if it isn't here, then it would have to be in each
    /// piece of data stored in game data, OR each time the asset is loaded, we would have to do
    /// a lookup to get the path from the category OR it would have to be hardcoded. So this seems
    /// like the best way to do it to avoid mistakes later on even though it costs a bit extra in
    /// terms of programming time to put the category in the load. </param>
    /// <param name="assetName"></param>
    /// <param name="handler"></param>
    /// <param name="data"></param>
    /// <param name="assetPathSuffix">optional category used for certain specific naming conventions for bundles</param>
	public void LoadAsset(UnityGameState gs, string assetPathSuffix, string assetName,
        OnDownloadHandler handler,
        System.Object data, object parentIn,
        CancellationToken token, string subdirectory = null)
    {
        if (!TokenUtils.IsValid(token))
        {
            return;
        }
        var parent = parentIn as GEntity;
        if (string.IsNullOrEmpty(assetName))
        {
            if (handler != null)
            {
                handler(gs, assetName, null, data, token);
            }
            return;
        }

        if (_spriteCache.ContainsKey(assetName))
        {
            if (handler != null)
            {
                handler(gs, assetName, _spriteCache[assetName], data, token);
            }
            return;
        }

        if (!string.IsNullOrEmpty(subdirectory))
        {
            assetPathSuffix += "/" + subdirectory;
        }

        if ((false || ShouldCheckResources(assetPathSuffix)) && !_failedResourceLoads.Contains(assetName))
        {
            string categoryPath = AssetUtils. GetAssetPath(assetPathSuffix);
            if (assetName.IndexOf(categoryPath) == 0)
            {
                categoryPath = "";
            }

            var fullPath = categoryPath + assetName;

            UnityEngine.Object robj = null;

            if (_loadedResources.ContainsKey(fullPath))
            {
                robj = _loadedResources[fullPath];
            }
            else
            {
                robj = AssetUtils.LoadResource<GEntity>(fullPath);
                if (robj != null)
                {
                    _loadedResources[fullPath] = robj;
                }
            }
            if (robj == null)
            {
                _failedResourceLoads.Add(assetName);
            }
            else
            {

                var rtex = robj as Texture2D;
                if (rtex != null)
                {
                    var sprite = Sprite.Create(rtex, new Rect(0, 0, rtex.width, rtex.height), Vector2.zero);
                    _spriteCache[assetName] = sprite;
                    if (handler != null)
                    {
                        handler(gs, assetName, sprite, data, token);

                    }
                    return;
                }

                var rgo = GEntityUtils.InstantiateIntoParent(robj, parent);
                if (rgo != null)
                {
                    robj = rgo;
                }
                if (handler != null)
                {
                    if (robj is GEntity go)
                    {
                        GEntityUtils.InitializeHierarchy(gs, go);
                    }
                    handler(gs, assetName, robj, data, token);
                }
                return;
            }
        }

#if UNITY_EDITOR
        if (!String.IsNullOrEmpty(assetPathSuffix) && !_failedAssetPathLoads.Contains(assetName))
        {
            bool tryLocalLoad = !InitClient.EditorInstance.ForceDownloadFromAssetBundles;
            if (tryLocalLoad)
            {
                string categoryPath = AssetUtils.GetAssetPath(assetPathSuffix);
                if (!string.IsNullOrEmpty(categoryPath))
                {
                    UnityEngine.Object asset = null;
                    string fullPath = "Assets/" + AssetConstants.DownloadAssetRootPath + categoryPath + assetName;

                    if (fullPath.IndexOf(AssetConstants.ArtFileSuffix) < 0)
                    {
                        asset = AssetDatabase.LoadAssetAtPath<GEntity>(fullPath + AssetConstants.ArtFileSuffix);
                    }
                    else
                    {
                        asset = AssetDatabase.LoadAssetAtPath<GEntity>(fullPath);
                    }
                    if (asset != null)
                    {
                        asset = GEntityUtils.InstantiateIntoParent(asset, parent);

                        GEntityUtils.InitializeHierarchy(gs, asset as GEntity);

                        if (handler != null)
                        {
                            handler(gs, assetName, asset, data, token);
                        }
                        return;
                    }
                    else
                    {
                        _failedAssetPathLoads.Add(assetName);
                    }
                }
            }
        }
#endif

        string bundleName = GetBundleNameForCategoryAndAsset(gs, assetPathSuffix, assetName);

        if (_bundleFailedDownloads.Contains(bundleName))
        {
            if (handler != null)
            {
                handler(gs, assetName, null, data, token);
            }
            return;
        }
        if (assetName.LastIndexOf("/") >= 0)
        {            
            assetName = assetName.Substring(assetName.LastIndexOf("/") + 1);
        }

        BundleDownload currentBundleDownload = null;
        currentBundleDownload = new BundleDownload();
        currentBundleDownload.bundleName = bundleName;
        currentBundleDownload.assetName = assetName;
        currentBundleDownload.handler = handler;
        currentBundleDownload.data = data;
        currentBundleDownload.parent = parent;

        if (_bundleCache.ContainsKey(bundleName))
        {
            _bundleCache[bundleName].lastUsed = DateTime.UtcNow;
            _existingDownloads.Enqueue(currentBundleDownload);
            return;
        }

        currentBundleDownload.url = GetFullBundleURL(bundleName);

        if (_bundleDownloadQueue.ContainsKey(bundleName))
        {
            _bundleDownloadQueue[bundleName].Add(currentBundleDownload);
        }
        // If we are already downloading, add this to the queue and bail out.
        else if (_bundleDownloading.ContainsKey(bundleName))
        {
            _bundleDownloading[bundleName].Add(currentBundleDownload);
            return;
        }
        else
        {
            // New download, Add this bundle to the download, and add this asset to that bundle's download list.
            // When the bundle is finally downloaded, this will contain lots of bundle downloads waiting to be added.
            _bundleDownloadQueue[bundleName] = new List<BundleDownload>();
            _bundleDownloadQueue[bundleName].Add(currentBundleDownload);
        }

    }

    public static bool LoadLocallyInEditor = false;

    public void SetWorldAssetEnv(string worldAssetEnv)
    {
        _urlPrefixes.Remove(true);
        _worldDataEnv = worldAssetEnv;
        GetArtURLPrefix(true);
    }

    private Dictionary<bool, string> _urlPrefixes = new Dictionary<bool, string>();
    public string GetArtURLPrefix(bool worldData)
    {
#if UNITY_EDITOR
        if (LoadLocallyInEditor)
        {
            return "Assets/AssetBundles/";
        }
#endif

        if (_urlPrefixes.TryGetValue(worldData, out var prefix))
        {
            return prefix;
        }

        string newUrl = _assetURLPrefix + (worldData ? _worldDataEnv : _contentDataEnv) + "/";

        _urlPrefixes[worldData] = newUrl;

        return newUrl;
    }

    public bool IsInitialized(UnityGameState g)
    {
        return _isInitialized;
    }

    private async UniTask PermanentLoadFromExistingBundles(CancellationToken token)
    {
        await LoadFromExistingBundles(_gs, true, token);
    }

    int totalBundleLoaders = 0;
    private async UniTask LoadFromExistingBundles(UnityGameState gs, bool isPermanentLoader, CancellationToken token)
    {
        totalBundleLoaders++;
        await UniTask.NextFrame( cancellationToken: token);
        while (true)
        {
            if (_existingDownloads.Count > 0)
            {
                await LoadAssetFromExistingBundle(gs, _existingDownloads.Dequeue(), token);
                if (gs.rand.NextDouble() < _loadDelayChance)
                {
                    await UniTask.NextFrame( cancellationToken: token);
                }
            }
            else
            {
                if (!isPermanentLoader)
                {
                    totalBundleLoaders--;
                    return;
                }
                await UniTask.NextFrame( cancellationToken: token);
            }
        }
    }

    private async UniTask DownloadNewBundles(UnityGameState gs, CancellationToken token)
    {
        while (true)
        {
            if (_bundleDownloadQueue.Keys.Count > 0)
            {
                var firstKey = _bundleDownloadQueue.Keys.First();
                var firstDownload = _bundleDownloadQueue[firstKey];
                _bundleDownloadQueue.Remove(firstKey);
                if (firstDownload.Count < 1)
                {
                    await UniTask.NextFrame( cancellationToken: token);
                }
                else
                {
                    _bundleDownloading[firstKey] = firstDownload;
                    await DownloadOneBundle(gs, firstDownload[0], token);
                    _bundleDownloading.Remove(firstKey);
                    if (!_bundleCache.ContainsKey(firstDownload[0].bundleName))
                    {
                        foreach (var bdl in firstDownload)
                        {
                            bdl?.handler(gs, bdl.url, null, bdl.data, token);
                        }
                    }
                    else
                    {
                        foreach (var bdl in firstDownload)
                        {
                            _existingDownloads.Enqueue(bdl);
                        }
                    }
                }
            }
            else
            {
                await UniTask.NextFrame( cancellationToken: token);
            }
        }
    }

    private async UniTask LoadAssetFromExistingBundle(UnityGameState gs, BundleDownload bdl, CancellationToken token)
    {
        // Need to check existence of bundle here since this call is delayed from when 
        if (bdl == null || !_bundleCache.ContainsKey(bdl.bundleName))
        {
            return;
        }
        var bdata = _bundleCache[bdl.bundleName];
        bdata.LoadingCount++;
        bdata.lastUsed = DateTime.UtcNow;
        if (!bdata.LoadedAssets.ContainsKey(bdl.assetName))
        {

            var request = StartLoadAssetFromBundle(bdl.bundleName, bdl.assetName);
            if (request != null)
            {

                while (!request.isDone)
                {
                    await UniTask.NextFrame( cancellationToken: token);
                }
                bdata.LoadedAssets[bdl.assetName] = request.asset;
            }
        }

        if (bdata.LoadedAssets.ContainsKey(bdl.assetName))
        {
            var newObj = InstantiateBundledAsset(gs, bdata.LoadedAssets[bdl.assetName], bdl.parent, bdl.bundleName, bdl.assetName);
            bdata.LoadingCount--;
            if (bdl.handler != null)
            {
                bdl.handler(gs, bdl.assetName, newObj, bdl.data, token);
                return;
            }
        }
        else
        {
            bdata.LoadingCount--;
            return;
        }
    }

    private void AddBundleToCache(UnityGameState gs, BundleDownload bad, AssetBundle downloadedBundle)
    {
        if (bad == null || downloadedBundle == null || _bundleCache.ContainsKey(bad.bundleName)) return;
        var bdata = new BundleCacheData()
        {
            name = bad.bundleName,
            assetBundle = downloadedBundle,
            lastUsed = DateTime.UtcNow,
            KeepLoaded = (bad.bundleName.IndexOf("atlas") == 0
            || bad.bundleName.IndexOf("screen") == 0
            || bad.bundleName.IndexOf("ui") == 0),
        };
        _bundleCache[bad.bundleName] = bdata;
        _assetCounts.BundlesLoaded++;
    }

    private AssetBundleRequest StartLoadAssetFromBundle(string bundleName, string assetName)
    {
        if (string.IsNullOrEmpty(bundleName) || string.IsNullOrEmpty(assetName))
        {
            return null;
        }
        string fullname = (bundleName + "--" + assetName).ToLower();

        if (!_bundleCache.ContainsKey(bundleName))
        {
            return null;
        }

        BundleCacheData cacheData = _bundleCache[bundleName];
        var bundle = cacheData.assetBundle;
        var fullName = GetAssetNameFromPath(bundle, assetName);
        try
        {
            return bundle.LoadAssetAsync(assetName);
        }
        catch (Exception e)
        {
            _logger.Exception(e, "Failed asset Load:" + assetName);
        }
        return null;
    }
    public void LoadSpriteInto(UnityGameState gs, string atlasName, string spriteName, object parentSprite, CancellationToken token)
    {
        LoadSprite(gs, atlasName, spriteName, null, parentSprite, token);
    }


    public GEntity _assetParent = null;
    public void LoadSprite(UnityGameState gs, string atlasName, string spriteName, OnDownloadHandler handler, object data, CancellationToken token)
    {
        if (string.IsNullOrEmpty(atlasName))
        {
            if (handler != null)
            {
                handler(gs, spriteName, null, data, token);
            }
            return;
        }

        var sdata = new AtlasSpriteDownload()
        {
            atlasName = atlasName,
            spriteName = spriteName,
            finalHandler = handler,
            data = data,
        };

        if (_atlasCache.ContainsKey(atlasName))
        {
            GetAtlasSprite(gs, sdata, token);
            return;
        }

        if (_assetParent == null)
        {
            _assetParent = GEntityUtils.FindSingleton(AssetConstants.GlobalAssetParent, true);
        }

        LoadAssetInto(gs, _assetParent, AssetCategoryNames.Atlas, atlasName, OnDownloadAtlas, sdata, token);

    }

    private void OnDownloadAtlas(UnityGameState gs, string url, object obj, object data, CancellationToken token)
    {
        var sdata = data as AtlasSpriteDownload;
        var go = obj as GEntity;
        if (go == null)
        {
            if (sdata != null && sdata.finalHandler != null)
            {
                sdata.finalHandler(gs, url, null, sdata.data, token);
            }

            return;
        }

        if (sdata == null || string.IsNullOrEmpty(sdata.atlasName))
        {
            GEntityUtils.Destroy(go);

            if (sdata != null && sdata.finalHandler != null)
            {
                sdata.finalHandler(gs, url, null, sdata.data, token);
            }
            return;
        }

        var atlasCont = go.GetComponent<SpriteAtlasContainer>();
        if (atlasCont == null || atlasCont.Atlas == null)
        {
            if (sdata.finalHandler != null)
            {
                sdata.finalHandler(gs, url, null, sdata.data, token);
            }
        }

        if (_atlasCache.ContainsKey(sdata.atlasName))
        {
            GEntityUtils.Destroy(go);
        }
        else
        {
            _atlasCache[sdata.atlasName] = atlasCont.Atlas;
        }

        GetAtlasSprite(gs, sdata, token);

    }

    public string GetSpriteCacheKey(UnityGameState gs, string atlasName, string spriteName)
    {
        if (string.IsNullOrEmpty(atlasName) || string.IsNullOrEmpty(spriteName)) return "";
        return (spriteName + "." + atlasName).ToLower();
    }

    private void GetAtlasSprite(UnityGameState gs, AtlasSpriteDownload download, CancellationToken token)
    {
        if (download == null)
        {
            return;
        }

        if ((string.IsNullOrEmpty(download.atlasName) ||
            string.IsNullOrEmpty(download.spriteName) ||
            !_atlasCache.ContainsKey(download.atlasName)) &&
            download.finalHandler != null)
        {
            SpriteAtlas atlasTemp = null;
            if (_atlasCache.ContainsKey(download.atlasName))
            {
                atlasTemp = _atlasCache[download.atlasName];
            }
            download.finalHandler(gs, download.spriteName, atlasTemp, download.data, token);
            return;
        }
        var atlas = _atlasCache[download.atlasName];

        string cacheName = GetSpriteCacheKey(gs, download.atlasName, download.spriteName);

        Sprite sprite = null;
        if (_spriteCache.ContainsKey(cacheName))
        {
            sprite = _spriteCache[cacheName];
        }
        else
        {
            sprite = atlas.GetSprite(download.spriteName);
            if (sprite != null)
            {
                sprite.name = sprite.name.Replace("(Clone)", "");
                _spriteCache[cacheName] = sprite;
            }
            else
            {
                _logger.Debug("Missing sprite: " + download.spriteName);
            }
        }

        var image = download.data as GImage;
        if (image != null)
        {
            image.sprite = sprite;
        }


        if (download.finalHandler != null)
        {
            download.finalHandler(gs, download.spriteName, atlas, download.data, token);
        }
    }


    private void OnDownloadSpriteForList(UnityGameState gs, string url, object obj, object data, CancellationToken token)
    {
        var spriteDelegate = data as SpriteListDelegate;
        if (spriteDelegate == null)
        {
            return;
        }
        var atlas = obj as SpriteAtlas;

        if (obj == null)
        {
            spriteDelegate(gs, new Sprite[0]);
            return;
        }

        var spriteCount = atlas.spriteCount;
        var retval = new Sprite[atlas.spriteCount];
        atlas.GetSprites(retval);
        spriteDelegate(gs, retval);
    }















    protected string GetBundleHash(string bundleName)
    {
        if (string.IsNullOrEmpty(bundleName)) return "";
        if (!_bundleVersions.Versions.ContainsKey(bundleName))
        {
            return "";
        }
        return _bundleVersions.Versions[bundleName].Hash;
    }

    protected uint[] GetBundleHashInts(string bundleName)
    {
        if (string.IsNullOrEmpty(bundleName)) return null;
        if (!_bundleVersions.Versions.ContainsKey(bundleName))
        {
            return null;
        }
        return _bundleVersions.Versions[bundleName].GetHashInts();
    }

    protected Hash128 GetBundleHash128(string bundleName)
    {
        var hashInts = GetBundleHashInts(bundleName);
        if (hashInts == null || hashInts.Length != 4) return new Hash128();
        return new Hash128(hashInts[0], hashInts[1], hashInts[2], hashInts[3]);
    }


    protected void LoadLastSaveTimeFile(UnityGameState gs, CancellationToken token)
    {
        var path = AssetUtils.GetRuntimePrefix() + AssetConstants.BundleUpdateFile;
        var ddata = new DownloadData() { ForceDownload = true, Handler = OnDownloadLastSaveTimeText, IsText = true };
        DownloadFile(gs, path, ddata, false, token);
    }

    private void OnDownloadLastSaveTimeText(UnityGameState gs, string url, object obj, object data, CancellationToken token)
    {
        _bundleUpdateInfo = SerializationUtils.TryDeserialize<BundleUpdateInfo>(obj);

        LoadAssetBundleList(gs, token);
    }

    void LoadAssetBundleList(UnityGameState gs, CancellationToken token)
    {
        _bundleVersions = _localRepo.LoadObject<BundleVersions>(AssetConstants.BundleVersionsFile);

        var ddata = new DownloadData() { ForceDownload = true, Handler = OnDownloadBundleVersions, IsText = true };

        if (_bundleVersions == null || _bundleVersions.UpdateInfo == null ||
            _bundleUpdateInfo == null ||
            _bundleVersions.UpdateInfo.ClientVersion != _bundleUpdateInfo.ClientVersion ||
            _bundleVersions.UpdateInfo.UpdateTime != _bundleUpdateInfo.UpdateTime)
        {
            var path = AssetUtils.GetRuntimePrefix() + AssetConstants.BundleVersionsFile;
            DownloadFile(gs, path, ddata, false, token);
            gs.logger.Info("YES DOWNLOAD BUNDLE VERSIONS!");
        }
        else
        {
            _isInitialized = true;
            gs.logger.Info("NO DOWNLOAD BUNDLE VERSIONS");
        }
    }

    private void OnDownloadBundleVersions(UnityGameState gs, string url, object obj, object data, CancellationToken token)
    {
        BundleVersions newVersions = SerializationUtils.TryDeserialize<BundleVersions>(obj);

        _isInitialized = true;

        if (newVersions != null && newVersions.UpdateInfo != null &&
            newVersions.Versions != null && newVersions.Versions.Keys.Count > 0)
        {
            _bundleVersions = newVersions;
            _localRepo.SaveObject(AssetConstants.BundleVersionsFile, _bundleVersions);
        }
    }

    protected string GetFullBundleURL(string bundleName)
    {
        return GetArtURLPrefix(false) + AssetUtils.GetRuntimePrefix() + bundleName + "_" + GetBundleHash(bundleName);
    }

    private async UniTask DownloadOneBundle(UnityGameState gs, BundleDownload bad, CancellationToken token)
    {
        if (string.IsNullOrEmpty(bad.url))
        {
            return;
        }

        for (int i = 0; i < _retryTimes; i++)
        {
            string bundleHash = GetBundleHash(bad.bundleName);
            if (string.IsNullOrEmpty(bundleHash))
            {
                _logger.Debug("No bundle hash for: " + bad.url);
                return;
            }

            using (UnityWebRequest request = UnityWebRequestAssetBundle.GetAssetBundle(bad.url,
                GetBundleHash128(bad.bundleName)))
            {
                UnityWebRequestAsyncOperation asyncOp = request.SendWebRequest();
                while (!asyncOp.isDone)
                {
                    await UniTask.NextFrame( cancellationToken: token);
                }

                AssetBundle downloadedBundle = null;

                if (request.result != UnityWebRequest.Result.ProtocolError)
                {
                    try
                    {
                        downloadedBundle = DownloadHandlerAssetBundle.GetContent(request);
                    }
                    catch (Exception e)
                    {
                        _logger.Exception(e, "FailedbundleDownload: " + bad.url + " " + bad.assetName);
                    }
                }

                if (downloadedBundle != null)
                {
                    AddBundleToCache(gs, bad, downloadedBundle);

                    request.Dispose();
                    return;
                }
                else
                {
                    request.Dispose();
                    await UniTask.Delay(TimeSpan.FromSeconds(1.1f), cancellationToken: token);
                }
            }
        }
        if (!_bundleFailedDownloads.Contains(bad.bundleName))
        {
            _bundleFailedDownloads.Add(bad.bundleName);
        }
    }


    protected GEntity InstantiateBundledAsset(UnityGameState gs, object child, GEntity parent, string bundleName, string assetName)
    {
        var go = GEntityUtils.InstantiateIntoParent(child, parent);
        if (go != null)
        {
            _bundleCache[bundleName].Instances.Add(go);
        }
        else
        {
            _logger.Error("Failed to load " + assetName + " from " + bundleName);
            return null;
        }
        BaseBehaviour oneBehavior = go.GetComponent<BaseBehaviour>();
        if (oneBehavior != null)
        {
            GEntityUtils.InitializeHierarchy(gs, go);
        }
        return go;
    }

    public void GetSpriteList(UnityGameState gs, string atlasName, SpriteListDelegate onLoad, CancellationToken token)
    {
        LoadSprite(gs, atlasName, "", OnDownloadSpriteForList, onLoad, token);
    }

    /// <summary>
    /// Get the bundle name for an asset, leave an override so later on I can have different
    /// categories of asset bundles or different numbers of asset bundles for different
    /// games.
    /// </summary>
    /// <param name="gs"></param>
    /// <param name="assetPath"></param>
    /// <param name="pathPrefix"></param>
    /// <returns></returns>
    /// 
    private Dictionary<string, Dictionary<string, string>> _existingBundleNames = new Dictionary<string, Dictionary<string, string>>();
    public virtual string GetBundleNameForCategoryAndAsset(UnityGameState gs, string pathPrefix, string assetPath)
    {
        if (_existingBundleNames.TryGetValue(pathPrefix, out Dictionary<string,string> assetDictionary))
        {
            if (assetDictionary.TryGetValue(assetPath, out string path))
            {
                return path;
            }
        }
        else
        {
            assetDictionary = new Dictionary<string, string>();
            _existingBundleNames[pathPrefix] = assetDictionary;
        }

        string fullName = pathPrefix + "/" + assetPath;


        int firstSlashIndex = fullName.IndexOf('/');
        int lastSlashIndex = fullName.LastIndexOf('/');

        // Two slashes, so the fullName becomes everything before the last slash
        if (firstSlashIndex > 0 && lastSlashIndex > firstSlashIndex && lastSlashIndex < fullName.Length-1)
        {
            fullName = fullName.Substring(0, lastSlashIndex);
        }


        string lettername = new String(fullName.Where(x => char.IsLetter(x)).ToArray()).ToLowerInvariant();
        assetDictionary[assetPath] = lettername;

        return lettername;
    }

    public void LoadAssetInto(UnityGameState gs, object parent, string assetPathSuffix, string assetPath, OnDownloadHandler handler, object data, CancellationToken token, string subdirectory = null)
    {
        LoadAsset(gs, assetPathSuffix, assetPath, handler, data, parent, token, subdirectory);
    }

    private bool ShouldCheckResources(string assetSuffix)
    {
        if (assetSuffix == AssetCategoryNames.Prefabs)
        {
            return true;
        }

        return false;
    }

    public string StripPathPrefix(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return "";
        }
        path = path.Substring(path.LastIndexOf("/") + 1);
        path = path.Substring(path.LastIndexOf("\\") + 1);
        return path;
    }
}

