

using Genrpg.Shared.DataStores.Categories;
using Genrpg.Shared.DataStores.DataGroups;
using Genrpg.Shared.Interfaces;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public interface IAssetService : IInitializable
{
    bool IsInitialized();
    void LoadAssetInto(object parent, string assetCategory, string assetPath, OnDownloadHandler handler, object data, CancellationToken token, string subdirectory=null);
    void LoadAsset(string assetCategory, string assetPath, OnDownloadHandler handler, object data, object parent, CancellationToken token,string subdirectory=null);
    void LoadAtlasSpriteInto(string atlasName, string spriteName, object parentObject, CancellationToken token);
    void LoadSpriteInto(string spriteName, GImage parentSprite, CancellationToken token);
    Awaitable<GameObject> LoadAssetAsync(string assetCategory, string assetPath, object parent, CancellationToken token, string subdirectory = null);


    void GetSpriteList(string atlasName, SpriteListDelegate onLoad, CancellationToken token);

    void ClearBundleCache(CancellationToken token);
    string GetBundleNameForCategoryAndAsset(string assetCategory, string assetPath);

    ClientAssetCounts GetAssetCounts();

    string StripPathPrefix(string path);
    void SetWorldAssetEnv(string worldAssetEnv);

    string GetContentRootURL(EDataCategories category);

    bool IsDownloading();

    string GetWorldDataEnv();

}
