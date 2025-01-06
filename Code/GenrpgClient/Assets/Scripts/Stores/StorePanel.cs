
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

            foreach (PlayerOfferProduct product in _offer.Products)
            {
                _assetService.LoadAssetInto(ProductParent, AssetCategoryNames.Stores, ProductPanelPrefab, OnLoadStorePanel,
                    product, token, _theme.Art);
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

            productPanel.Init(data as PlayerOfferProduct, _screen.GetName(), _theme, token);
        }
    }
}
