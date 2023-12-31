﻿using Cysharp.Threading.Tasks;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Login.Messages.RefreshStores;
using Genrpg.Shared.Purchasing.PlayerData;
using Genrpg.Shared.Purchasing.Settings;
using Genrpg.Shared.Stats.Messages;
using System.Collections.Generic;
using System.Threading;
using GEntity = UnityEngine.GameObject;

namespace Assets.Scripts.UI.Stores
{
    public class StoreScreen : BaseScreen
    {
        public GEntity StoreParent;

        public GButton CloseButton;

        const string StorePanelPrefab = "StorePanel";

        private List<StorePanel> _panels = new List<StorePanel>();
        private PlayerStoreOfferData _offerData = null;
        protected override async UniTask OnStartOpen(object data, CancellationToken token)
        {
            _offerData = _gs.ch.Get<PlayerStoreOfferData>();

            UIHelper.SetButton(CloseButton, GetAnalyticsName(), StartClose);
          
            SetupData(token);

            _gs.AddEvent<RefreshStoresResult>(this, OnRefreshStores);
            await UniTask.CompletedTask;
        }

        private void SetupData(CancellationToken token)
        {
            if (_offerData == null || StoreParent == null)
            {
                StartClose();
                return;
            }

            GEntityUtils.DestroyAllChildren(StoreParent);

            foreach (PlayerStoreOffer offer in _offerData.StoreOffers)
            {
                StoreTheme theme = _gs.data.GetGameData<StoreThemeSettings>(_gs.ch).GetStoreTheme(offer.StoreThemeId);

                _assetService.LoadAssetInto(_gs, StoreParent, AssetCategoryNames.Stores, StorePanelPrefab, OnLoadStorePanel, offer, token, theme.Art);
            }
        }

        private void OnLoadStorePanel(GameState gs, string url, object obj, object data, CancellationToken token)
        {
            GEntity go = obj as GEntity;

            if (go == null)
            {
                return;
            }

            StorePanel storePanel = go.GetComponent<StorePanel>();

            storePanel.Init(this, data as PlayerStoreOffer, token);
            _panels.Add(storePanel);

        }

        private RefreshStoresResult OnRefreshStores (UnityGameState gs, RefreshStoresResult result)
        {
            _offerData = result.Stores;
            SetupData(_token);
            return null;
        }
    }
}
