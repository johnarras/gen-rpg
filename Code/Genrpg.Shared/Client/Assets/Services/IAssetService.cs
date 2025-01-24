using Genrpg.Shared.Core.Interfaces;
using Genrpg.Shared.DataStores.DataGroups;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.MVC.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.Shared.Client.Assets.Services
{

    public delegate void OnDownloadHandler(object obj, object data, CancellationToken token);

    public delegate void SpriteListDelegate(object[] sprites);

    public class ClientAssetCounts
    {
        public long BundlesLoaded = 0;
        public long BundlesUnloaded = 0;
        public long ObjectsLoaded = 0;
        public long ObjectsUnloaded = 0;

    }
    public interface IAssetService : IInitializable, IGameCleanup
    {
        bool IsInitialized();
        void LoadAssetInto(object parent, string assetCategory, string assetPath, OnDownloadHandler handler, object data, CancellationToken token, string subdirectory = null);
        void LoadAsset(string assetCategory, string assetPath, OnDownloadHandler handler, object data, object parent, CancellationToken token, string subdirectory = null);
        void LoadAtlasSpriteInto(string atlasName, string spriteName, object parentObject, CancellationToken token);
        void LoadSpriteWithAtlasNameInto(string atlasSlashSpriteName, object parentObject, CancellationToken token);
        Task<T> LoadAssetAsync<T>(string assetCategory, string assetPath, object parent, CancellationToken token, string subdirectory = null) where T : class;
        Task<object> LoadAssetAsync(string assetCategory, string assetPath, object parent, CancellationToken token, string subdirectory = null);
        void GetSpriteList(string atlasName, SpriteListDelegate onLoad, CancellationToken token);
        Task ClearBundleCache(CancellationToken token);
        string GetBundleNameForCategoryAndAsset(string assetCategory, string assetPath);
        ClientAssetCounts GetAssetCounts();
        string StripPathPrefix(string path);
        void SetWorldAssetEnv(string worldAssetEnv);
        string GetContentRootURL(EDataCategories category);
        bool IsDownloading();
        string GetWorldDataEnv();
        void UnloadAsset(object obj);
        List<T> LoadAllResources<T>(string path);
        string GetAssetPath(string assetCategoryName);
        Task<VC> CreateAsync<VC, TModel>(TModel model, string assetCategoryName, string assetPath, object parent, CancellationToken token, string subdirectory = null) where VC : class, IViewController<TModel, IView>, new();
        void Create<VC, TModel>(TModel model, string assetCategoryName, string assetPath, object parent, Action<VC,CancellationToken> onLoadHandler, CancellationToken token, string subdirectory = null) where VC : class, IViewController<TModel, IView>, new();
        Task<VC> InitViewController<VC, TModel>(TModel model, object viewObj, object parent, CancellationToken token) where VC : class, IViewController<TModel, IView>, new();
       
    }
}
