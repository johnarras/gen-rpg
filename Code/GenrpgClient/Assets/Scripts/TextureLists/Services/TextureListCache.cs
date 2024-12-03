using Assets.Scripts.GameObjects;
using Genrpg.Shared.Client.Assets.Constants;
using Genrpg.Shared.Client.Assets.Services;
using Genrpg.Shared.Crawler.TextureLists.Services;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.TextureLists.Services
{

    public class DownloadTextureListData
    {
        public object Data;
        public DownloadTextureListHandler Handler;
        public string TextureName;
        public TextureList TextureList;
    }

    public class TextureListCache : ITextureListCache
    {

        private IAssetService _assetService;
        private ISingletonContainer _singletonContainer;
        private IClientEntityService _clientEntityService;

        private GameObject _textureListParent;

        private Dictionary<string, TextureList> _textureListCache = new Dictionary<string, TextureList>();

        public async Task Initialize(CancellationToken token)
        {
            await Task.CompletedTask;
        }

        private GameObject GetTextureListParent()
        {
            if (_textureListParent == null)
            {
                _textureListParent = _singletonContainer.GetSingleton("TextureListParent");
            }
            return _textureListParent;
        }

        public void LoadTextureList(string textureName, DownloadTextureListHandler handler, object data, CancellationToken token)
        {

            DownloadTextureListData downloadData = new DownloadTextureListData()
            {
                Handler = handler,
                Data = data, 
                TextureName = textureName    
            };

            _assetService.LoadAssetInto(GetTextureListParent(), AssetCategoryNames.TextureLists, textureName, OnDownloadTextureList, downloadData, token);
        }

        public async Task OnCleanup(CancellationToken token)
        {
            _clientEntityService.DestroyAllChildren(GetTextureListParent());
            _textureListCache = new Dictionary<string, TextureList>();
            await Task.CompletedTask;
        }

        private void OnDownloadTextureList (object obj, object data, CancellationToken token)
        {

            GameObject go = obj as GameObject;

            if (go == null)
            {
                return;
            }

            DownloadTextureListData downloadData = data as DownloadTextureListData;

            if (downloadData == null || string.IsNullOrEmpty(downloadData.TextureName))
            {
                _clientEntityService.Destroy(go);
                return;
            }

            if (_textureListCache.TryGetValue(downloadData.TextureName, out TextureList texList))
            {
                _clientEntityService.Destroy(go);
            }
            else
            {
                texList = go.GetComponent<TextureList>();
                if (texList == null)
                {
                    _clientEntityService.Destroy(go);
                    return;
                }
                _clientEntityService.SetActive(go, false);
                _textureListCache[downloadData.TextureName] = texList;
            }

            downloadData.TextureList = texList;
            if (downloadData.Handler != null)
            {
                downloadData.Handler(texList, downloadData);
            }

        }
    }
}
