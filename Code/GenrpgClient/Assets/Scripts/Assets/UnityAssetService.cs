using UnityEngine.U2D;
using UnityEngine.Networking;
using System.Collections.Generic;
using System;
using System.Linq;

using System.Threading;
using UnityEngine;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Logging.Interfaces;
using System.Threading.Tasks;
using Genrpg.Shared.Client.Core;
using Genrpg.Shared.Constants;
using Genrpg.Shared.DataStores.Utils;
using Genrpg.Shared.DataStores.DataGroups;
using Genrpg.Shared.Client.Assets.Services;
using Genrpg.Shared.Client.Assets.Constants;
using Assets.Scripts.GameObjects;
using Genrpg.Shared.MVC.Interfaces;
using Assets.Scripts.MVC;
using Assets.Scripts.Awaitables;
using Assets.Scripts.Assets;

#if UNITY_EDITOR
using UnityEditor;
#endif


internal class GameObjectContainer
{
    public GameObject Entity;
    public bool FailedLoad;
}

public class AtlasSpriteDownload
{
    public string atlasName;
    public string spriteName;
    public OnDownloadHandler finalHandler;
    public object data;
}

public class CachedSpriteTexture
{
    public string SpriteName;
    public Sprite CurrSprite;
    public int Count;
    public DateTime LastTimeUsed = DateTime.UtcNow;
}

public class SpriteWithImage
{
    public GImage Image;
    public CachedSpriteTexture Cache;
}

public class BundleDownload
{
	public string url;
	public string bundleName;
	public string assetName;
	public OnDownloadHandler handler;
	public System.Object data;
    public GameObject parent;
}

public class BundleCacheData
{
    public string name;
    public AssetBundle assetBundle;
    public DateTime LastUsed = DateTime.UtcNow;
    public int LoadingCount;
    public Dictionary<string, object> LoadedAssets = new Dictionary<string, object>();
    public List<object> Instances = new List<object>();
    public bool KeepLoaded = false;

    public List<BundleCacheData> ParentDependencies { get; set; } = new List<BundleCacheData>();
    public List<BundleCacheData> ChildDependencies { get; set; } = new List<BundleCacheData>();
}


public class UnityAssetService : IAssetService
{
    private ILogService _logService;
    private IFileDownloadService _fileDownloadService;
    protected IClientRandom _rand;
    protected IClientGameState _gs;
    protected IClientEntityService _clientEntityService;
    private IClientConfigContainer _config; 
    private ISingletonContainer _singletonContainer;
    private IClientAppService _clientAppService;
    private IBinaryFileRepository _binaryFileRepo;
    private IInitClient _initClient;

    private IAwaitableService _awaitableService;
    private ILocalLoadService _localLoadService;

    private const float _loadDelayChance = 0.03f;

    private const int _maxConcurrentExistingDownloads = 5;

    private const int _maxConcurrentBundleDownloads = 3;

    private const int _retryTimes = 3;

    protected bool _isInitialized = false;

    protected Dictionary<string, BundleCacheData> _bundleCache = new Dictionary<string, BundleCacheData>();

    protected Dictionary<string, List<BundleDownload>> _bundleDownloading = new Dictionary<string, List<BundleDownload>>();

    protected Dictionary<string, List<BundleDownload>> _bundleDownloadQueue = new Dictionary<string, List<BundleDownload>>();

    private Dictionary<EDataCategories, string> _urlPrefixes = new Dictionary<EDataCategories, string>();
    private Dictionary<EDataCategories, string> _assetEnvs = new Dictionary<EDataCategories, string>();

    protected HashSet<string> _bundleFailedDownloads = new HashSet<string>();

    protected HashSet<string> _failedLocalLoads = new HashSet<string>(); 
    private Dictionary<string, GameObject> _localLoads = new Dictionary<string, GameObject>();

    protected Dictionary<string, SpriteAtlas> _atlasCache = new Dictionary<string, SpriteAtlas>();
    protected Dictionary<string, Sprite[]> _spriteListCache = new Dictionary<string, Sprite[]>();
    protected Dictionary<string, Sprite> _atlasSpriteCache = new Dictionary<string, Sprite>();

    protected BundleVersions _bundleVersions = null;
    protected BundleUpdateInfo _bundleUpdateInfo = null;

    private Queue<BundleDownload> _existingDownloads = new Queue<BundleDownload>();


    private string _contentRootUrl = null;


    private ClientAssetCounts _assetCounts = new ClientAssetCounts();

    public ClientAssetCounts GetAssetCounts()
    {
        return _assetCounts;
    }

    public string GetWorldDataEnv() 
    { 
        return _assetEnvs[EDataCategories.Worlds]; 
    }

