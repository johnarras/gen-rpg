﻿
using GObject = UnityEngine.Object;
using UnityEngine; // Needed
using Cysharp.Threading.Tasks;
using System.Threading;

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
    public static async UniTask UnloadUnusedAssets(CancellationToken token)
    {
        if (_unloadingAssets)
        {
            return;
        }
        _unloadingAssets = true;
        AsyncOperation op = Resources.UnloadUnusedAssets();
        while (!op.isDone)
        {
            await UniTask.NextFrame( cancellationToken: token);
        }
        _unloadingAssets = false;
    }
}
