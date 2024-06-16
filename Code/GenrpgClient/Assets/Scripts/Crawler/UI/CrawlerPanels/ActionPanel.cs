using Assets.Scripts.Crawler.CrawlerStates;
using Assets.Scripts.UI.Core;
using Assets.Scripts.UI.Crawler.ActionUI;

using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Crawler.UI.Interfaces;
using Genrpg.Shared.DataStores.Entities;
using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using GEntity = UnityEngine.GameObject;

namespace Assets.Scripts.UI.Crawler.CrawlerPanels
{
    public class ActionPanel : BaseCrawlerPanel, IActionPanel
    {
        const string PanelRowPrefab = "ActionPanelRow";
        const string PanelTextPrefab = "ActionPanelText";

        public GEntity Content;

        public LabeledInputField Input;

        private ActionPanelRow _panelRow = null; // Need to load once and then use over and over;
        private ActionPanelText _panelText = null;

        private CrawlerStateData _nextStateData = null;

        public ScrollRect ScrollRect = null;

        public override async Awaitable Init(CrawlerScreen screen, CancellationToken token)
        {
            await base.Init(screen, token);

            _assetService.LoadAsset(AssetCategoryNames.UI, PanelRowPrefab, OnLoadPanelRow, null, this, token, screen.Subdirectory);
            _assetService.LoadAsset(AssetCategoryNames.UI, PanelTextPrefab, OnLoadTextRow, null, this, token, screen.Subdirectory);

        }

        private void OnLoadPanelRow(object obj, object data, CancellationToken token)
        {
            GEntity entity = obj as GEntity;
            _panelRow = GEntityUtils.GetComponent<ActionPanelRow>(entity);
            GEntityUtils.SetActive(entity, false);
            GEntityUtils.AddToParent(entity, gameObject);
            if (_nextStateData != null)
            {
                OnNewStateData(_nextStateData);
            }
        }

        private void OnLoadTextRow(object obj, object data, CancellationToken token)
        {
            GEntity entity = obj as GEntity;
            _panelText = GEntityUtils.GetComponent<ActionPanelText>(entity);
            GEntityUtils.SetActive(entity, false);
            GEntityUtils.AddToParent(entity, gameObject);
        }

        public override void OnNewStateData(CrawlerStateData stateData)
        {
            _nextStateData = stateData;

            if (_panelRow == null)
            {
                return;
            }
            GEntityUtils.DestroyAllChildren(Content);

            for (int a = 0; a < stateData.Actions.Count; a++)
            {

                CrawlerStateAction action = stateData.Actions[a];

                if (action.Key == KeyCode.Escape && stateData.HasInput())
                {
                    continue;
                }

                ActionPanelRow row = _gameObjectService.FullInstantiate<ActionPanelRow>(_panelRow);

                GEntityUtils.AddToParent(row.gameObject, Content);

                row.Init(stateData, stateData.Actions[a], _token);
            }

            if (stateData.HasInput() && Input != null)
            {
                GEntityUtils.SetActive(Input, true);
                stateData.InputField = Input.Input;
                _uIInitializable.SetText(Input.Placeholder, stateData.InputPlaceholderText);
                _uIInitializable.SetText(Input.Label, stateData.InputLabel);
                _uIInitializable.SetInputText(Input.Input, "");
            }
            else
            {
                GEntityUtils.SetActive(Input, false);
            }
            ScrollRect?.ScrollToBottom();
        }

        public void Clear()
        {
            GEntityUtils.DestroyAllChildren(Content);
        }

        public void AddText(string text)
        {
            if (string.IsNullOrEmpty(text) || _panelText == null)
            {
                return;
            }

            ActionPanelText panelText = _gameObjectService.FullInstantiate<ActionPanelText>(_panelText);

            GEntityUtils.AddToParent(panelText.gameObject, Content);

            panelText.Init(text, _token);

            ScrollRect?.ScrollToBottom();
        }
    }
}