    private BundleVersion GetBundleVersion(string bundleName)
    {
        if (_bundleVersions.Versions.TryGetValue(bundleName, out BundleVersion version))
        {
            return version;
        }
        return null;
    }

    private BundleCacheData GetBundleCacheData(string bundleName)
    {
        if (_bundleCache.TryGetValue(bundleName, out  BundleCacheData bundleCacheData))
        {
            return bundleCacheData;
        }
        return null;
    }
       

    public async Task Initialize(CancellationToken token)
    {
        if (!_clientAppService.IsPlaying)
        {
            return;
        }

        _contentRootUrl = _config.Config.GetContentRoot();
        SetAssetEnv(EDataCategories.Assets, _config.Config.GetAssetDataEnv());
        SetAssetEnv(EDataCategories.Worlds, _config.Config.GetWorldDataEnv());

        _assetParent = _singletonContainer.GetSingleton(AssetConstants.GlobalAssetParent);
        SpriteAtlasManager.atlasRequested += DummyRequestAtlas;
        SpriteAtlasManager.atlasRegistered += DummyRegisterAtlas;
        string persPath = _clientAppService.PersistentDataPath;
        for (int i = 0; i < _maxConcurrentExistingDownloads; i++)
        {
            _awaitableService.ForgetAwaitable(LoadFromExistingBundles(true, token));
        }
        for (int i = 0; i < _maxConcurrentBundleDownloads; i++)
        {
            _awaitableService.ForgetAwaitable(DownloadNewBundles(token));
        }

        _awaitableService.ForgetAwaitable(IncrementalClearMemoryCache(token));


        if (_initClient.PlayerContainsAllAssets())
        {
            BypassInitForPlayerWithAllAssets();
        }
        else
        {
            LoadLastSaveTimeFile(token);
        }
        await Task.CompletedTask;
    }

    private void BypassInitForPlayerWithAllAssets()
    {

        _bundleUpdateInfo = new BundleUpdateInfo() { UpdateTime = DateTime.UtcNow.Date };
        _bundleVersions = new BundleVersions() { UpdateInfo = _bundleUpdateInfo };
        _isInitialized = true;
    }

    public bool IsDownloading()
    {
        return 
            _existingDownloads.Count > 0 ||
            _bundleDownloading.Keys.Count > 0 ||
            _bundleDownloadQueue.Keys.Count > 0;
    }

    private void DummyRequestAtlas(string tag, System.Action<SpriteAtlas> callback)
    {

    }

    private void DummyRegisterAtlas(SpriteAtlas callback)
    {

    }

    public async Task OnCleanup(CancellationToken token)
    {
        await ClearBundleCache(token);
    }

    public async Task ClearBundleCache(CancellationToken token)
    {
        Dictionary<string,BundleCacheData> newBundleCache = new Dictionary<string, BundleCacheData>();

        foreach (string item in _bundleCache.Keys)
        {
            BundleCacheData bdata = _bundleCache[item];

            if (bdata.assetBundle != null)
            {
                bdata.assetBundle.Unload(true);
                _assetCounts.BundlesUnloaded++;
                _clientEntityService.Destroy(bdata.assetBundle);
            }
        }

        _bundleCache = newBundleCache;
        await UnloadUnusedAssets(token);
    }

    protected async Awaitable IncrementalClearMemoryCache(CancellationToken token)
    {
        while (true)
        {
            await Awaitable.WaitForSecondsAsync(5.0f, cancellationToken: token);

            int removeCount = 0;
            List<string> bundleCacheKeys = _bundleCache.Keys.ToList();
            foreach (string item in bundleCacheKeys)
            {
                if (_bundleCache.TryGetValue(item, out BundleCacheData bundle))
                {
                    if (bundle.LoadingCount < 1 &&
                        bundle.assetBundle != null &&
                        !bundle.KeepLoaded &&
                        bundle.LastUsed < DateTime.UtcNow.AddSeconds(-20) &&
                        bundle.ParentDependencies.Count < 1)
                    {
                        if (bundle.Instances.Any(x => x.Equals(null)))
                        {
                            bundle.Instances = bundle.Instances.Where(x => !x.Equals(null)).ToList();
                        }
                        if (bundle.Instances.Count > 0)
                        {
                            continue;
                        }
                        foreach (BundleCacheData childBundle in bundle.ChildDependencies)
                        {
                            childBundle.ParentDependencies.Remove(bundle);
                        }
                        bundle.assetBundle.Unload(true);
                        _assetCounts.BundlesUnloaded++;
                        _clientEntityService.Destroy(bundle.assetBundle);
                        _bundleCache.Remove(item);
                        removeCount++;
                        if (removeCount > 5)
                        {
                            break;
                        }
                        await Awaitable.NextFrameAsync(cancellationToken: token);
                    }
                }
            }
            if (removeCount > 0)
            {
                await UnloadUnusedAssets(token);
            }
        }
    }

