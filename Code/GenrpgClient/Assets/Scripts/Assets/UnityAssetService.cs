using GEntity = UnityEngine.GameObject;
using UnityEngine.U2D;
using UnityEngine.Networking;
using System.Collections.Generic;
using System;
using System.Text;
using System.Linq;	
using System.Threading.Tasks;
using Scripts.Assets.Assets.Constants;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Interfaces;
using System.Threading;
using Genrpg.Shared.Logs.Entities;
using UnityEngine;
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
    public bool unloadImmediately = false;
}

public class BundleCacheData
{
    public string name;
    public AssetBundle assetBundle;
    public DateTime lastUsed = DateTime.UtcNow;
    public int LoadingCount;
    public bool NeverUnload;
    public bool RequireIncrementalUnload;
    public Dictionary<string, object> LoadedAssets = new Dictionary<string, object>();
    public List<object> Instances = new List<object>();
}


public class UnityAssetService : IAssetService
{
    private ILogSystem _logger;
    private IUnityUpdateService _updateService;

    public static long BundlesLoaded = 0;
    public static long BundlesUnloaded = 0;
    public static long ObjectsLoaded = 0;
    public static long ObjectsUnloaded = 0;

    private static float LoadDelayChance = 0.03f;

    public static LoadSpeed LoadSpeed = LoadSpeed.Normal;


    private UnityGameState _gs;

    public const int MaxConcurrentDownloads = 2;

    public const int LoadsRequiredForBurstLoading = 70;
    public const int BurstConcurrentExistingLoads = 400;

    public const int MaxConcurrentExistingLoads = 5;

    public const int MaxConcurrentBundleDownloads = 3;

    public const int RetryTimes = 3;

    public const string ArtFileSuffix = ".prefab";

    public const string BundleVersionFilename = "bundleVersions.txt";

    protected bool _isInitialized = false;

    protected Dictionary<string, List<FileDownload>> _downloading = new Dictionary<string,List<FileDownload>>();
	
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
    protected Dictionary<string, BundleVersionData> _bundleVersions = new Dictionary<string, BundleVersionData>();


    public const string GlobalAssetParent = "GlobalAssetParent";

    private static string _persistentDataPath = null;

    int _fileDownloadingCount = 0;
    private CancellationToken _token;
    private Queue<BundleDownload> _existingDownloads = new Queue<BundleDownload>();

    public static string GetPerisistentDataPath()
    {
        if (string.IsNullOrEmpty(_persistentDataPath)) _persistentDataPath = AppUtils.PersistentDataPath;
        return _persistentDataPath;
    }

    public static string GetPlatformString()
    {
        string prefix = PlatformAssetPrefixes.Win;
#if UNITY_STANDALONE_OSX
        prefix = PlatformAssetPrefixes.OSX;
#endif
#if UNITY_STANDALONE_LINUX
        prefix = PlatformAssetPrefixes.Linux;
#endif
#if UNITY_EDITOR
        prefix = PlatformAssetPrefixes.Win;
#endif

        return prefix;
    }

