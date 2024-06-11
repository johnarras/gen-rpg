using GEntity = UnityEngine.GameObject;
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
using Genrpg.Shared.Core.Entities;
using Assets.Scripts.ProcGen.RandomNumbers;
using Assets.Scripts.Assets.Bundles;







#if UNITY_EDITOR
using UnityEditor;
#endif

public delegate void OnDownloadHandler(object obj, object data, CancellationToken token);

public delegate void SpriteListDelegate (Sprite[] sprites);


internal class GEntityContainer
{
    public GEntity Entity;
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
    public GEntity parent;
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
    private ILogService _logService;
    private IFileDownloadService _fileDownloadService;
    protected IClientRandom _rand;

    private static float _loadDelayChance = 0.03f;

    private IUnityGameState _gs;

    private const int _maxConcurrentExistingDownloads = 5;

    private const int _maxConcurrentBundleDownloads = 3;

    private const int _retryTimes = 3;

    protected bool _isInitialized = false;

    protected Dictionary<string, BundleCacheData> _bundleCache = new Dictionary<string, BundleCacheData>();

    protected Dictionary<string, List<BundleDownload>> _bundleDownloading = new Dictionary<string, List<BundleDownload>>();

    protected Dictionary<string, List<BundleDownload>> _bundleDownloadQueue = new Dictionary<string, List<BundleDownload>>();


    protected HashSet<string> _bundleFailedDownloads = new HashSet<string>();

#if UNITY_EDITOR
    protected HashSet<string> _failedAssetPathLoads = new HashSet<string>();
#endif
    protected Dictionary<string, SpriteAtlas> _atlasCache = new Dictionary<string, SpriteAtlas>();
    protected Dictionary<string, Sprite[]> _spriteListCache = new Dictionary<string, Sprite[]>();
    protected Dictionary<string, Sprite> _atlasSpriteCache = new Dictionary<string, Sprite>();

    protected BundleVersions _bundleVersions = null;
    protected BundleUpdateInfo _bundleUpdateInfo = null;


    private BinaryFileRepository _localRepo = null;

    private Queue<BundleDownload> _existingDownloads = new Queue<BundleDownload>();


    private string _contentRootUrl = null;
    private string _assetDataEnv = null;
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
    public async Task Initialize(IGameState gsIn, CancellationToken token)
    {
        if (!AppUtils.IsPlaying)
        {
            return;
        }

        IUnityGameState gs = gsIn as IUnityGameState;

        _gs = gs;
        _contentRootUrl = gs.Config.GetContentRoot();
        _assetDataEnv = gs.Config.GetAssetDataEnv();
        _worldDataEnv = gs.Config.GetWorldDataEnv();
        _assetParent = GEntityUtils.FindSingleton(AssetConstants.GlobalAssetParent, true);
        SpriteAtlasManager.atlasRequested += DummyRequestAtlas;
        SpriteAtlasManager.atlasRegistered += DummuRegisterAtlas;
        _localRepo = new BinaryFileRepository(_logService);
        AssetUtils.GetPerisistentDataPath();
        for (int i = 0; i < _maxConcurrentExistingDownloads; i++)
        {
            LoadFromExistingBundles(true, token);
        }
        for (int i = 0; i < _maxConcurrentBundleDownloads; i++)
        {
            DownloadNewBundles(token);
        }

        IncrementalClearMemoryCache(token);
        LoadLastSaveTimeFile(token);

        await Task.CompletedTask;
    }