    private bool _unloadingAssets = false;
    private async Awaitable UnloadUnusedAssets(CancellationToken token)
    {
        if (_unloadingAssets)
        {
            return;
        }
        _unloadingAssets = true;
        _localLoads.Clear();
        AsyncOperation op = Resources.UnloadUnusedAssets();
        while (!op.isDone)
        {
            await Awaitable.NextFrameAsync(cancellationToken: token);
        }
_unloadingAssets = false;
    }

    private string GetAssetNameFromPath(AssetBundle assetBundle, string assetName)
    {
        if (assetBundle == null || string.IsNullOrEmpty(assetName))
        {
            return assetName;
        }

        assetName = assetName.ToLower();
        if (assetName.IndexOf("/") >= 0)
        {
            assetName = assetName.Substring(assetName.LastIndexOf("/") + 1);
        }

        string fullAssetName = assetName;
        string[] assetNames = assetBundle.GetAllAssetNames();
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
     
    public string GetAssetPath(string assetCategoryName)
    {
        return assetCategoryName + "/";
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
    public void LoadAsset(string assetPathSuffix, string assetName,
            OnDownloadHandler handler,
            System.Object data, object parentIn,
            CancellationToken token, string subdirectory = null)
    {
        if (!TokenUtils.IsValid(token))
        {
            return;
        }
        GameObject parent = parentIn as GameObject;

        if (parent == null)
        {
            MonoBehaviour mb = parentIn as MonoBehaviour;
            if (mb != null)
            {
                parent = mb.gameObject;
            }
        }

        if (string.IsNullOrEmpty(assetName))
        {
            if (handler != null)
            {
                handler(null, data, token);
            }
            return;
        }

        if (_atlasSpriteCache.ContainsKey(assetName))
        {
            if (handler != null)
            {
                handler(_atlasSpriteCache[assetName], data, token);
            }
            return;
        }

        if (!string.IsNullOrEmpty(subdirectory))
        {
            assetPathSuffix += "/" + subdirectory;
        }
        if (_initClient.PlayerContainsAllAssets())
        {
            string categoryPath = GetAssetPath(assetPathSuffix);
            string fullAssetName = categoryPath + assetName;
            if (!String.IsNullOrEmpty(categoryPath) &&
            !_failedLocalLoads.Contains(fullAssetName))
            {
                UnityEngine.Object asset = null;
                string fullPath = "";
#if UNITY_EDITOR
                fullPath = AssetConstants.DownloadAssetRootPath + fullAssetName +
                (assetName.IndexOf(AssetConstants.ArtFileSuffix) < 0 ? AssetConstants.ArtFileSuffix : "");
                if (_localLoads.ContainsKey(fullPath))
                {
                    asset = _localLoads[fullPath];
                }
                else
                {
                    asset = AssetDatabase.LoadAssetAtPath<GameObject>(fullPath);
                }
#else
                fullPath = fullAssetName;
                asset = _localLoadService.LocalLoad<GameObject>(fullPath);               
#endif
                

                if (asset != null)
                {
                    asset = InstantiateIntoParent(asset, parent);

                    _clientEntityService.InitializeHierarchy(asset as GameObject);

                    if (handler != null)
                    {
                        handler(asset, data, token);
                    }
                }
                else
                {
                    _failedLocalLoads.Add(assetName);
                }
            }
            return;
        }

        string bundleName = GetBundleNameForCategoryAndAsset(assetPathSuffix, assetName);

        if (_bundleFailedDownloads.Contains(bundleName))
        {
            if (handler != null)
            {
                handler(null, data, token);
            }
            return;
        }
        if (assetName.LastIndexOf("/") >= 0)
        {
            assetName = assetName.Substring(assetName.LastIndexOf("/") + 1);
        }


        AddBundleDownload(bundleName, assetName, handler, data, parent, new List<string>());
    }

    private void QueueBundleDependencies(string bundleName, List<string> parentBundles)
    {
        if (parentBundles.Contains(bundleName))
        {
            return;
        }

        if (_bundleVersions.Versions.TryGetValue(bundleName, out BundleVersion version))
        {
            if (version.ChildDependencies.Count < 1)
            {
                return;
            }

            foreach (string dependency in version.ChildDependencies)
            {
                parentBundles.Add(dependency);
                AddBundleDownload(dependency, null, null, null, null, parentBundles);
            }
        }
    }

    private void AddBundleDownload(string bundleName, string assetName, OnDownloadHandler handler, object data, GameObject parent, List<string> parentBundles)
    {

        BundleDownload currentBundleDownload = null;
        currentBundleDownload = new BundleDownload();
        currentBundleDownload.bundleName = bundleName;
        currentBundleDownload.assetName = assetName;
        currentBundleDownload.handler = handler;
        currentBundleDownload.data = data;
        currentBundleDownload.parent = parent;
        currentBundleDownload.url = GetFullBundleURL(bundleName);

        if (_bundleCache.ContainsKey(bundleName))
        {
            _bundleCache[bundleName].LastUsed = DateTime.UtcNow;
            _existingDownloads.Enqueue(currentBundleDownload);
            return;
        }

        QueueBundleDependencies(bundleName, parentBundles);

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

    public void SetWorldAssetEnv(string worldAssetEnv)
    {
        SetAssetEnv(EDataCategories.Worlds, worldAssetEnv);
    }

    private void SetAssetEnv(EDataCategories category, string env)
    {

        string containerName = BlobUtils.GetBlobContainerName(category.ToString(), Game.Prefix, env);

        _urlPrefixes[category] = _contentRootUrl + "/" + containerName + "/";
        _assetEnvs[category] = env;
    }

    public string GetContentRootURL(EDataCategories dataCategory)
    {
        if (_urlPrefixes.TryGetValue(dataCategory, out string url))
        {
            return url;
        }

        return null;
    }

    public bool IsInitialized()
    {
        return _isInitialized;
    }

    private async Awaitable PermanentLoadFromExistingBundles(CancellationToken token)
    {
        await LoadFromExistingBundles(true, token);
    }

    private async Awaitable LoadFromExistingBundles(bool isPermanentLoader, CancellationToken token)
    {
        await Awaitable.NextFrameAsync(cancellationToken: token);

        while (true)
        {
            if (_existingDownloads.Count > 0)
            {
                try
                {
                    await LoadAssetFromExistingBundle(_existingDownloads.Dequeue(), token);
                }
                catch (Exception e)
                {
                    _logService.Exception(e, "LoadFromExistingBundles");
                }
                if (_rand.NextDouble() < _loadDelayChance)
                {
                    await Awaitable.NextFrameAsync(cancellationToken: token);
                }
            }
            else
            {
                if (!isPermanentLoader)
                {
                    return;
                }
                await Awaitable.NextFrameAsync(cancellationToken: token);
            }
        }
    }

    private async Awaitable DownloadNewBundles(CancellationToken token)
    {
        while (true)
        {
            if (_bundleDownloadQueue.Keys.Count > 0)
            {
                string firstKey = _bundleDownloadQueue.Keys.First();
                List<BundleDownload> firstDownloadList = _bundleDownloadQueue[firstKey];
                _bundleDownloadQueue.Remove(firstKey);
                if (firstDownloadList.Count < 1)
                {
                    await Awaitable.NextFrameAsync(cancellationToken: token);
                }
                else
                {
                    _bundleDownloading[firstKey] = firstDownloadList;
                    await DownloadOneBundle(firstDownloadList[0], token);
                    _bundleDownloading.Remove(firstKey);
                    if (!_bundleCache.ContainsKey(firstDownloadList[0].bundleName))
                    {
                        foreach (BundleDownload bundleDownload in firstDownloadList)
                        {
                            bundleDownload?.handler(null, bundleDownload.data, token);
                        }
                    }
                    else
                    {
                        foreach (BundleDownload bundleDownload in firstDownloadList)
                        {
                            _existingDownloads.Enqueue(bundleDownload);
                        }
                    }
                }
            }
            else
            {
                await Awaitable.NextFrameAsync(cancellationToken: token);
            }
        }
    }

    private async Awaitable LoadAssetFromExistingBundle(BundleDownload bdl, CancellationToken token)
    {
        // Need to check existence of bundle here since this call is delayed from when 
        if (bdl == null || !_bundleCache.ContainsKey(bdl.bundleName) || string.IsNullOrEmpty(bdl.assetName))
        {
            return;
        }
        BundleCacheData bdata = _bundleCache[bdl.bundleName];
        bdata.LoadingCount++;
        bdata.LastUsed = DateTime.UtcNow;
        if (!bdata.LoadedAssets.ContainsKey(bdl.assetName))
        {
            AssetBundleRequest request = StartLoadAssetFromBundle(bdl.bundleName, bdl.assetName);
            if (request != null)
            {
                while (!request.isDone)
                {
                    await Awaitable.NextFrameAsync(cancellationToken: token);
                }
                bdata.LoadedAssets[bdl.assetName] = request.asset;
            }
        }

        if (bdata.LoadedAssets.ContainsKey(bdl.assetName))
        {
            bdata.LoadingCount--;

             object newObj = InstantiateBundledAsset(bdata.LoadedAssets[bdl.assetName], bdl.parent, bdl.bundleName, bdl.assetName);
            if (bdl.handler != null)
            {
                bdl.handler(newObj, bdl.data, token);
                return;
            }
        }
        else
        {
            bdata.LoadingCount--;
            return;
        }
    }

    private void AddBundleToCache(BundleDownload bad, AssetBundle downloadedBundle, CancellationToken token)
    {
        if (downloadedBundle == null || _bundleCache.ContainsKey(bad.bundleName))
        {
            return;
        }

        BundleCacheData bdata = new BundleCacheData()
        {
            name = bad.bundleName,
            assetBundle = downloadedBundle,
            LastUsed = DateTime.UtcNow,
            KeepLoaded = (bad.bundleName.IndexOf("atlas") == 0
            || bad.bundleName.IndexOf("screen") == 0
            || bad.bundleName.IndexOf("ui") == 0),
        };

        _bundleCache[bad.bundleName] = bdata;
        _assetCounts.BundlesLoaded++;

        BundleVersion version = GetBundleVersion(bad.bundleName);

        if (version != null)
        {
            foreach (string dep in version.ChildDependencies)
            {

                BundleCacheData childData = GetBundleCacheData(dep);

                if (childData == null)
                {
                    _logService.Error("Bundle " + bad.bundleName + " is missing dependency bundle " + dep);
                    continue;
                }
                childData.ParentDependencies.Add(bdata);
                bdata.ChildDependencies.Add(childData);
            }
        } 
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
        AssetBundle bundle = cacheData.assetBundle;
        string fullName = GetAssetNameFromPath(bundle, assetName);

        try
        {
            return bundle.LoadAssetAsync(assetName);
        }
        catch (Exception e)
        {
            _logService.Exception(e, "Failed asset Load:" + assetName);
        }
        return null;
    }


    public void LoadSpriteWithAtlasNameInto(string atlasSlashSpriteName, object parentObject, CancellationToken token)
    {
        string[] words = atlasSlashSpriteName.Split('/');
        if (words.Length != 2 || string.IsNullOrEmpty(words[0]) || string.IsNullOrEmpty(words[1]))
        {
            _logService.Warning("Full Sprite name doesn't have format Atlas/Sprite: " + atlasSlashSpriteName);
            return;
        }


        LoadAtlasSpriteInto(words[0], words[1], parentObject, token);
    }

    public void LoadAtlasSpriteInto(string atlasName, string spriteName, object parentObject, CancellationToken token)
    {
        LoadAtlasSprite(atlasName, spriteName, null, parentObject, token);
    }


    public GameObject _assetParent = null;
    private void LoadAtlasSprite(string atlasName, string spriteName, OnDownloadHandler handler, object parentSprite, CancellationToken token)
    {
        if (string.IsNullOrEmpty(atlasName))
        {
            if (handler != null)
            {
                handler(null, parentSprite, token);
            }
            return;
        }

        AtlasSpriteDownload atlasDownload = new AtlasSpriteDownload()
        {
            atlasName = atlasName,
            spriteName = spriteName,
            finalHandler = handler,
            data = parentSprite,
        };

        if (_atlasCache.ContainsKey(atlasName))
        {
            GetAtlasSprite(atlasDownload, token);
            return;
        }

        LoadAssetInto(_assetParent, AssetCategoryNames.Atlas, atlasName, OnDownloadAtlas, atlasDownload, token);

    }

    private void OnDownloadAtlas(object obj, object data, CancellationToken token)
    {
        AtlasSpriteDownload atlasSpriteDownload = data as AtlasSpriteDownload;
        GameObject go = obj as GameObject;

        if (go == null)
        {
            if (atlasSpriteDownload != null && atlasSpriteDownload.finalHandler != null)
            {
                atlasSpriteDownload.finalHandler(null, atlasSpriteDownload.data, token);
            }

            return;
        }

        if (atlasSpriteDownload == null || string.IsNullOrEmpty(atlasSpriteDownload.atlasName))
        {
            _clientEntityService.Destroy(go);

            if (atlasSpriteDownload != null && atlasSpriteDownload.finalHandler != null)
            {
                atlasSpriteDownload.finalHandler(null, atlasSpriteDownload.data, token);
            }
            return;
        }

        SpriteAtlasContainer atlasCont = go.GetComponent<SpriteAtlasContainer>();
        if (atlasCont == null || atlasCont.Atlas == null)
        {
            if (atlasSpriteDownload.finalHandler != null)
            {
                atlasSpriteDownload.finalHandler(null, atlasSpriteDownload.data, token);
            }
        }

        if (_atlasCache.ContainsKey(atlasSpriteDownload.atlasName))
        {
            _clientEntityService.Destroy(go);
        }
        else
        {
            _atlasCache[atlasSpriteDownload.atlasName] = atlasCont.Atlas;
        }

        GetAtlasSprite(atlasSpriteDownload, token);

    }

    public string GetSpriteCacheKey(string atlasName, string spriteName)
    {
        if (string.IsNullOrEmpty(atlasName) || string.IsNullOrEmpty(spriteName)) return "";
        return (spriteName + "." + atlasName).ToLower();
    }

    private void GetAtlasSprite(AtlasSpriteDownload download, CancellationToken token)
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
            download.finalHandler(atlasTemp, download.data, token);
            return;
        }
        
        SpriteAtlas atlas = _atlasCache[download.atlasName];

        string cacheName = GetSpriteCacheKey(download.atlasName, download.spriteName);

        Sprite sprite = null;
        if (_atlasSpriteCache.ContainsKey(cacheName))
        {
            sprite = _atlasSpriteCache[cacheName];
        }
        else
        {
            sprite = atlas.GetSprite(download.spriteName);
            if (sprite != null)
            {
                sprite.name = sprite.name.Replace("(Clone)", "");
                _atlasSpriteCache[cacheName] = sprite;
            }
            else
            {
                _logService.Debug("Missing sprite: " + download.spriteName);
            }
        }

        GImage image = download.data as GImage;
        if (image != null)
        {
            image.sprite = sprite;
        }


        if (download.finalHandler != null)
        {
            download.finalHandler(atlas, download.data, token);
        }
    }


    private void OnDownloadSpriteForList(object obj, object data, CancellationToken token)
    {
        SpriteListDelegate spriteDelegate = data as SpriteListDelegate;
        if (spriteDelegate == null)
        {
            return;
        }
        
        SpriteAtlas atlas = obj as SpriteAtlas;

        if (obj == null)
        {
            spriteDelegate(new Sprite[0]);
            return;
        }

        int spriteCount = atlas.spriteCount;
        Sprite[] retval = new Sprite[atlas.spriteCount];
        atlas.GetSprites(retval);
        spriteDelegate(retval);
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
        uint[] hashInts = GetBundleHashInts(bundleName);
        if (hashInts == null || hashInts.Length != 4) return new Hash128();
        return new Hash128(hashInts[0], hashInts[1], hashInts[2], hashInts[3]);
    }


    protected void LoadLastSaveTimeFile(CancellationToken token)
    {
        string path = _clientAppService.GetRuntimePrefix() + AssetConstants.BundleUpdateFile;
        DownloadFileData ddata = new DownloadFileData()
        {
            ForceDownload = true,
            Handler = OnDownloadLastSaveTimeText,
            IsText = true,
            Category = EDataCategories.Assets,
        };
        _fileDownloadService.DownloadFile(path, ddata, token);
    }

    private void OnDownloadLastSaveTimeText(object obj, object data, CancellationToken token)
    {
        _bundleUpdateInfo = SerializationUtils.TryDeserialize<BundleUpdateInfo>(obj);


        if (_bundleUpdateInfo == null)
        {
            _bundleUpdateInfo = new BundleUpdateInfo() { UpdateTime = DateTime.UtcNow.Date };
        }

        LoadAssetBundleList(token);
    }

    private BundleVersions SetupBundleVersions(BundleVersions versions)
    {
        if (versions == null)
        {
            return versions;
        }

        foreach (BundleVersion version in versions.Versions.Values)
        {
            foreach (string dep in version.ChildDependencies)
            {
                if (versions.Versions.TryGetValue(dep, out BundleVersion childBundle))
                {
                    if (!childBundle.ParentDependencies.Contains(dep))
                    {
                        childBundle.ParentDependencies.Add(dep);
                    }
                }
            }
        }

        return versions;
    }

    void LoadAssetBundleList(CancellationToken token)
    {
        _bundleVersions = SetupBundleVersions(_binaryFileRepo.LoadObject<BundleVersions>(AssetConstants.BundleVersionsFile));

        if (_bundleVersions == null || _bundleVersions.UpdateInfo == null ||
            _bundleVersions.UpdateInfo.ClientVersion != _bundleUpdateInfo.ClientVersion ||
            _bundleVersions.UpdateInfo.UpdateTime != _bundleUpdateInfo.UpdateTime)
        {
            DownloadFileData ddata = new DownloadFileData()
            {
                ForceDownload = true,
                Handler = OnDownloadBundleVersions,
                IsText = true,
                Category = EDataCategories.Assets,
            };

            string path = _clientAppService.GetRuntimePrefix() + AssetConstants.BundleVersionsFile;
            path += ("?timestamp=" + _bundleUpdateInfo.UpdateTime.Ticks);
            _fileDownloadService.DownloadFile(path, ddata, token);
            _logService.Info("YES DOWNLOAD BUNDLE VERSIONS!");
        }
        else
        {
            _isInitialized = true;
            _logService.Info("NO DOWNLOAD BUNDLE VERSIONS");
        }
    }

    private void OnDownloadBundleVersions(object obj, object data, CancellationToken token)
    {
        BundleVersions newVersions = SerializationUtils.TryDeserialize<BundleVersions>(obj);

        _isInitialized = true;

        if (newVersions != null && newVersions.UpdateInfo != null &&
            newVersions.Versions != null && newVersions.Versions.Keys.Count > 0)
        {
            _bundleVersions = SetupBundleVersions(newVersions);
            _binaryFileRepo.SaveObject(AssetConstants.BundleVersionsFile, _bundleVersions);
        }
    }

    protected string GetFullBundleURL(string bundleName)
    {
        return GetContentRootURL(EDataCategories.Assets) + _clientAppService.GetRuntimePrefix() + bundleName + "_" + GetBundleHash(bundleName);
    }

    private async Awaitable DownloadOneBundle(BundleDownload bad, CancellationToken token)
    {
        if (string.IsNullOrEmpty(bad.url))
        {
            return;
        }

        BundleVersion version = GetBundleVersion(bad.bundleName);

        if (version != null)
        {
            foreach (string dep in version.ChildDependencies)
            {

                BundleCacheData childData = null;

                while (childData == null)
                {
                    childData = GetBundleCacheData(dep);

                    if (childData == null)
                    {
                        await Awaitable.NextFrameAsync();
                    }
                }
            }
        }
        
        for (int i = 0; i < _retryTimes; i++)
        {
            string bundleHash = GetBundleHash(bad.bundleName);
            if (string.IsNullOrEmpty(bundleHash))
            {
                _logService.Debug("No bundle hash for: " + bad.url);
                return;
            }

            using (UnityWebRequest request = UnityWebRequestAssetBundle.GetAssetBundle(bad.url,
                GetBundleHash128(bad.bundleName)))
            {
                UnityWebRequestAsyncOperation asyncOp = request.SendWebRequest();
                while (!asyncOp.isDone)
                {
                    await Awaitable.NextFrameAsync(cancellationToken: token);
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
                        _logService.Exception(e, "FailedbundleDownload: " + bad.url + " " + bad.assetName);
                    }
                }

                if (downloadedBundle != null)
                {
                    AddBundleToCache(bad, downloadedBundle, token);

                    request.Dispose();
                    return;
                }
                else
                {
                    request.Dispose();
                    await Awaitable.WaitForSecondsAsync(1.1f, cancellationToken: token);
                }
            }
        }
        if (!_bundleFailedDownloads.Contains(bad.bundleName))
        {
            _bundleFailedDownloads.Add(bad.bundleName);
        }
    }


