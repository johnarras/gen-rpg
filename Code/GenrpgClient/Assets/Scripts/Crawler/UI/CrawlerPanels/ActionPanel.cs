using Assets.Scripts.Crawler.UI.ActionUI;
using Assets.Scripts.MVC;
using Assets.Scripts.UI.Core;
using Assets.Scripts.UI.Crawler.ActionUI;
using Genrpg.Shared.Client.Assets.Constants;
using Genrpg.Shared.Crawler.States.Constants;
using Genrpg.Shared.Crawler.States.Entities;
using Genrpg.Shared.Crawler.UI.Interfaces;
using Genrpg.Shared.MVC.Interfaces;
using Genrpg.Shared.UI.Interfaces;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Assets.Scripts.UI.Crawler.CrawlerPanels
{
    public class TextAction
    {
        public string Text { get; set; }
        public Action ClickAction { get; set; }
    }
    public class ActionPanel : BaseCrawlerPanel, IActionPanel
    {

        const string PanelRowPrefab = "ActionPanelRow";
        const string PanelTextPrefab = "ActionPanelText";
        const string PanelGridPrefab = "ActionPanelGrid";
        const string PanelButtonPrefab = "ActionPanelButton";

        private object _content;

        private object _root;

        private ConcurrentQueue<TextAction> _textToShow = new ConcurrentQueue<TextAction>();    

        public const int InputCount = 3;
        private List<ILabeledInputField> _inputs = new List<ILabeledInputField>();

        private IView _panelRow = null; 
        private IView _panelText = null;
        private IView _panelGrid = null;
        private IView _panelButton = null;

        private CrawlerStateData _nextStateData = null;

        public object _scrollRect = null;

        private List<object> _subObjects = new List<object>();

        public override async Task Init(CrawlerScreen model, IView view, CancellationToken token)
        {
            await base.Init(model, view, token);

            _content = view.Get<object>("Content");
            _root = view.Get<object>("Root");

            for (int i =0; i < InputCount; i++)
            {
                _inputs.Add(view.Get<ILabeledInputField>("Input" + i));
                _gs.loc.Resolve(_inputs[i]);
            }

            _scrollRect = view.Get<IScrollRect>("ScrollRect");

            _panelRow = await _assetService.LoadAssetAsync<IView>(AssetCategoryNames.UI, PanelRowPrefab, _root, _token, _model.Subdirectory);
            _clientEntityService.SetActive(_panelRow, false);
            _panelText = await _assetService.LoadAssetAsync<IView>(AssetCategoryNames.UI, PanelTextPrefab, _root, _token, _model.Subdirectory);
            _clientEntityService.SetActive(_panelText, false);
            _panelGrid = await _assetService.LoadAssetAsync<IView>(AssetCategoryNames.UI, PanelGridPrefab, _root, _token, _model.Subdirectory);
            _clientEntityService.SetActive(_panelGrid, false);
            _panelButton = await _assetService.LoadAssetAsync<IView>(AssetCategoryNames.UI, PanelButtonPrefab, _root, _token, _model.Subdirectory);
            _clientEntityService.SetActive(_panelButton, false);

            AddUpdate(OnLateUpdate, UpdateType.Late);
        }

        private bool IsSetUp()
        {
            return _panelButton != null &&
                _panelRow != null &&
                _panelText != null &&
                _panelGrid != null;

        }

        public override async Task OnNewStateData(CrawlerStateData stateData, CancellationToken token)
        {
            _nextStateData = stateData;

            if (!IsSetUp())
            {
                return;
            }

            _clientEntityService.DestroyAllChildren(_content);

            List<CrawlerStateAction> buttonActions = new List<CrawlerStateAction>();

            for (int a = 0; a < stateData.Actions.Count; a++)
            {

                CrawlerStateAction action = stateData.Actions[a];

                if (action.Key == CharCodes.Escape && stateData.HasInput())
                {
                    continue;
                }

                if (!action.ForceButton && !action.RowFiller && (action.Key == CharCodes.Escape || action.Key == CharCodes.Space ||
                    string.IsNullOrEmpty(action.Text) || action.Text.Length >= 20 ||
                    action.NextState == ECrawlerStates.None))
                {
                    ActionPanelRow actionPanelRow = await _assetService.InitViewController<ActionPanelRow, CrawlerStateWithAction>(
                        new CrawlerStateWithAction() { State = stateData, Action = stateData.Actions[a] },
                        _panelRow,
                        _content, _token);
                    _subObjects.Add(actionPanelRow);    
                }
                else
                {
                    buttonActions.Add(action);
                }
            }

            ActionPanelGrid grid = null;

            for (int a = 0; a < buttonActions.Count; a++)
            {
                if (grid == null)
                {
                    grid = await _assetService.InitViewController<ActionPanelGrid,bool>(stateData.UseSmallerButtons, _panelGrid, _content, _token);
                    _subObjects.Add(grid);
                }

                CrawlerStateAction action = buttonActions[a];

                if (action.RowFiller)
                {
                    grid = null;
                    continue;
                }

                CrawlerStateWithAction stateAction = new CrawlerStateWithAction()
                {
                    State = stateData,
                    Action = action,
                };

                ActionPanelRow button = await _assetService.InitViewController<ActionPanelRow,CrawlerStateWithAction>(stateAction, _panelButton, grid.GetContentRoot(), _token);
                _subObjects.Add(button);
            }

            List<CrawlerInputData> stateInputs = stateData.Inputs;

            for (int i = 0; i < _inputs.Count; i++)
            {
                _inputs[i].SetLabel("");
                _inputs[i].SetPlaceholder("");
                _inputs[i].SetInputText("");
                _clientEntityService.SetActive(_inputs[i], false);
            }

            for (int i = 0; i < _inputs.Count && i < stateInputs.Count; i++)
            {
                _clientEntityService.SetActive(_inputs[i], true);
                stateInputs[i].InputField = _inputs[i];
                _inputs[i].SetLabel(stateInputs[i].InputLabel);
                _inputs[i].SetPlaceholder(stateData.InputPlaceholderText);
                _inputs[i].SetInputText("");
            }

            _uiService.ScrollToBottom(_scrollRect);
            await Task.CompletedTask;
        }

        public void Clear()
        {
            _textToShow.Clear();
            _clientEntityService.DestroyAllChildren(_content);
            _subObjects.Clear();
        }

        public void AddText(string text, Action onClickHandler = null)
        {
            _textToShow.Enqueue(new TextAction() { Text=text, ClickAction=onClickHandler });
        }

        private bool _showingText = false;
        private void OnLateUpdate()
        {

            if (_showingText)
            {
                return;
            }
            if (_textToShow.Count > 0)
            {
                _taskService.ForgetTask(OnLateUpdateAsync());
            }

        }

        private async Task OnLateUpdateAsync()
        { 
            bool showedText = false;
            while (_textToShow.TryDequeue(out TextAction action))
            {
                ActionPanelText actionPanelRow = await _assetService.InitViewController<ActionPanelText, TextAction>(
                    action,
                    _panelText,
                    _content, _token);
                _subObjects.Add(actionPanelRow);

                showedText = true;
            }

            if (showedText)
            {
                await Task.Delay(50);
                _uiService.ScrollToBottom(_scrollRect);
            }
            _showingText = false;
        }
    }
}
