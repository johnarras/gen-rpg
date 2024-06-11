
using GObject = UnityEngine.Object;
using UnityEngine; // Needed

using System.Threading;
using Scripts.Assets.Assets.Constants;

public class AssetUtils
{

    public static T LoadResource<T>(string path) where T : GObject
    {
        return Resources.Load<T>(path);
    }

    public static T[] LoadAllResources<T>(string path) where T : GObject
    {
        return Resources.LoadAll<T>(path);
    }

    public static void UnloadAsset<T>(T obj) where T : GObject
    {
        Resources.UnloadAsset(obj);
    }

    private static bool _unloadingAssets = false;
    public static async Awaitable UnloadUnusedAssets(CancellationToken token)
    {
        if (_unloadingAssets)
        {
            return;
        }
        _unloadingAssets = true;
        AsyncOperation op = Resources.UnloadUnusedAssets();
        while (!op.isDone)
        {
            await Awaitable.NextFrameAsync(cancellationToken: token);
        }
        _unloadingAssets = false;
    }

    public static string GetAssetPath(string assetCategoryName)
    {
        return assetCategoryName + "/";
    }

    private static string _persistentDataPath = null;
    public static string GetPerisistentDataPath()
    {
        if (string.IsNullOrEmpty(_persistentDataPath))
        {
            _persistentDataPath = AppUtils.PersistentDataPath;
        }
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

        string prefix = GetPlatformString();
        _runtimePrefix = AppUtils.Version + "/" + prefix + "/";
        return _runtimePrefix;
    }

}