    protected object InstantiateBundledAsset(object child, GameObject parent, string bundleName, string assetName)
    {
        BundleCacheData bundleCache = _bundleCache[bundleName];
        if (child is Texture2D tex2d)
        {
            bundleCache.Instances.Add(tex2d);
            return tex2d;
        }

        GameObject go = InstantiateIntoParent(child, parent);

        if (go != null)
        {
            MonoBehaviour mbh = go.GetComponent<MonoBehaviour>();

            if (mbh == null)
            {
                mbh = go.AddComponent<BaseBehaviour>();
            }

            bundleCache.Instances.Add(mbh);
            mbh.destroyCancellationToken.Register(() =>
            {
                bundleCache.Instances.Remove(mbh);
            });
        }
        else
        {
            _logService.Error("Failed to load " + assetName + " from " + bundleName);
            return null;
        }
        BaseBehaviour oneBehavior = go.GetComponent<BaseBehaviour>();
        if (oneBehavior != null)
        {
            _clientEntityService.InitializeHierarchy(go);
        }
        return go;
    }

    public void GetSpriteList(string atlasName, SpriteListDelegate onLoad, CancellationToken token)
    {
        LoadAtlasSprite(atlasName, "", OnDownloadSpriteForList, onLoad, token);
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
    public virtual string GetBundleNameForCategoryAndAsset(string pathPrefix, string assetPath)
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

    public void LoadAssetInto(object parent, string assetPathSuffix, string assetPath, OnDownloadHandler handler, object data, CancellationToken token, string subdirectory = null)
    {
        LoadAsset(assetPathSuffix, assetPath, handler, data, parent, token, subdirectory);
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


    private GameObject InstantiateIntoParent(object child, GameObject parent)
    {
        GameObject go = child as GameObject;
        if (go == null)
        {
            return null;
        }
        go = GameObject.Instantiate<GameObject>(go);

        go.name = go.name.Replace("(Clone)", "");
        go.name = go.name.Replace(AssetConstants.ArtFileSuffix, "");

        if (parent != null)
        {
            _clientEntityService.AddToParent(go, parent);
        }
        return go;
    }
    public async Task<object> LoadAssetAsync(string assetCategory, string assetPath, object parent, CancellationToken token, string subdirectory = null)
    {
        return await LoadAssetAsync<object>(assetCategory, assetPath, parent, token, subdirectory);
    }

    public async Task<T> LoadAssetAsync<T>(string assetCategory, string assetPath, object parent, CancellationToken token, string subdirectory = null) where T : class
    {
        GameObjectContainer cont = new GameObjectContainer();
        LoadAssetInto(parent, assetCategory, assetPath, OnLoadEntityAsync, cont, token, subdirectory);

        while (cont.Entity == null && !cont.FailedLoad)
        {
            await Awaitable.NextFrameAsync(token);
        }
           
        if (typeof(T) == typeof(object))
        {
            return cont.Entity as T;
        }
        else
        {
            return _clientEntityService.GetComponent<T>(cont.Entity);
        }
    }

    private void OnLoadEntityAsync(object obj, object data, CancellationToken token)
    {
        GameObjectContainer cont = data as GameObjectContainer;

        if (cont == null)
        {
            return;
        }

        cont.Entity = obj as GameObject;

        if (cont.Entity == null)
        {
            cont.FailedLoad = true;
        }

    }

    public void UnloadAsset(object obj)
    {
        if (obj is UnityEngine.Object uobj)
        {
            Resources.UnloadAsset(uobj);
        }
    }

    public List<T> LoadAllResources<T>(string path)
    {
        UnityEngine.Object[] objs = Resources.LoadAll(path);
        List<T> retval = new List<T>(); 

        foreach (UnityEngine.Object obj in objs)
        {
            if (obj is T t)
            {
                retval.Add(t);
            }
        }
        return retval;

        
      
    }

    public async Task<VC> CreateAsync<VC, TModel>(TModel model, string assetCategoryName, string assetPath, object parent, CancellationToken token, string subdirectory) where VC : class, IViewController<TModel, IView>, new()
    {
        IView view = await LoadAssetAsync<IView>(assetCategoryName, assetPath, parent, token, subdirectory);
        if (view != null)
        {
            return await InitViewControllerInternal<VC, TModel>(model, view, parent, token, false);
        }
        return default(VC); 
    }

    public async Task<VC> InitViewController<VC, TModel>(TModel model, object viewObj, object parent, CancellationToken token) where VC : class, IViewController<TModel, IView>, new()
    {
        return await InitViewControllerInternal<VC, TModel>(model, viewObj, parent, token, true);
    }

    private async Task<VC> InitViewControllerInternal<VC,TModel>(TModel model, object viewObj, object parent, CancellationToken token, bool dupeObject) where VC : class, IViewController<TModel, IView>, new()
    {
        if (dupeObject)
        {
            viewObj = _clientEntityService.FullInstantiate(viewObj);
        }
        if (viewObj is BaseView viewDupe)
        {
            VC viewController = new VC();
            _gs.loc.Resolve(viewController);
            await viewController.Init(model, viewDupe, token);
            _clientEntityService.AddToParent(viewDupe.gameObject, parent);
            _clientEntityService.SetActive(viewDupe.gameObject, true);
            return viewController;
        }
        return default(VC);
    }

    public void Create<VC, TModel>(TModel model, string assetCategoryName, string assetPath, object parent, Action<VC, CancellationToken> handler, CancellationToken token, string subdirectory) where VC : class, IViewController<TModel, IView>, new()
    {
        LoadAssetInto(parent, assetCategoryName, assetPath,
            (obj, data, token) =>
            {
                Task.Run( async () => 
                {
                    VC vc = await InitViewController<VC, TModel>(model, obj, parent, token);
                    handler?.Invoke(vc, token);
                });
            },
            model, token, subdirectory);
   
    }
}

