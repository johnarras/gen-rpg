
using Genrpg.Shared.Interfaces;
using System.Collections.Generic;
using System.Threading;

public interface IAssetService : IService
{

    void Init(UnityGameState gs, CancellationToken token);
    bool IsInitialized(UnityGameState gs);
    void LoadAssetInto(UnityGameState gs, object parent, string assetCategory, string assetPath, OnDownloadHandler handler, object data, CancellationToken token);
    void LoadAsset(UnityGameState gs, string assetCategory, string assetPath, OnDownloadHandler handler, object data, object parent, CancellationToken token);
        
    void LoadSprite(UnityGameState gs, string atlasName, string spriteName, OnDownloadHandler handler, object data, CancellationToken token);
    void LoadSpriteInto(UnityGameState gs, string atlasName, string spriteName, object parentSprite, CancellationToken token);
    void GetSpriteList(UnityGameState gs, string atlasName, SpriteListDelegate onLoad, CancellationToken token);

    void DownloadFile(UnityGameState gs, string url, DownloadData data, CancellationToken token);
    string GetArtURLPrefix(UnityGameState gs);
    void ClearBundleCache(UnityGameState gs, CancellationToken token);
    string GetBundleNameForCategoryAndAsset(UnityGameState gs, string assetCategory, string assetPath);

    string StripPathPrefix(string path);

    bool IsDownloading(UnityGameState gs);

}
