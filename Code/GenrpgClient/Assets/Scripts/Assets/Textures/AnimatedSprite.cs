using Assets.Scripts.TextureLists.Services;
using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Crawler.Maps.Services;
using Genrpg.Shared.Crawler.TextureLists.Services;
using Genrpg.Shared.UI.Interfaces;
using Microsoft.SqlServer.Server;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.SearchService;

namespace Assets.Scripts.Assets.Textures
{
    public class AnimatedSprite : BaseBehaviour
    {

        private ITextureListCache _textureListCache;
        private ICrawlerMapService _crawlerMapService;

        private SpriteList _textureList;

        public GImage AnimatedImage;


        const int FramesBetweenIncrement = 20;

        private string _currentSpriteName = null;
        private string _newSpriteName = null;

        private int currIncrementFrame = 0;

        private int _currentTextureFrame = 0;

        public override void Init()
        {
            _clientEntityService.SetActive(AnimatedImage, false);
            _updateService.AddUpdate(this, LateUpdatePicture, UpdateType.Late, GetToken());
        }

        public void SetImage(string spriteName)
        {

            if (spriteName == _currentSpriteName)
            {
                return;
            }

            _newSpriteName = spriteName;

        }

        private void OnLoadTextureList(object textureList, object data)
        {

            if (data is DownloadTextureListData downloadData)
            {
                if (_currentSpriteName == downloadData.TextureName)
                {
                    return;
                }
                _currentSpriteName = downloadData.TextureName;
                _textureList = downloadData.TextureList;
                ShowTexture(0);
            }
        }


        private void LateUpdatePicture()
        {
            currIncrementFrame++;
            if (currIncrementFrame % FramesBetweenIncrement == 0)
            {
                _currentTextureFrame++;
                ShowTexture(_currentTextureFrame);
            }

            string spriteName = _newSpriteName;
            if (_newSpriteName == _currentSpriteName)
            {
                return;
            }
            if (!string.IsNullOrEmpty(spriteName) && spriteName.IndexOf("Building") >= 0)
            {
                spriteName = _crawlerMapService.GetBuildingArtPrefix() + spriteName;
            }

            if (string.IsNullOrEmpty(spriteName))
            {
                _textureList = null;
                _currentSpriteName = spriteName;
                ShowTexture(0);
                return;
            }
            if (_currentSpriteName == spriteName)
            {
                return;
            }
            _textureListCache.LoadTextureList(spriteName, OnLoadTextureList, spriteName, GetToken());
        }


        private void ShowTexture(int frame)
        {
            _currentTextureFrame = frame;

            if (_textureList == null || _textureList.Sprites.Count < 1)
            {
                _clientEntityService.SetActive(AnimatedImage, false);
                return;
            }

            _clientEntityService.SetActive(AnimatedImage, true);

            int currentFrame = _currentTextureFrame % _textureList.Sprites.Count;


            _uiService.SetImageSprite(AnimatedImage, _textureList.Sprites[currentFrame]);
        }
    }
}
