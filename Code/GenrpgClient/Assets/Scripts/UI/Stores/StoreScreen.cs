
using Genrpg.Shared.Website.Messages.RefreshStores;
using Genrpg.Shared.Purchasing.PlayerData;
using Genrpg.Shared.Purchasing.Settings;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Genrpg.Shared.Client.Assets.Constants;

namespace Assets.Scripts.UI.Stores
{
    public class StoreScreen : BaseScreen
    {
        public GameObject StoreParent;

        const string StorePanelPrefab = "StorePanel";

        private List<StorePanel> _panels = new List<StorePanel>();
        private PlayerStoreOfferData _offerData = null;
        protected override async Task OnStartOpen(object data, CancellationToken token)
        {
            _offerData = _gs.ch.Get<PlayerStoreOfferData>();

            SetupData(token);

            AddListener<RefreshStoresResult>(OnRefreshStores);

            await Task.CompletedTask;
        }

        private void SetupData(CancellationToken token)
        {
            if (_offerData == null || StoreParent == null)
            {
                StartClose();
                return;
            }

            _gameObjectService.DestroyAllChildren(StoreParent);

            foreach (PlayerStoreOffer offer in _offerData.StoreOffers)
            {
                StoreTheme theme = _gameData.Get<StoreThemeSettings>(_gs.ch).Get(offer.StoreThemeId);

                _assetService.LoadAssetInto(StoreParent, AssetCategoryNames.Stores, StorePanelPrefab, OnLoadStorePanel, offer, token, theme.Art);
            }
        }

        private void OnLoadStorePanel(object obj, object data, CancellationToken token)
        {
            GameObject go = obj as GameObject;

            if (go == null)
            {
                return;
            }

            StorePanel storePanel = go.GetComponent<StorePanel>();

            storePanel.Init(this, data as PlayerStoreOffer, token);
            _panels.Add(storePanel);

        }

        private void OnRefreshStores (RefreshStoresResult result)
        {
            _offerData = result.Stores;
            SetupData(_token);
            return;
        }
    }
}
