
using Genrpg.Shared.Interfaces;
using System.Collections.Generic;
using System.Threading;

public interface IAssetService : IService
{

    void Init(UnityGameState gs, string assetURLPrefix, string contentDataEnv, string worldDataEnv, CancellationToken token);
    bool IsInitialized(UnityGameState gs);
    void LoadAssetInto(UnityGameState gs, object parent, string assetCategory, string assetPath, OnDownloadHandler handler, object data, CancellationToken token, string subdirectory=null);
    void LoadAsset(UnityGameState gs, string assetCategory, string assetPath, OnDownloadHandler handler, object data, object parent, CancellationToken token,string subdirectory=null);
    void LoadSprite(UnityGameState gs, string atlasName, string spriteName, OnDownloadHandler handler, object data, CancellationToken token);
    void LoadSpriteInto(UnityGameState gs, string atlasName, string spriteName, object parentSprite, CancellationToken token);
    void GetSpriteList(UnityGameState gs, string atlasName, SpriteListDelegate onLoad, CancellationToken token);

    void DownloadFile(UnityGameState gs, string url, DownloadData data, bool worldData, CancellationToken token);
    void ClearBundleCache(UnityGameState gs, CancellationToken token);
    string GetBundleNameForCategoryAndAsset(UnityGameState gs, string assetCategory, string assetPath);

    ClientAssetCounts GetAssetCounts();

    string StripPathPrefix(string path);
    void SetWorldAssetEnv(string worldAssetEnv);

    bool IsDownloading(UnityGameState gs);

    string GetWorldDataEnv();
    string GetContentDataEnv();

}
