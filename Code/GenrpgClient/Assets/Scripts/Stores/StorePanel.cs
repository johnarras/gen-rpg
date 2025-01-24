
using Genrpg.Shared.Client.Assets.Constants;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Purchasing.PlayerData;
using Genrpg.Shared.Purchasing.Settings;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Assets.Scripts.UI.Stores
{
    public class StorePanel : BaseBehaviour
    {

        const string ProductPanelPrefab = "StoreProductPanel";

        public GText Header;
        public GameObject ProductParent;

        private List<StoreProductPanel> _panels = new List<StoreProductPanel>();

        private StoreScreen _screen;
        private PlayerStoreOffer _offer;
        private StoreTheme _theme;

        public long Index()
        {
            return _offer.StoreSlotId;
        }

        public void Init(StoreScreen screen, PlayerStoreOffer offer, CancellationToken token)
        {
            _screen = screen;
            _offer = offer;

            _theme = _gameData.Get<StoreThemeSettings>(_gs.ch).Get(offer.StoreThemeId);

            _uiService.SetText(Header, _offer.Name);

            foreach (PlayerStoreOfferItem item in _offer.Items)
            {
                _assetService.LoadAssetInto(ProductParent, AssetCategoryNames.Stores, ProductPanelPrefab, OnLoadStorePanel,
                    item, token, _theme.Art);
            }
        }
        
        private void OnLoadStorePanel(object obj, object data, CancellationToken token)
        {
            GameObject go = obj as GameObject;

            if (go == null)
            {
                return;
            }

            StoreProductPanel productPanel = go.GetComponent<StoreProductPanel>();

            productPanel.Init(data as PlayerStoreOfferItem, _screen.GetName(), _theme, token);
        }
    }
}
