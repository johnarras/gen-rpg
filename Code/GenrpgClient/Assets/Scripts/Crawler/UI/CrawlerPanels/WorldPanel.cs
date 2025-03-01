

using Assets.Scripts.ClientEvents;
using Genrpg.Shared.Crawler.Maps.Constants;
using Genrpg.Shared.Crawler.Maps.Entities;
using Assets.Scripts.Crawler.Tilemaps;
using Assets.Scripts.Crawler.UI.WorldUI;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Utils;
using System;
using System.Threading;
using Genrpg.Shared.Crawler.GameEvents;
using Genrpg.Shared.Crawler.Maps.Services;
using Genrpg.Shared.Client.Assets.Constants;
using Genrpg.Shared.MVC.Interfaces;
using System.Threading.Tasks;
using Genrpg.Shared.UI.Interfaces;
using Assets.Scripts.Assets.Textures;
using Genrpg.Shared.Crawler.Constants;
using Assets.Scripts.Crawler.Combat;
using Assets.Scripts.Info.UI;
using Assets.Scripts.Crawler.ClientEvents.WorldPanelEvents;

namespace Assets.Scripts.UI.Crawler.CrawlerPanels
{

    public class ShowWorldPanelImage
    {
        public string SpriteName;
    }

    public class WorldPanel : BaseCrawlerPanel
    {
        const string PanelTextPrefab = "WorldPanelText";
        const string DefaultWorldBG = "DefaultWorldBG";
        private ICrawlerMapService _crawlerMapService;
        private ICrawlerWorldService _worldService;


        private AnimatedSprite _bgImage;
        private AnimatedSprite _worldImage;
        private AnimatedSprite _combatBgImage;

        private IText _mapNameText;
        private IText _mapPositionText;
        private IText _timeOfDayText;

        private IImage _peacefulImage;
        private IImage _noMagicImage;

        private object _infoParent;
        private InfoPanel _infoPanel;
        private object _panelButtonsParent;

        private IButton _closeTooltipButton;

        private object _root;

        private CrawlerTilemap _minimap;

        private IView _textRow;

        private WorldPanelButtons _panelButtons;
        private WorldPanelCompass _panelCompass;

        private CrawlerCombatUI _combatUI;


        public override async Task Init(CrawlerScreen screen, IView view, CancellationToken token)
        {
            await base.Init(screen, view, token);
            AddListener<ShowWorldPanelImage>(OnShowWorldPanelImage);
            AddListener<CrawlerUIUpdate>(OnUpdateWorldUI);
            AddListener<ShowInfoPanelEvent>(OnShowTooltip);
            AddListener<HideInfoPanelEvent>(OnHideTooltip);
            AddListener<UpdateCombatGroups>(OnUpdateCombatGroups);

            _panelCompass = _view.Get<WorldPanelCompass>("Compass");
            _timeOfDayText = _view.Get<IText>("TimeOfDayText");
            _mapNameText = _view.Get<IText>("MapNameText");
            _mapPositionText = _view.Get<IText>("MapPositionText");
            _panelButtonsParent = _view.Get<object>("PanelButtonsParent");
            _peacefulImage = _view.Get<IImage>("Peaceful");
            _noMagicImage = _view.Get<IImage>("NoMagicImage");
            _minimap = _view.Get<CrawlerTilemap>("Minimap");
            _gs.loc.Resolve(_minimap);
            _infoParent = _view.Get<object>("InfoParent");
            _root = _view.Get<object>("Root");
            _combatUI = _view.Get<CrawlerCombatUI>("CombatUI");

            _bgImage = _view.Get<AnimatedSprite>("BGImage");
            _worldImage = _view.Get<AnimatedSprite>("WorldImage");
            _infoPanel = _view.Get<InfoPanel>("InfoPanel");

            _clientEntityService.SetActive(_infoPanel, false);

            _clientEntityService.SetActive(_infoParent, false);

            _uiService.SetButton(_closeTooltipButton, GetType().Name, () => { OnHideTooltip(new HideInfoPanelEvent()); });

            _textRow = await _assetService.LoadAssetAsync<IView>(AssetCategoryNames.UI, PanelTextPrefab, _root, _token, _model.Subdirectory);

            _panelButtons = await _assetService.CreateAsync<WorldPanelButtons, WorldPanel>(this, AssetCategoryNames.UI, "WorldPanelButtons", _panelButtonsParent, token, screen.Subdirectory);

            AddUpdate(OnLateUpdate, UpdateTypes.Late);

            SetPicture(CrawlerClientConstants.DefaultWorldBG, true);

        }

        public override async Task OnNewStateData(CrawlerStateData stateData, CancellationToken token)
        {
            SetPicture(stateData.WorldSpriteName, stateData.BGImageOnly);
            await Task.CompletedTask;
        }

        private void OnShowWorldPanelImage(ShowWorldPanelImage imageToShow)
        {
            SetPicture(imageToShow.SpriteName, false);
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

            _uiService.SetText(_mapPositionText, "(" + partyData.MapX + "," + partyData.MapZ + ")");    

            _uiService.SetText(_mapNameText, map.GetName(partyData.MapX, partyData.MapZ));

            int magicBits = _crawlerMapService.GetMagicBits(partyData.MapId, partyData.MapX, partyData.MapZ);

            _clientEntityService.SetActive(_peacefulImage, FlagUtils.IsSet(magicBits, MapMagics.Peaceful));
            _clientEntityService.SetActive(_noMagicImage, FlagUtils.IsSet(magicBits, MapMagics.NoMagic));

        }

        private void OnShowTooltip(ShowInfoPanelEvent showEvent)
        {
            if (showEvent.EntityTypeId > 0 && showEvent.EntityId > 0)
            {
                _infoPanel.ShowInfo(showEvent.EntityTypeId, showEvent.EntityId);
            }
            else if (showEvent.Lines.Count > 0)
            {
                _infoPanel.ShowLines(showEvent.Lines);
            }
            else
            {
                return;
            }
            _clientEntityService.SetActive(_infoPanel, true);
        }

        private void OnHideTooltip(HideInfoPanelEvent hideEvent)
        {
            _clientEntityService.SetActive(_infoParent, false);
        }

        private void OnUpdateCombatGroups(UpdateCombatGroups update)
        {
            UpdateCOmbatGroupsInternal();
        }

        public void SetPicture(string spriteName, bool useBgOnly)
        {
            if (!useBgOnly)
            {
                if (string.IsNullOrEmpty(spriteName))
                {
                    _bgImage?.SetImage(null);
                    _worldImage?.SetImage(null);
                }
                else
                {
                    _bgImage?.SetImage(_crawlerMapService.GetBGImageName());
                    _worldImage?.SetImage(spriteName);
                }
            }
            else
            {
                _worldImage?.SetImage(null);
                if (string.IsNullOrEmpty(spriteName))
                {
                    _bgImage?.SetImage(null);
                }
                else
                {
                    _bgImage?.SetImage(spriteName);
                }
            }
        }

       


        public void ApplyEffect(string effectName, float duration)
        {
        }

        private void OnLateUpdate()
        {
            LateUpdateWorldUI();
        }

        private void UpdateCOmbatGroupsInternal()
        {
            _combatUI?.UpdateData();
        }
    }
}
