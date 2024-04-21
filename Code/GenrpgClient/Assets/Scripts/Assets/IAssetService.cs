
using Cysharp.Threading.Tasks;
using Genrpg.Shared.Interfaces;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public interface IAssetService : IInitializable
{
    bool IsInitialized(UnityGameState gs);
    void LoadAssetInto(UnityGameState gs, object parent, string assetCategory, string assetPath, OnDownloadHandler handler, object data, CancellationToken token, string subdirectory=null);
    void LoadAsset(UnityGameState gs, string assetCategory, string assetPath, OnDownloadHandler handler, object data, object parent, CancellationToken token,string subdirectory=null);
    void LoadAtlasSpriteInto(UnityGameState gs, string atlasName, string spriteName, object parentObject, CancellationToken token);
    void LoadSpriteInto(UnityGameState gs, string spriteName, GImage parentSprite, CancellationToken token);
    UniTask<GameObject> LoadAssetAsync(UnityGameState gs, string assetCategory, string assetPath, object parent, CancellationToken token, string subdirectory = null);


    void GetSpriteList(UnityGameState gs, string atlasName, SpriteListDelegate onLoad, CancellationToken token);

    void ClearBundleCache(UnityGameState gs, CancellationToken token);
    string GetBundleNameForCategoryAndAsset(UnityGameState gs, string assetCategory, string assetPath);

    ClientAssetCounts GetAssetCounts();

    string StripPathPrefix(string path);
    void SetWorldAssetEnv(string worldAssetEnv);

    string GetContentRootURL(bool worldData);

    bool IsDownloading(UnityGameState gs);

    string GetWorldDataEnv();

}