    public bool IsDownloading(IUnityGameState g)
    {
        return 
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

    public void ClearBundleCache(CancellationToken token)
    {
        Dictionary<string,BundleCacheData> newBundleCache = new Dictionary<string, BundleCacheData>();

        foreach (string item in _bundleCache.Keys)
        {
            BundleCacheData bdata = _bundleCache[item];

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
        AssetUtils.UnloadUnusedAssets(token);
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
                        bundle.LastUsed < DateTime.UtcNow.AddSeconds(-20))
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
                        await Awaitable.NextFrameAsync(cancellationToken: token);
                    }
                }
            }
            if (removeCount > 0)
            {
                await AsyncUnloadAssets(token);
            }
        }
    }

    private async Awaitable AsyncUnloadAssets(CancellationToken token)
    {
        await AssetUtils.UnloadUnusedAssets(token);
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
        GEntity parent = parentIn as GEntity;
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
                        if (categoryPath.IndexOf(AssetCategoryNames.Sprites) == 0)
                        {
                            string loadPath = fullPath;
                            if (fullPath.LastIndexOf(".png") != fullPath.Length-4)
                            {
                                loadPath = fullPath + ".png"; 
                            }
                            Sprite spriteAsset = AssetDatabase.LoadAssetAtPath<Sprite>(loadPath);

                            if (spriteAsset != null)
                            {
                                handler(spriteAsset, data, token);
                            }
                            else
                            {
                                _failedAssetPathLoads.Add(assetName);
                            }
                            return;
                        }
                        else
                        {
                            asset = AssetDatabase.LoadAssetAtPath<GEntity>(fullPath + AssetConstants.ArtFileSuffix);
                        }
                    }
                    else
                    {
                        asset = AssetDatabase.LoadAssetAtPath<GEntity>(fullPath);
                    }
                    if (asset != null)
                    {
                        asset = InstantiateIntoParent(asset, parent);

                        GEntityUtils.InitializeHierarchy(_gs, asset as GEntity);

                        if (handler != null)
                        {
                            handler(asset, data, token);
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

        BundleDownload currentBundleDownload = null;
        currentBundleDownload = new BundleDownload();
        currentBundleDownload.bundleName = bundleName;
        currentBundleDownload.assetName = assetName;
        currentBundleDownload.handler = handler;
        currentBundleDownload.data = data;
        currentBundleDownload.parent = parent;

        if (_bundleCache.ContainsKey(bundleName))
        {
            _bundleCache[bundleName].LastUsed = DateTime.UtcNow;
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

    public void SetWorldAssetEnv(string worldAssetEnv)
    {
        _urlPrefixes.Remove(true);
        _worldDataEnv = worldAssetEnv;
        GetContentRootURL(true);
    }

    private Dictionary<bool, string> _urlPrefixes = new Dictionary<bool, string>();
    public string GetContentRootURL(bool worldData)
    {
        if (_urlPrefixes.TryGetValue(worldData, out string prefix))
        {
            return prefix;
        }

        string newUrl = _contentRootUrl + (worldData ? _worldDataEnv : _assetDataEnv) + "/";

        _urlPrefixes[worldData] = newUrl;

        return newUrl;
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
                await LoadAssetFromExistingBundle(_existingDownloads.Dequeue(), token);
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
        if (bdl == null || !_bundleCache.ContainsKey(bdl.bundleName))
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

    private void AddBundleToCache(BundleDownload bad, AssetBundle downloadedBundle)
    {
        if (bad == null || downloadedBundle == null || _bundleCache.ContainsKey(bad.bundleName)) return;
        
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
    public void LoadAtlasSpriteInto(string atlasName, string spriteName, object parentObject, CancellationToken token)
    {
        LoadAtlasSprite(atlasName, spriteName, null, parentObject, token);
    }


    public GEntity _assetParent = null;
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

        if (_assetParent == null)
        {
            _assetParent = GEntityUtils.FindSingleton(AssetConstants.GlobalAssetParent, true);
        }

        LoadAssetInto(_assetParent, AssetCategoryNames.Atlas, atlasName, OnDownloadAtlas, atlasDownload, token);

    }

    private void OnDownloadAtlas(object obj, object data, CancellationToken token)
    {
        AtlasSpriteDownload atlasSpriteDownload = data as AtlasSpriteDownload;
        GEntity go = obj as GEntity;

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
            GEntityUtils.Destroy(go);

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
            GEntityUtils.Destroy(go);
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

    private Dictionary<string, CachedSpriteTexture> _spriteCache = new Dictionary<string, CachedSpriteTexture>();
    public void LoadSpriteInto(string spriteName, GImage parent, CancellationToken token)
    {
        if (parent == null)
        {
            return;
        }

        string finalSpriteName = spriteName.Replace("/", "");
        if (_spriteCache.ContainsKey(finalSpriteName))
        {     
            SpriteWithImage swi = new SpriteWithImage()
            {
                Cache = _spriteCache[finalSpriteName],
                Image = parent,
            };
            OnLoadRawSprite(_spriteCache[finalSpriteName].CurrSprite, swi, token);
        }
        else
        {
            SpriteWithImage swi = new SpriteWithImage()
            {
                Cache = new CachedSpriteTexture() { SpriteName = finalSpriteName },
                Image = parent,
            };
            LoadAsset(AssetCategoryNames.Sprites, spriteName, OnLoadRawSprite, swi, null, token);
        }

    }

    protected void OnLoadRawSprite(object obj, object data, CancellationToken token)
    {

        SpriteWithImage swi = data as SpriteWithImage;

        if (swi == null || swi.Image == null)
        {
            return;
        }

        Sprite currSprite = swi.Cache.CurrSprite;
        Texture2D justDownloadedTex = obj as Texture2D;

        Sprite finalSprite = null;

        if (currSprite == null)
        {
            if (justDownloadedTex == null)
            {
                return;
            }
            else
            {
                finalSprite = Sprite.Create(justDownloadedTex, new Rect(0, 0, justDownloadedTex.width, justDownloadedTex.height), Vector2.zero);
                swi.Cache.CurrSprite = finalSprite;
                finalSprite.name = swi.Cache.SpriteName;
            }
        }
        else
        {
            finalSprite = currSprite;
        }

        CachedSpriteTexture nameCache = swi.Cache;
        if (nameCache == null)
        {
            return;
        }

        if (_spriteCache.ContainsKey(nameCache.SpriteName) &&
            _spriteCache[nameCache.SpriteName] != nameCache)
        {
            nameCache = _spriteCache[nameCache.SpriteName];
        }
        else
        {
            _spriteCache[nameCache.SpriteName] = nameCache;
        }
        
        if (swi.Image.sprite != null)
        {
            Sprite oldSprite = swi.Image.sprite;

            if (finalSprite != oldSprite)
            {
                if (_spriteCache.TryGetValue(oldSprite.name, out CachedSpriteTexture oldTexCache))
                {
                    if (_spriteCache.Keys.Count > 50 && oldTexCache.Count == 0)
                    {
                        List<CachedSpriteTexture> oldTextures = _spriteCache.Values.Where(x=>x.Count == 0 && x.LastTimeUsed < DateTime.UtcNow.AddSeconds(-5)).ToList();

                        foreach (CachedSpriteTexture oldTexture in oldTextures)
                        { 
                            _spriteCache.Remove(oldTexture.SpriteName);
                            Resources.UnloadAsset(oldSprite.texture);
                        }
                    }
                    oldTexCache.Count--;
                    oldTexCache.LastTimeUsed = DateTime.UtcNow;
                }
            }
            else
            {
                return;
            }
        }

        nameCache.Count++;

        swi.Image.sprite = finalSprite;

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
        string path = AssetUtils.GetRuntimePrefix() + AssetConstants.BundleUpdateFile;
        DownloadFileData ddata = new DownloadFileData() { ForceDownload = true, Handler = OnDownloadLastSaveTimeText, IsText = true };
        _fileDownloadService.DownloadFile(path, ddata, false, token);
    }

    private void OnDownloadLastSaveTimeText(object obj, object data, CancellationToken token)
    {
        _bundleUpdateInfo = SerializationUtils.TryDeserialize<BundleUpdateInfo>(obj);

        LoadAssetBundleList(token);
    }

    void LoadAssetBundleList(CancellationToken token)
    {
        _bundleVersions = _localRepo.LoadObject<BundleVersions>(AssetConstants.BundleVersionsFile);

        DownloadFileData ddata = new DownloadFileData() { ForceDownload = true, Handler = OnDownloadBundleVersions, IsText = true };

        if (_bundleVersions == null || _bundleVersions.UpdateInfo == null ||
            _bundleUpdateInfo == null ||
            _bundleVersions.UpdateInfo.ClientVersion != _bundleUpdateInfo.ClientVersion ||
            _bundleVersions.UpdateInfo.UpdateTime != _bundleUpdateInfo.UpdateTime)
        {
            string path = AssetUtils.GetRuntimePrefix() + AssetConstants.BundleVersionsFile;
            _fileDownloadService.DownloadFile(path, ddata, false, token);
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
            _bundleVersions = newVersions;
            _localRepo.SaveObject(AssetConstants.BundleVersionsFile, _bundleVersions);
        }
    }

    protected string GetFullBundleURL(string bundleName)
    {
        return GetContentRootURL(false) + AssetUtils.GetRuntimePrefix() + bundleName + "_" + GetBundleHash(bundleName);
    }

    private async Awaitable DownloadOneBundle(BundleDownload bad, CancellationToken token)
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
                    AddBundleToCache(bad, downloadedBundle);

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


    protected object InstantiateBundledAsset(object child, GEntity parent, string bundleName, string assetName)
    {
        BundleCacheData bundleCache = _bundleCache[bundleName];
        if (child is Texture2D tex2d)
        {
            bundleCache.Instances.Add(tex2d);
            return tex2d;
        }

        GEntity go = InstantiateIntoParent(child, parent);
        if (go != null)
        {
            BundleCacheItem cacheItem = go.AddComponent<BundleCacheItem>();
            cacheItem.RegisterCache(bundleCache);
            bundleCache.Instances.Add(go);
        }
        else
        {
            _logService.Error("Failed to load " + assetName + " from " + bundleName);
            return null;
        }
        BaseBehaviour oneBehavior = go.GetComponent<BaseBehaviour>();
        if (oneBehavior != null)
        {
            GEntityUtils.InitializeHierarchy(_gs, go);
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


    private GEntity InstantiateIntoParent(object child, GEntity parent)
    {
        GEntity go = child as GEntity;
        if (go == null)
        {
            return null;
        }
        go = GEntity.Instantiate<GEntity>(go);

        go.name = go.name.Replace("(Clone)", "");
        go.name = go.name.Replace(AssetConstants.ArtFileSuffix, "");

        if (parent != null)
        {
            GEntityUtils.AddToParent(go, parent);
        }
        return go;
    }
    
    public async Awaitable<GEntity> LoadAssetAsync(string assetCategory, string assetPath, object parent, CancellationToken token, string subdirectory = null)
    {
        GEntityContainer cont = new GEntityContainer();
        LoadAsset(assetCategory, assetPath, OnLoadEntityAsync, cont, null, token, subdirectory);

        while (cont.Entity == null && !cont.FailedLoad)
        {
            await Awaitable.NextFrameAsync(token);
        }

        return cont.Entity;
    }

    private void OnLoadEntityAsync(object obj, object data, CancellationToken token)
    {
        GEntityContainer cont = data as GEntityContainer;

        if (cont == null)
        {
            return;
        }

        cont.Entity = obj as GEntity;

        if (cont.Entity == null)
        {
            cont.FailedLoad = true;
        }

    }

}

