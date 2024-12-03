

using Assets.Scripts.ClientEvents;
using Genrpg.Shared.Crawler.Maps.Constants;
using Genrpg.Shared.Crawler.Maps.Entities;
using Assets.Scripts.Crawler.Tilemaps;
using Assets.Scripts.Crawler.UI.WorldUI;
using Assets.Scripts.ProcGen.Services;
using Assets.Scripts.UI.Crawler.WorldUI;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.UI.Interfaces;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.Shared.Utils;
using System;
using System.Threading;
using Genrpg.Shared.Crawler.GameEvents;
using Genrpg.Shared.Crawler.Maps.Services;
using Genrpg.Shared.Client.Assets.Constants;
using Genrpg.Shared.MVC.Interfaces;
using System.Threading.Tasks;
using Genrpg.Shared.UI.Interfaces;
using UnityEngine;
using UnityEngine.UI;
using Assets.Scripts.TextureLists.Services;
using Genrpg.Shared.Crawler.TextureLists.Services;
using Assets.Scripts.Assets.Textures;
using Genrpg.Shared.Crawler.Constants;
using Assets.Scripts.Crawler.Combat;

namespace Assets.Scripts.UI.Crawler.CrawlerPanels
{

    public class ShowWorldPanelImage
    {
        public string SpriteName;
    }

    public class WorldPanel : BaseCrawlerPanel, IWorldPanel
    {
        const string PanelTextPrefab = "WorldPanelText";
        const string DefaultWorldBG = "DefaultWorldBG";
        private ICrawlerMapService _crawlerMapService;
        private ICurveGenService _curveGenService;
        private ICrawlerWorldService _worldService;


        private AnimatedTexture _bgImage;
        private AnimatedTexture _worldImage;

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

        private CrawlerCombatUI _combatUI;

        public override async Task Init(CrawlerScreen screen, IView view, CancellationToken token)
        {
            await base.Init(screen, view, token);
            AddListener<ShowWorldPanelImage>(OnShowWorldPanelImage);
            AddListener<CrawlerUIUpdate>(OnUpdateWorldUI);
            AddListener<ShowCrawlerTooltipEvent>(OnShowTooltip);
            AddListener<HideCrawlerTooltipEvent>(OnHideTooltip);

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
            _combatUI = _view.Get<CrawlerCombatUI>("CombatUI");

            _bgImage = _view.Get<AnimatedTexture>("BGImage");
            _worldImage = _view.Get<AnimatedTexture>("WorldImage");

            _clientEntityService.SetActive(_tooltipParent, false);

            _uiService.SetButton(_closeTooltipButton, GetType().Name, () => { OnHideTooltip(new HideCrawlerTooltipEvent()); });

            _textRow = await _assetService.LoadAssetAsync<IView>(AssetCategoryNames.UI, PanelTextPrefab, _root, _token, _model.Subdirectory);

            _panelButtons = await _assetService.CreateAsync<WorldPanelButtons, WorldPanel>(this, AssetCategoryNames.UI, "WorldPanelButtons", _panelButtonsParent, token, screen.Subdirectory);

            AddUpdate(OnLateUpdate, UpdateType.Late);

            SetPicture(CrawlerClientConstants.DefaultWorldBG);

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


        public void SetPicture(string spriteName)
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

       


        public void ApplyEffect(string effectName, float duration)
        {
        }

        private void OnLateUpdate()
        {
            LateUpdateWorldUI();
        }

        public void UpdateCombatGroups()
        {
            _combatUI?.UpdateData();
        }
    }
}
