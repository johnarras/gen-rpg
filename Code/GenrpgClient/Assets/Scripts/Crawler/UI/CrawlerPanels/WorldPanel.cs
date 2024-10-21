

using Assets.Scripts.ClientEvents;
using Genrpg.Shared.Crawler.Maps.Constants;
using Genrpg.Shared.Crawler.Maps.Entities;
using Assets.Scripts.Crawler.Tilemaps;
using Assets.Scripts.Crawler.UI.WorldUI;
using Assets.Scripts.ProcGen.Components;
using Assets.Scripts.ProcGen.Services;
using Assets.Scripts.UI.Crawler.WorldUI;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.UI.Interfaces;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Threading;
using Genrpg.Shared.Crawler.GameEvents;
using Genrpg.Shared.Crawler.Maps.Services;
using Genrpg.Shared.Client.Assets.Constants;
using Genrpg.Shared.MVC.Interfaces;
using System.Threading.Tasks;
using Genrpg.Shared.UI.Interfaces;
using UnityEngine;

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
        private ICurveGenService _curveGenService;
        private ICrawlerWorldService _worldService;
            

        private IRawImage _worldImage;
        private IText _worldPosText;
        private IText _mapNameText;
        private IText _timeOfDayText;

        private IImage _noMeleeImage;
        private IImage _noRangedImage;
        private IImage _noMagicImage;

        private object _tooltipParent;
        private object _tooltipContent;
        private object _panelButtonsParent;

        private IButton _closeTooltipButton;

        private object _root;

        private CrawlerTilemap _minimap;

        private IView _textRow;

        private WorldPanelButtons _panelButtons;

        public override async Task Init(CrawlerScreen screen, IView view, CancellationToken token)
        {
            await base.Init(screen, view, token);
            AddListener<ShowWorldPanelImage>(OnShowWorldPanelImage);
            AddListener<CrawlerUIUpdate>(OnUpdateWorldUI);
            AddListener<ShowCrawlerTooltipEvent>(OnShowTooltip);
            AddListener<HideCrawlerTooltipEvent>(OnHideTooltip);

            _worldImage = _view.Get<IRawImage>("WorldImage");
            _worldPosText = _view.Get<IText>("WorldPosText");
            _timeOfDayText = _view.Get<IText>("TimeOfDayText");
            _mapNameText = _view.Get<IText>("MapNameText");
            _panelButtonsParent = _view.Get<object>("PanelButtonsParent");
            _noMeleeImage = _view.Get<IImage>("NoMeleeImage");
            _noRangedImage = _view.Get<IImage>("NoRangedImage");
            _noMagicImage = _view.Get<IImage>("NoMagicImage");
            _minimap = _view.Get<CrawlerTilemap>("Minimap");
            _gs.loc.Resolve(_minimap);
            _tooltipParent = _view.Get<object>("TooltipParent");
            _tooltipContent = _view.Get<object>("TooltipContent");
            _root = _view.Get<object>("Root");

            _clientEntityService.SetActive(_tooltipParent, false);

            _uiService.SetButton(_closeTooltipButton, GetType().Name, () => { OnHideTooltip(new HideCrawlerTooltipEvent()); });

            _textRow = await _assetService.LoadAssetAsync<IView>(AssetCategoryNames.UI, PanelTextPrefab, _root, _token, _model.Subdirectory);

            _panelButtons = await _assetService.CreateAsync<WorldPanelButtons, WorldPanel>(this, AssetCategoryNames.UI, "WorldPanelButtons", _panelButtonsParent, token, screen.Subdirectory);

            AddUpdate(OnLateUpdate, UpdateType.Late);

            SetPicture(null);

        }

        public override async Task OnNewStateData(CrawlerStateData stateData, CancellationToken token)
        {
            SetPicture(stateData.WorldSpriteName);
            await Task.CompletedTask;
        }

        private void OnShowWorldPanelImage(ShowWorldPanelImage imageToShow)
        {
            SetPicture(imageToShow.SpriteName);
            return;
        }


        private CrawlerUIUpdate _update = null;
        private void OnUpdateWorldUI(CrawlerUIUpdate update)
        {
            _update = update;
        }

        private void LateUpdateWorldUI()
        { 
            if (_update == null)
            {
                return;
            }
            _update = null;
            PartyData partyData = _crawlerService.GetParty();

            TimeSpan ts = TimeSpan.FromHours(partyData.HourOfDay);

            _uiService.SetText(_timeOfDayText, ts.ToString(@"hh\:mm") + " Day " + (partyData.DaysPlayed+1));

            CrawlerMap map = _worldService.GetMap(partyData.MapId);

            if (map == null)
            {
                return;
            }

            _uiService.SetText(_mapNameText, map.GetName(partyData.MapX, partyData.MapZ));
            string txt = MapUtils.DirFromAngle(partyData.MapRot) + "(" + partyData.MapX + "," + partyData.MapZ + ")";

            _uiService.SetText(_worldPosText, txt);

            byte disables = map.Get(partyData.MapX, partyData.MapZ, CellIndex.Disables);

            _clientEntityService.SetActive(_noMeleeImage, FlagUtils.IsSet(disables, MapDisables.NoMelee));
            _clientEntityService.SetActive(_noRangedImage, FlagUtils.IsSet(disables, MapDisables.NoRanged));
            _clientEntityService.SetActive(_noMagicImage, FlagUtils.IsSet(disables, MapDisables.NoMagic));

        }

        private void OnShowTooltip(ShowCrawlerTooltipEvent showEvent)
        {
            _taskService.ForgetTask(OnShowTooltipAsync(showEvent));
        }

        private async Task OnShowTooltipAsync(ShowCrawlerTooltipEvent showEvent)
        {        
            if (showEvent.Lines.Count < 1)
            {
                return;
            }

            _clientEntityService.DestroyAllChildren(_tooltipContent);
            _clientEntityService.SetActive(_tooltipParent, true);

            for (int i = 0; i < showEvent.Lines.Count; i++)
            {
                WorldPanelText text = await _assetService.InitViewController<WorldPanelText, string>(showEvent.Lines[i], _textRow, _tooltipContent, _token);
            }
        }

        private void OnHideTooltip(HideCrawlerTooltipEvent hideEvent)
        {
            _clientEntityService.SetActive(_tooltipParent, false);
        }


        private string _currentSpriteName = null;
        private string _newSpriteName = null;
        private Dictionary<string, TextureList> _cachedSprites = new Dictionary<string, TextureList>();
        public void SetPicture(string spriteName)
        {
            _newSpriteName = spriteName;
        }

        private void LateUpdatePicture()
        {
            string spriteName = _newSpriteName;
            if (_newSpriteName == _currentSpriteName)
            {
                return;
            }
            if (!string.IsNullOrEmpty(spriteName) && spriteName.IndexOf("Building") >= 0)
            {
                spriteName = _mapService.GetBuildingArtPrefix() + spriteName;
            }

            if (_worldImage == null)
            {
                return;
            }
            if (string.IsNullOrEmpty(spriteName))
            {
                _uiService.SetImageTexture(_worldImage, null);
                _clientEntityService.SetActive(_worldImage, false);
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
                    _clientEntityService.SetActive(_worldImage, true);
                    _uiService.SetImageTexture(_worldImage, textureList.Textures[0]);
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
            GameObject go = obj as GameObject;

            if (go == null)
            {
                return;
            }

            string spriteName = data as string;

            if (spriteName == null)
            {
                _clientEntityService.Destroy(go);
                return;
            }

            if (_cachedSprites.TryGetValue(spriteName, out TextureList texList))
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
               // _clientEntityService.AddToParent(go, gameObject);
                _cachedSprites[spriteName] = texList;
            }
            _clientEntityService.SetActive(_worldImage, true);
            _uiService.SetImageTexture(_worldImage, texList.Textures[0]);
            SetTextureFrame(0);
        }

        const int FramesBetweenIncrement = 20;
        int currIncrementFrame = 0;
        int _currentTextureFrame = 0;
        private void OnLateUpdate()
        {
            LateUpdateWorldUI();
            LateUpdatePicture();
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
            if (_worldImage == null || _uiService.GetImageTexture(_worldImage) == null)
            {
                return;
            }

            int height = _uiService.GetImageHeight(_worldImage);    
            int width = _uiService.GetImageWidth(_worldImage);

            int frameCount = 1;

            if (width > height)
            {
                frameCount = width / height;
            }

            int currFrame = _currentTextureFrame % frameCount;

            float xmin = 1.0f * currFrame / frameCount;
            float xmax = 1.0f * (currFrame + 1) / frameCount;
            _uiService.SetUVRect(_worldImage, xmin, 0, 1.0f / frameCount, 1);
        }

        public void UpdateTime(PartyData partyData)
        {
        }
    }
}