    private static string _runtimePrefix = "";
    public static string GetRuntimePrefix()
	{
        if (!string.IsNullOrEmpty(_runtimePrefix))
        {
            return _runtimePrefix;
        }

        var prefix = GetPlatformString();
        _runtimePrefix = AppUtils.Version + "/" + prefix + "/";
        return _runtimePrefix;
	}
    /// <summary>
    /// Downloads the asset bundle list.
    /// </summary>
    /// <param name="gs"></param>
	public void Init(UnityGameState gs, CancellationToken token)
	{
        if (!AppUtils.IsPlaying)
        {
            return;
        }

        _gs = gs;
        _logger = gs.logger;
        _token = token;
        SpriteAtlasManager.atlasRequested += DummyRequestAtlas;
        SpriteAtlasManager.atlasRegistered += DummuRegisterAtlas;
        GetPerisistentDataPath();
        LoadAssetBundleList(gs, token);

        for (int i = 0; i < MaxConcurrentExistingLoads; i++)
        {
            TaskUtils.AddTask(LoadFromExistingBundles(gs, true, token));
        }
        for (int i = 0; i < MaxConcurrentBundleDownloads; i++)
        {
            TaskUtils.AddTask(DownloadNewBundles(gs, token));
        }

        TaskUtils.AddTask(IncrementalClearMemoryCache(gs, token));
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

	public void ClearMemoryCache (UnityGameState gs, CancellationToken token)
	{
        var newBundleCache = new Dictionary<string, BundleCacheData>();
		foreach (var item in _bundleCache.Keys)
        {
            var bdata = _bundleCache[item];

            if (bdata.NeverUnload || bdata.RequireIncrementalUnload)
            {
                newBundleCache[item] = bdata;
            }
            else
            {
                if (bdata.assetBundle != null)
                {
                    bdata.assetBundle.Unload(true);
                    BundlesUnloaded++;
                    GEntityUtils.Destroy(bdata.assetBundle);
                }
            }
		}

        _bundleCache = newBundleCache;
        TaskUtils.AddTask(AssetUtils.UnloadUnusedAssets(token));
	}


    protected async Task IncrementalClearMemoryCache(UnityGameState gs, CancellationToken token)
    {
        while (true)
        {
            await Task.Delay(5000, token);

            int removeCount = 0;
            List<string> bundleCacheKeys = _bundleCache.Keys.ToList();
            foreach (string item in bundleCacheKeys)
            {
                var bundle = _bundleCache[item];
                if (!bundle.NeverUnload &&
                    bundle.LoadingCount < 1 &&
                    bundle.assetBundle != null &&
                    bundle.lastUsed < DateTime.UtcNow.AddSeconds(-20))
                {
                    if (bundle.Instances.Any(x=> x.Equals(null)))
                    {
                        bundle.Instances = bundle.Instances.Where(x => !x.Equals(null)).ToList();
                    }
                    if (bundle.Instances.Count > 0)
                    {
                        continue;
                    }

                    bundle.assetBundle.Unload(true);
                    BundlesUnloaded++;
                    GEntityUtils.Destroy(bundle.assetBundle);
                    _bundleCache.Remove(item);
                    removeCount++;
                    if (removeCount > 5)
                    {
                        break;
                    }
                    await Task.Delay(1, token);
                }
            }
            if (removeCount > 0)
            {
                await AsyncUnloadAssets(token);
            }
        }
    }

    private async Task AsyncUnloadAssets(CancellationToken token)
    {
        await AssetUtils.UnloadUnusedAssets(token);
    }


    // If it's in the cache, return it.
    // If it's a failed download, return nothing.
    // If it's in downloading, add the handler to the queue.	
    // If it's none of those, start the download.


	public void DownloadFile(UnityGameState gs, string filePath, DownloadData downloadData, CancellationToken token)
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
				downloadData.Handler (gs, filePath, null,downloadData.Data, token);
			}
			return;
		}

        string urlPrefix = downloadData.URLPrefixOverride;

        if (string.IsNullOrEmpty(urlPrefix))
        {
            urlPrefix = GetArtURLPrefix(gs);
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
            TaskUtils.AddTask( StartDownloadFile(gs, fileDownLoad, token));
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
		return GetPerisistentDataPath() + "/" + url;
	}

    private async Task StartDownloadFile(UnityGameState gs, FileDownload fileDownload, CancellationToken token)
    {

        if (fileDownload == null)
        {
            return;
        }
		Texture2D tex = null;
        string txt = null;

        System.Object obj = null;

        var stringrepo = new LocalFileRepository(gs.logger);
        if (!fileDownload.DownloadData.ForceDownload)
        {
            fileDownload.DownloadData.StartBytes = stringrepo.LoadBytes(fileDownload.FilePath);
        }
        if (fileDownload.DownloadData.StartBytes == null || fileDownload.DownloadData.StartBytes.Length < 1)
        {
            for (int i = 0; i < RetryTimes; i++)
            {
                while (_fileDownloadingCount >= MaxConcurrentDownloads && LoadSpeed != LoadSpeed.Fast)
                {
                    await Task.Delay(1, token);
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
                        UnityWebRequestAsyncOperation  asyncOp = request.SendWebRequest();

                        while (!asyncOp.isDone)
                        {
                            await Task.Delay(1, token);
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
                            if (i < RetryTimes - 1)
                            {
                                _downloading.Remove(fileDownload.FilePath);
                                _fileDownloadingCount--;
                                await Task.Delay(TimeSpan.FromSeconds(1.0f), cancellationToken: token);
                            }
                        }
                    }

                    if (fileDownload.DownloadData.StartBytes != null)
                    {
                        byte[] finalBytes = fileDownload.DownloadData.StartBytes;
                       
                        if (UnityAssetService.LoadSpeed != LoadSpeed.Fast)
                        {
                            await Task.Delay(1, token);
                        }
                        stringrepo.SaveBytes(fileDownload.FilePath, finalBytes);
                        if (UnityAssetService.LoadSpeed != LoadSpeed.Fast)
                        {
                            await Task.Delay(1, token);
                        }
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
                if (UnityAssetService.LoadSpeed != LoadSpeed.Fast)
                {
                    await Task.Delay(1, token);
                }
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
    /// <param name="assetCategory">This is the category where the asset resides. It exists here so that
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
    /// <param name="assetCategory">optional category used for certain specific naming conventions for bundles</param>
	public void LoadAsset (UnityGameState gs, string assetCategory, string assetPath, 
        OnDownloadHandler handler, 
        System.Object data, object parentIn,
        CancellationToken token)
	{
        if (!TokenUtils.IsValid(token))
        {
            return;
        }
        var parent = parentIn as GEntity;
        if (string.IsNullOrEmpty(assetPath))
        {
            if (handler != null)
            {
                handler(gs, assetPath, null, data, token);
            }
            return;
        }

        if (_spriteCache.ContainsKey(assetPath))
        {
            if (handler != null)
            {
                handler(gs, assetPath, _spriteCache[assetPath], data, token);
            }
            return;
        }

        if ((false || ShouldCheckResources(assetCategory)) && !_failedResourceLoads.Contains(assetPath))
        {
            string categoryPath = GetAssetPath(assetCategory);
            if (assetPath.IndexOf(categoryPath) == 0)
            {
                categoryPath = "";
            }

            var fullPath = categoryPath + assetPath;

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
                _failedResourceLoads.Add(assetPath);
            }
            else
            {

                var rtex = robj as Texture2D;
                if (rtex != null)
                {
                    var sprite = Sprite.Create(rtex, new Rect(0, 0, rtex.width, rtex.height), Vector2.zero);
                    _spriteCache[assetPath] = sprite;
                    if (handler != null)
                    {
                        handler(gs, assetPath, sprite, data, token);
                     
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
                    handler(gs, assetPath, robj, data, token);
                }
                return;
            }
        }

#if UNITY_EDITOR
        if (!String.IsNullOrEmpty(assetCategory) && !_failedAssetPathLoads.Contains(assetPath))
        {
            bool tryLocalLoad = !InitClient.EditorInstance.ForceDownloadFromAssetBundles;
            if (tryLocalLoad)
            {
                string categoryPath = GetAssetPath(assetCategory);
                if (!string.IsNullOrEmpty(categoryPath))
                {
                    UnityEngine.Object asset = null;
                    string fullPath = "Assets/" + AssetCategory.DownloadAssetRootPath + categoryPath + assetPath;

                    if (fullPath.IndexOf(ArtFileSuffix) < 0)
                    {
                        asset = AssetDatabase.LoadAssetAtPath<GEntity>(fullPath + ArtFileSuffix);
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
                            handler(gs, assetPath, asset, data, token);
                        }
                        return;
                    }
                    else
                    {
                        _failedAssetPathLoads.Add(assetPath);
                    }
                }
            }
        }
#endif

        string bundleName = GetBundleForCategoryAndAsset(gs, assetCategory, assetPath);

        if (_bundleFailedDownloads.Contains(bundleName))
        {
            if (handler != null)
            {
                handler(gs, assetPath, null, data, token);
            }
            return;
        }
        if (assetPath.LastIndexOf("/") >= 0)
        {
            assetPath = assetPath.Substring(assetPath.LastIndexOf("/") + 1);
        }

        BundleDownload currentBundleDownload = null;
        currentBundleDownload = new BundleDownload();
        currentBundleDownload.bundleName = bundleName;
        currentBundleDownload.assetName = assetPath;
        currentBundleDownload.handler = handler;
        currentBundleDownload.data = data;
        currentBundleDownload.parent = parent;

        if (_bundleCache.ContainsKey(bundleName))
        {
            _bundleCache[bundleName].lastUsed = DateTime.UtcNow;
            _existingDownloads.Enqueue(currentBundleDownload);
            return;
        }

        currentBundleDownload.url = GetFullBundleURL(gs, bundleName);

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
    private string _artURLPrefix = null;
	public string GetArtURLPrefix(UnityGameState gs)
	{
#if UNITY_EDITOR
		if (LoadLocallyInEditor)
		{
			return "Assets/AssetBundles/";
		}
#endif
        if (!string.IsNullOrEmpty(_artURLPrefix)) return _artURLPrefix;
        _artURLPrefix = GetArtURLPrefixInner(gs);
        return _artURLPrefix;
	}

    public bool IsInitialized(UnityGameState g)
    {
        return _isInitialized;
    }

    private async Task PermanentLoadFromExistingBundles(CancellationToken token)
    {
        await LoadFromExistingBundles(_gs, true, token);
    }

    int totalBundleLoaders = 0;
    private async Task LoadFromExistingBundles(UnityGameState gs, bool isPermanentLoader, CancellationToken token)
    {
        totalBundleLoaders++;
        await Task.Delay(1, token);
        while (true)
        {
            if (_existingDownloads.Count > 0)
            {
                await LoadAssetFromExistingBundle(gs, _existingDownloads.Dequeue(), token);
                if (gs.rand.NextDouble() < LoadDelayChance)
                {
                    await Task.Delay(1, token);
                }
            }
            else
            {
                if (!isPermanentLoader && LoadSpeed != LoadSpeed.Fast)
                {
                    totalBundleLoaders--;
                    return;
                }
                await Task.Delay(1, token);
            }
        }
    }

    private async Task DownloadNewBundles (UnityGameState gs, CancellationToken token)
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
                    await Task.Delay(1, token);
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
                await Task.Delay(1, token);
            }
        }
	}

    private async Task LoadAssetFromExistingBundle (UnityGameState gs, BundleDownload bdl, CancellationToken token)
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
                    await Task.Delay(1, _token);
                }
                bdata.LoadedAssets[bdl.assetName] = request.asset;
            }
        }

        if (bdata.LoadedAssets.ContainsKey(bdl.assetName))
        {
            var newObj = InstantiateBundledAsset(gs, bdata.LoadedAssets[bdl.assetName], bdl.parent, bdl.bundleName, bdl.assetName) ;
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

    private void AddBundleToCache (UnityGameState gs, BundleDownload bad, AssetBundle downloadedBundle)
    {
        if (bad == null || downloadedBundle == null || _bundleCache.ContainsKey(bad.bundleName)) return;
        var bdata = new BundleCacheData()
        {
            name = bad.bundleName,
            assetBundle = downloadedBundle,
            lastUsed = DateTime.UtcNow,
            NeverUnload = IsNeverUnloadBundle(gs, bad.bundleName),
            RequireIncrementalUnload=IncrementalUnloadOnly(gs, bad.bundleName),
        };
        _bundleCache[bad.bundleName] = bdata;
        BundlesLoaded++;
    }

	private AssetBundleRequest StartLoadAssetFromBundle (string bundleName, string assetName)
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
    public void LoadSpriteInto (UnityGameState gs, string atlasName, string spriteName, object parentSprite, CancellationToken token)
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
            _assetParent = GEntityUtils.FindSingleton(GlobalAssetParent, true);
        }

        LoadAssetInto(gs, _assetParent, AssetCategory.Atlas, atlasName, OnDownloadAtlas, sdata, token);

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

        if(obj == null)
        {
            spriteDelegate(gs, new Sprite[0]);
            return;
        }

        var spriteCount = atlas.spriteCount;
        var retval = new Sprite[atlas.spriteCount];
        atlas.GetSprites(retval);
        spriteDelegate(gs, retval);
    }















    protected string GetBundleHash(UnityGameState gs, string bundleName)
    {
        if (string.IsNullOrEmpty(bundleName)) return "";
        if (!_bundleVersions.ContainsKey(bundleName))
        {
            return "";
        }
        return _bundleVersions[bundleName].Hash;
    }

    protected uint[] GetBundleHashInts(UnityGameState gs, string bundleName)
    {
        if (string.IsNullOrEmpty(bundleName)) return null;
        if (!_bundleVersions.ContainsKey(bundleName))
        {
            return null;
        }
        return _bundleVersions[bundleName].GetHashInts();
    }

    protected Hash128 GetBundleHash128(UnityGameState gs, string bundleName)
    {
        var hashInts = GetBundleHashInts(gs, bundleName);
        if (hashInts == null || hashInts.Length != 4) return new Hash128();
        return new Hash128(hashInts[0], hashInts[1], hashInts[2],hashInts[3]);
    }

    protected string GetOnlineBundleVersionPath(UnityGameState gs)
    {
        return GetRuntimePrefix() + BundleVersionFilename;
    }

    protected string GetLocalBundleVersionPath(UnityGameState gs)
    {
        return BundleVersionFilename;
    }


    protected void LoadAssetBundleList(UnityGameState gs, CancellationToken token)
    {
        var stringRepo = new LocalFileRepository(gs.logger);

        string filename = GetLocalBundleVersionPath(gs);
        var txt = stringRepo.Load(filename);
        var newBundleVersions = ConvertTextToBundleVersions(gs, txt);
        if (newBundleVersions != null && newBundleVersions.Keys.Count > 0)
        {
            _bundleVersions = newBundleVersions;
        }

        var path = GetOnlineBundleVersionPath(gs);
        var ddata = new DownloadData() { ForceDownload = true, Handler= OnDownloadBundleVersions, IsText = true };
        DownloadFile(gs, path, ddata, token);
    }

    private void OnDownloadBundleVersions (UnityGameState gs, string url, object obj, object data, CancellationToken token)
    {

        _isInitialized = true;
        var txt = obj as String;
        if (txt == null)
        {
            return;
        }

        var oldVersions = _bundleVersions;

        var newBundleVersions = ConvertTextToBundleVersions(gs, txt);
        if (newBundleVersions != null && newBundleVersions.Keys.Count > 0)
        {
            _bundleVersions = newBundleVersions;
        }

        SaveBundleVersionsText(gs);


        TaskUtils.AddTask(DownloadInitialBundles(gs, token));
    }

    private async Task DownloadInitialBundles(UnityGameState gs, CancellationToken token)
    {
        await CacheBundleList(gs, GetInitialDownloadBundleList(gs), OnCacheInitialBundles, token);
    }

    private async Task CacheAllBundles(UnityGameState gs, CancellationToken token)
    {
        if (_bundleVersions != null)
        {
            var keys = _bundleVersions.Keys.ToList();
            await CacheBundleList(gs, keys, null, token);
        }
    }

    private async Task CacheBundleList(UnityGameState gs, List<String> bundles, GameStateObjectDelegate onDownloadComplete, CancellationToken token)
    {
        if (bundles == null)
        {
            return;
        }

        for (int b = 0; b < bundles.Count; b++)
        {
            var bundleName = bundles[b];
            string fullUrl = GetFullBundleURL(gs, bundleName);
            var bad = new BundleDownload();
            bad.url = fullUrl;
            bad.bundleName = bundleName;
            bad.assetName = "";
            bad.handler = OnCacheOneBundle;
            bad.unloadImmediately = true;

            await DownloadOneBundle(gs, bad, token);
            if (bundles.Count > 10)
            {
                _logger.Error("Cached bundle " + (b + 1) + " of " + bundles.Count);
            }
        }

        if (onDownloadComplete != null)
        {
            onDownloadComplete(gs, bundles);
        }
    }

    protected void OnCacheInitialBundles(GameState gs, object obj)
    {
        if (_assetParent == null) _assetParent = GEntityUtils.FindSingleton(GlobalAssetParent, true);     
   }

    protected string GetFullBundleURL(UnityGameState gs, string bundleName)
    {
        return GetArtURLPrefix(gs) + GetRuntimePrefix() + bundleName + "_" + GetBundleHash(gs, bundleName);
    }

    private void OnCacheOneBundle(UnityGameState gs, string url, object obj, object data, CancellationToken token)
    {
        var bad = data as BundleDownload;

        if (bad == null ||string.IsNullOrEmpty(bad.bundleName))
        {
            return;
        }
    }

    public static Dictionary<string,BundleVersionData> ConvertTextToBundleVersions(UnityGameState gs, string txt)
    {

        var dict = new Dictionary<String, BundleVersionData>();
        if (string.IsNullOrEmpty(txt))
        {
            return dict;
        }

        var lines = txt.Split('\n');
        int wordsPerLine = 3;
        for (int l = 0; l < lines.Length; l++)
        {
            var words = lines[l].Split(' ');
            if (words.Length < wordsPerLine)
            {
                continue;
            }
            bool blankWord = false;
            for (int i = 0; i < 3; i++)
            {
                if (string.IsNullOrEmpty(words[i]))
                {
                    blankWord = true;
                    break;
                }
            }
            if (blankWord)
            {
                continue;
            }
            var bvd = new BundleVersionData();
            bvd.Name = words[0];
            bvd.Hash = words[1];
            int sizeVal = 0;
            Int32.TryParse(words[2], out sizeVal);
            bvd.Size = sizeVal;
            if (dict.ContainsKey(bvd.Name))
            {
                dict.Remove(bvd.Name);
            }
            dict[bvd.Name] = bvd;
        }
        return dict;
    }

    public static string ConvertBundleVersionsToText(UnityGameState gs, Dictionary<string, BundleVersionData> dict)
    {
        if (dict == null)
        {
            return "";
        }


        StringBuilder sb = new StringBuilder();
        foreach (var key in dict.Keys)
        {
            var kvb = dict[key];
            if (!string.IsNullOrEmpty(kvb.Name) &&
                !string.IsNullOrEmpty(kvb.Hash))
            {
                sb.Append(kvb.Name + " " + kvb.Hash + " " + kvb.Size + "\n");
            }
        }
        return sb.ToString();
    }

    protected void SaveBundleVersionsText(UnityGameState gs)
    {
        string txt = ConvertBundleVersionsToText(gs, _bundleVersions);

        if (!string.IsNullOrEmpty(txt))
        {
            var stringRepo = new LocalFileRepository(gs.logger);
            var filename = GetLocalBundleVersionPath(gs);
            stringRepo.Save(filename, txt);
        }
    }

    private async Task DownloadOneBundle(UnityGameState gs, BundleDownload bad, CancellationToken token)
    {
        if (string.IsNullOrEmpty(bad.url))
        {
            return;
        }
               
        for (int i = 0; i < RetryTimes; i++)
        {
            string bundleHash = GetBundleHash(gs, bad.bundleName);
            if (string.IsNullOrEmpty(bundleHash))
            {
                _logger.Debug("No bundle hash for: " + bad.url);
                return;
            }

            using (UnityWebRequest request = UnityWebRequestAssetBundle.GetAssetBundle(bad.url,
                GetBundleHash128(gs, bad.bundleName)))
            {
                UnityWebRequestAsyncOperation asyncOp = request.SendWebRequest();
                while (!asyncOp.isDone)
                {
                    await Task.Delay(1);
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
                    if (bad.unloadImmediately && !IsNeverUnloadBundle(gs, bad.bundleName))
                    {
                        downloadedBundle.Unload(true);
                        BundlesUnloaded++;
                    }
                    else
                    {
                        AddBundleToCache(gs, bad, downloadedBundle);
                    }

                    request.Dispose();
                    return;
                }
                else
                {
                    request.Dispose();
                    await Task.Delay(TimeSpan.FromSeconds(1.1f), cancellationToken: token);
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

    public void GetSpriteList (UnityGameState gs, string atlasName, SpriteListDelegate onLoad, CancellationToken token)
    {
        LoadSprite(gs, atlasName, "", OnDownloadSpriteForList, onLoad, token);
    }
    private static List<String> RootBundles = new List<string> { AssetCategory.UI, AssetCategory.Magic };

    /// <summary>
    /// Get the bundle name for an asset, leave an override so later on I can have different
    /// categories of asset bundles or different numbers of asset bundles for different
    /// games.
    /// </summary>
    /// <param name="gs"></param>
    /// <param name="assetPath"></param>
    /// <param name="assetCategory"></param>
    /// <returns></returns>
    public virtual string GetBundleForCategoryAndAsset(UnityGameState gs, string assetCategory, string assetPath)
    {
        if (string.IsNullOrEmpty(assetPath))
        {
            return "badbundle";
        }

        if (RootBundles.Contains(assetCategory))
        {
            return assetCategory.ToLower();
        }
        // If no asset category
        if (string.IsNullOrEmpty(assetCategory))
        {
            Dictionary<string, AssetCategory> paths = GetAllAssetCategories();
            if (paths != null)
            {

                // Search all asset categories
                foreach (string key in paths.Keys)
                {
                    if (paths[key].Path != null)
                    {
                        string val = paths[key].Path.ToLower();
                        // And if the beginning of the asset path is the same as this category path,
                        // set the category to this category. This will include a /  so there hopefully
                        // won't be collisions here.
                        if (assetPath.IndexOf(val) == 0)
                        {
                            assetPath = assetPath.Replace(val, "");
                            assetCategory = key;
                            break;
                        }
                    }
                }
            }
        }

        string baseAssetPath = GetAssetPath(assetCategory);




        string lowerBaseAssetPath = baseAssetPath.ToLower();

        string fullName = "";

        if (assetPath.IndexOf(lowerBaseAssetPath) >= 0)
        {
            fullName = assetPath;
            if (lowerBaseAssetPath != baseAssetPath)
            {
                fullName = fullName.Replace(lowerBaseAssetPath, baseAssetPath);
            }
        }
        else
        {
            fullName = baseAssetPath + assetPath;
        }

        if (!string.IsNullOrEmpty(assetCategory) &&
            assetCategory.ToLower().IndexOf(AssetCategory.DirectoryBundleSuffix.ToLower()) >= 0)
        {
            string newName = fullName.Replace(baseAssetPath, "");
            int slashIndex = newName.IndexOf("/");
            if (slashIndex < 1)
            {
                return StripPathPrefix(baseAssetPath);
            }
            string bundleName = baseAssetPath + newName.Substring(0, slashIndex);

            char[] nameArray = bundleName.Where(x => char.IsLetter(x) || char.IsDigit(x)).ToArray();

            return new string(nameArray).ToLower();
        }
        else
        {

            char[] assetArray = fullName.Where(x => char.IsLetter(x)).ToArray();

            string bundleName = new string(assetArray).ToLower();
            return bundleName;
        }
    }

    private string _artPrefix = "";
    public string GetArtURLPrefixInner(UnityGameState gs)
    {
        if (!string.IsNullOrEmpty(_artPrefix))
        {
            return _artPrefix;
        }

        _artPrefix = gs.ArtURL;
        return _artPrefix;
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

    List<String> _initialDownloadBundles = null;
    public List<string> GetInitialDownloadBundleList(UnityGameState gs)
    {
        if (_initialDownloadBundles == null)
        {
            List<string> list = new List<string>();
            list.Add(GetBundleForCategoryAndAsset(gs, AssetCategory.Magic, "SpellName"));
            _initialDownloadBundles = list;
        }
        return _initialDownloadBundles;

    }

    public bool IncrementalUnloadOnly(UnityGameState gs, string bundleName)
    {
        if (string.IsNullOrEmpty(bundleName))
        {
            return false;
        }

        if (bundleName.IndexOf("music") >= 0)
        {
            return true;
        }

        return false;
    }

    public bool IsNeverUnloadBundle(UnityGameState gs, string bundleName)
    {
        if (string.IsNullOrEmpty(bundleName))
        {
            return false;
        }

        string bname = bundleName.ToLower();
        if (bname.IndexOf("atlas") == 0)
        {
            return true;
        }

        if (bname.IndexOf("shader") == 0)
        {
            return true;
        }

        if (bname == AssetCategory.Magic.ToLower())
        {
            return true;
        }


        if (bname.IndexOf(AssetCategory.Screens.ToLower()) == 0 ||
            bname.IndexOf(AssetCategory.UI.ToLower()) == 0)
        {
            return true;
        }

        return false;
    }

    public void LoadAssetInto(UnityGameState gs, object parent, string assetCategory, string assetPath, OnDownloadHandler handler, object data, CancellationToken token)
    {
        LoadAsset(gs, assetCategory, assetPath, handler, data, parent, token);
    }

    private static void SetupAssetCategories()
    {
        // Asset path setup.
        _assetCategoryData = new Dictionary<string, AssetCategory>();
        AddAssetPath(AssetCategory.Atlas, "Atlas/", true);
        AddAssetPath(AssetCategory.Screens, "Screens/", false);
        AddAssetPath(AssetCategory.UI, "UI/", false);
        AddAssetPath(AssetCategory.Audio, "Audio/", true);
        AddAssetPath(AssetCategory.Prefabs, "Prefabs/", true);
        AddAssetPath(AssetCategory.MonsterTex, "Monsters/Textures/", false);
        AddAssetPath(AssetCategory.Monsters, "Monsters/", false);
        AddAssetPath(AssetCategory.Bushes, "Bushes/", false);
        AddAssetPath(AssetCategory.Trees, "Trees/", false);
        AddAssetPath(AssetCategory.Rocks, "Rocks/", false);
        AddAssetPath(AssetCategory.Props, "Props/", false);
        AddAssetPath(AssetCategory.TerrainTex, "TerrainTex/", false);
        AddAssetPath(AssetCategory.Grass, "Grass/", false);
        AddAssetPath(AssetCategory.Magic, "Magic/", false);
    }


    private static Dictionary<string, AssetCategory> _assetCategoryData = null;
    private static void AddAssetPath(string key, string val, bool checkResources)
    {
        
        if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(val))
        {
            return;
        }
        if (_assetCategoryData.ContainsKey(key))
        {
            return;
        }
        AssetCategory cat = new AssetCategory() { Name = key, Path = val, ShouldCheckResources = checkResources };
        _assetCategoryData[key] = cat;
    }

    public static string GetAssetPath(string key)
    {
        if (_assetCategoryData == null)
        {
            SetupAssetCategories();
        }
        if (string.IsNullOrEmpty(key) || !_assetCategoryData.ContainsKey(key))
        {
            return "";
        }
        return _assetCategoryData[key].Path;
    }

    public static Dictionary<string, AssetCategory> GetAllAssetCategories()
    {
        return _assetCategoryData;
    }

    public static bool ShouldCheckResources(string key)
    {
        if (_assetCategoryData == null)
        {
            SetupAssetCategories();
        }
        if (string.IsNullOrEmpty(key) || !_assetCategoryData.ContainsKey(key))
        {
            return true;
        }
        return _assetCategoryData[key].ShouldCheckResources;
    }
}

