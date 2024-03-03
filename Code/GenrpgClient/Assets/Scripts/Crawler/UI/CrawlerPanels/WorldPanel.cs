using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Triggers;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.UI.Interfaces;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using GEntity = UnityEngine.GameObject;

namespace Assets.Scripts.UI.Crawler.CrawlerPanels
{

    public class ShowWorldPanelImage
    {
        public string SpriteName;
    }

    public class WorldPanel : BaseCrawlerPanel, IWorldPanel
    {
        public GRawImage WorldImage;

        public override async UniTask Init(CrawlerScreen screen, CancellationToken token)
        {
            await base.Init(screen, token);
            _gs.AddEvent<ShowWorldPanelImage>(this, OnShowWorldPanelImage);

            _updateService.AddUpdate(this, IncrementTextureFrame, UpdateType.Late);
            SetPicture(null);
        }

        public override void OnNewStateData(CrawlerStateData stateData)
        {
            SetPicture(stateData.WorldSpriteName);
        }

        private ShowWorldPanelImage OnShowWorldPanelImage(UnityGameState gs, ShowWorldPanelImage imageToShow)
        {
            SetPicture(imageToShow.SpriteName);
            return null;
        }



        private string _currentSpriteName = null;
        private Dictionary<string, TextureList> _cachedSprites = new Dictionary<string, TextureList>();
        public void SetPicture(string spriteName)
        {
            if (WorldImage == null)
            {
                return;
            }
            if (string.IsNullOrEmpty(spriteName))
            {
                WorldImage.texture = null;
                GEntityUtils.SetActive(WorldImage, false);
                return;
            }
            if (_currentSpriteName == spriteName)
            {
                return;
            }

            _currentSpriteName = spriteName;
         
            if (_cachedSprites.TryGetValue(spriteName, out TextureList textureList))
            {
                if (textureList.Textures.Count > 0 && textureList.Textures[0] != null)
                {
                    GEntityUtils.SetActive(WorldImage, true);
                    WorldImage.texture = textureList.Textures[0];
                    SetTextureFrame(0);
                }
                return;
            }

            _assetService.LoadAsset(_gs, AssetCategoryNames.TextureLists, spriteName, OnDownloadAtlas, spriteName, null, _token); 
        }
        public void ApplyEffect(string effectName, float duration)
        {
        }

        private void OnDownloadAtlas(UnityGameState gs, object obj, object data, CancellationToken token)
        {
            GEntity go = obj as GEntity;

            if (go == null)
            {
                return;
            }

            string spriteName = data as string;

            if (spriteName == null)
            {
                GEntityUtils.Destroy(go);
                return;
            }

            if (_cachedSprites.TryGetValue(spriteName, out TextureList texList))
            {
                GEntityUtils.Destroy(go);
            }
            else
            {
                texList = go.GetComponent<TextureList>();
                if (texList == null)
                {
                    GEntityUtils.Destroy(go);
                    return;
                }
                GEntityUtils.SetActive(go, false);
                GEntityUtils.AddToParent(go, gameObject);
                _cachedSprites[spriteName] = texList;
            }
            GEntityUtils.SetActive(WorldImage, true);
            WorldImage.texture = texList.Textures[0];
            SetTextureFrame(0);
        }

        const int FramesBetweenIncrement = 20;
        int currIncrementFrame = 0;
        int _currentTextureFrame = 0;
        private void IncrementTextureFrame()
        {
            currIncrementFrame++;
            if (currIncrementFrame % FramesBetweenIncrement == 0)
            {
                _currentTextureFrame++;
                ShowTexture();
            }
        }

        private void SetTextureFrame(int frame)
        {
            _currentTextureFrame = 0;
            ShowTexture();
        }

        private void ShowTexture()
        {
            if (WorldImage.texture == null)
            {
                return;
            }

            int height = WorldImage.texture.height;
            int width = WorldImage.texture.width;

            int frameCount = 1;

            if (width > height)
            {
                frameCount = width / height;
            }

            int currFrame = _currentTextureFrame % frameCount;

            float xmin = 1.0f * currFrame / frameCount;
            float xmax = 1.0f * (currFrame + 1) / frameCount;
            WorldImage.uvRect = new UnityEngine.Rect(new Vector2(xmin, 0), new Vector2(1.0f / frameCount, 1));
        }
    }
}
