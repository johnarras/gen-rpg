using Assets.Scripts.Crawler.Combat;
using Assets.Scripts.TextureLists.Services;
using Genrpg.Shared.Crawler.Maps.Services;
using Genrpg.Shared.Crawler.TextureLists.Services;
using Genrpg.Shared.Utils;

namespace Assets.Scripts.Assets.Textures
{
    public class AnimatedSprite : BaseBehaviour
    {

        private ITextureListCache _textureListCache;
        private ICrawlerMapService _crawlerMapService;

        private SpriteList _textureList;

        public GImage AnimatedImage;

        public bool OnlyShowFirstFrame = false;


        public bool ShowSequence = false;
        public int FramesBetweenSequenceStep = 2;

        const float ChangeToBaseFrameChance = 0.2f;
        const float ChangeToOtherFrameChance = 0.05f;

        private string _currentSpriteName = null;
        private string _newSpriteName = null;

        private int _currentImageFrame = 0;
        private int _ticksBetweenFrameUpdate = 0;

        public override void Init()
        {
            _clientEntityService.SetActive(AnimatedImage, false);
            _updateService.AddUpdate(this, LateUpdatePicture, UpdateTypes.Late, GetToken());
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
                ShowTextureFrame(0);
            }
        }


        private void LateUpdatePicture()
        {

            string spriteName = _newSpriteName;
            if (_newSpriteName != _currentSpriteName)
            {
                if (!string.IsNullOrEmpty(spriteName) && spriteName.IndexOf("Building") >= 0)
                {
                    spriteName = _crawlerMapService.GetBuildingArtPrefix() + spriteName;
                }

                if (string.IsNullOrEmpty(spriteName))
                {
                    _textureList = null;
                    _currentSpriteName = spriteName;
                    ShowTextureFrame(0);
                    return;
                }
                if (_currentSpriteName == spriteName)
                {
                    return;
                }
                _textureListCache.LoadTextureList(spriteName, OnLoadTextureList, spriteName, GetToken());
                return;
            }

            if (_textureList == null || _textureList.Sprites.Count < 1)
            {
                _clientEntityService.SetActive(AnimatedImage, false);
                return;
            }

            if (_textureList.Sprites.Count == 1)
            {
                return;
            }

            if (!ShowSequence)
            {
                if (!OnlyShowFirstFrame)
                {
                    if (_currentImageFrame > 0 && _rand.NextDouble() < ChangeToBaseFrameChance)
                    {
                        ShowTextureFrame(0);
                        return;
                    }

                    if (_currentImageFrame == 0 && _rand.NextDouble() < ChangeToBaseFrameChance)
                    {
                        ShowTextureFrame(MathUtils.IntRange(1, _textureList.Sprites.Count - 1, _rand));
                        return;
                    }
                }
            }
           else
            {
                if (OnlyShowFirstFrame)
                {
                    if (_currentImageFrame > 0)
                    {  
                        ShowTextureFrame(0);
                    }
                    return;
                }

                _ticksBetweenFrameUpdate++;
                if (_ticksBetweenFrameUpdate >= FramesBetweenSequenceStep)
                {
                    _currentImageFrame++;
                    if (_currentImageFrame >= _textureList.Sprites.Count)
                    {
                        _currentImageFrame = 0;
                    }
                    ShowTextureFrame(_currentImageFrame);
                    _ticksBetweenFrameUpdate = 0;
                }
            }

        }


        private void ShowTextureFrame(int frame)
        {
            if (_textureList == null || _textureList.Sprites.Count < 1)
            {
                _clientEntityService.SetActive(AnimatedImage, false);
                return;
            }

            _clientEntityService.SetActive(AnimatedImage, true);

            if (_textureList.Sprites.Count > frame)
            {
                _uiService.SetImageSprite(AnimatedImage, _textureList.Sprites[frame]);
            }
            _currentImageFrame = frame;
        }
    }
}
