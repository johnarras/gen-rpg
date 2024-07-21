

using Assets.Scripts.ClientEvents;
using Assets.Scripts.Crawler.CrawlerStates;
using Assets.Scripts.Crawler.Events;
using Assets.Scripts.Crawler.Maps.Constants;
using Assets.Scripts.Crawler.Maps.Entities;
using Assets.Scripts.Crawler.Maps.Services;
using Assets.Scripts.Crawler.Services.CrawlerMaps;
using Assets.Scripts.UI.Crawler.ActionUI;
using Assets.Scripts.UI.Crawler.WorldUI;
using Assets.Scripts.UI.Services;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.UI.Interfaces;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.Shared.Utils;
using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;
using System.Threading;
using UnityEngine;
using UnityEngine.Rendering.UI;
using GEntity = UnityEngine.GameObject;

namespace Assets.Scripts.UI.Crawler.CrawlerPanels
{

    public class ShowWorldPanelImage
    {
        public string SpriteName;
    }

    public class WorldPanel : BaseCrawlerPanel, IWorldPanel
    {

        const string PanelTextPrefab = "WorldPanelText";
        private ICrawlerMapService _mapService;
        private IUIService _uiService;

        public GRawImage WorldImage;
        public GText WorldPosText;
        public GText MapNameText;

        public GImage NoMeleeImage;
        public GImage NoRangedImage;
        public GImage NoMagicImage;

        public GEntity TooltipParent;
        public GEntity TooltipContent;

        public GButton CloseTooltipButton;

       private WorldPanelText _textRow;

        public override async Awaitable Init(CrawlerScreen screen, CancellationToken token)
        {
            await base.Init(screen, token);
            _dispatcher.AddEvent<ShowWorldPanelImage>(this, OnShowWorldPanelImage);
            _dispatcher.AddEvent<CrawlerUIUpdate>(this, OnUpdateWorldUI);
            _dispatcher.AddEvent<ShowCrawlerTooltipEvent>(this, OnShowTooltip);
            _dispatcher.AddEvent<HideCrawlerTooltipEvent>(this, OnHideTooltip);

            GEntityUtils.SetActive(TooltipParent, false);

            _uiService.SetButton(CloseTooltipButton, name, () => { OnHideTooltip(new HideCrawlerTooltipEvent()); });

            _assetService.LoadAsset(AssetCategoryNames.UI, PanelTextPrefab, OnLoadTextRow, null, this, token, screen.Subdirectory);

            _updateService.AddUpdate(this, IncrementTextureFrame, UpdateType.Late);
            SetPicture(null);
        }

        private void OnLoadTextRow(object obj, object data, CancellationToken token)
        {
            GEntity entity = obj as GEntity;
            _textRow = GEntityUtils.GetComponent<WorldPanelText>(entity);
            GEntityUtils.SetActive(entity, false);
            GEntityUtils.AddToParent(entity, gameObject);
        }

        public override void OnNewStateData(CrawlerStateData stateData)
        {
            SetPicture(stateData.WorldSpriteName);
        }

        private void OnShowWorldPanelImage(ShowWorldPanelImage imageToShow)
        {
            SetPicture(imageToShow.SpriteName);
            return;
        }

        private void OnUpdateWorldUI(CrawlerUIUpdate update)
        {
            _uiService.SetText(MapNameText, update.Map.GetName(update.Party.MapX, update.Party.MapZ));

            string txt = MapUtils.DirFromAngle(update.Party.MapRot) + "(" + update.Party.MapX + "," + update.Party.MapZ + ")";

            _uiService.SetText(WorldPosText, txt);

            byte disables = update.Map.Get(update.Party.MapX, update.Party.MapZ, CellIndex.Disables);

            GEntityUtils.SetActive(NoMeleeImage, FlagUtils.IsSet(disables, MapDisables.NoMelee));
            GEntityUtils.SetActive(NoRangedImage, FlagUtils.IsSet(disables, MapDisables.NoRanged));
            GEntityUtils.SetActive(NoMagicImage, FlagUtils.IsSet(disables, MapDisables.NoMagic));

        }

        private void OnShowTooltip(ShowCrawlerTooltipEvent showEvent)
        {        
            if (showEvent.Lines.Count < 1)
            {
                return;
            }

            GEntityUtils.DestroyAllChildren(TooltipContent);
            GEntityUtils.SetActive(TooltipParent, true);

            for (int i = 0; i < showEvent.Lines.Count; i++)
            {
                WorldPanelText text = _gameObjectService.FullInstantiate<WorldPanelText>(_textRow);
                GEntityUtils.AddToParent(text.gameObject, TooltipContent);
                text.Init(showEvent.Lines[i], _token);
            }
        }

        private void OnHideTooltip(HideCrawlerTooltipEvent hideEvent)
        {
            GEntityUtils.SetActive(TooltipParent, false);
        }


        private string _currentSpriteName = null;
        private Dictionary<string, TextureList> _cachedSprites = new Dictionary<string, TextureList>();
        public void SetPicture(string spriteName)
        {
            if (!string.IsNullOrEmpty(spriteName) && spriteName.IndexOf("Building") >= 0)
            {
                spriteName = _mapService.GetBuildingArtPrefix() + spriteName;
            }

            if (WorldImage == null)
            {
                return;
            }
            if (string.IsNullOrEmpty(spriteName))
            {
                WorldImage.texture = null;
                GEntityUtils.SetActive(WorldImage, false);
                _currentSpriteName = spriteName;
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

            _assetService.LoadAsset(AssetCategoryNames.TextureLists, spriteName, OnDownloadAtlas, spriteName, null, _token); 
        }
        public void ApplyEffect(string effectName, float duration)
        {
        }

        private void OnDownloadAtlas(object obj, object data, CancellationToken token)
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
