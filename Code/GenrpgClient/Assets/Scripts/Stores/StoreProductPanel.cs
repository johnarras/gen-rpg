
using Genrpg.Shared.Client.Assets.Constants;
using Genrpg.Shared.Purchasing.PlayerData;
using Genrpg.Shared.Purchasing.Settings;
using Genrpg.Shared.Spawns.Settings;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Assets.Scripts.UI.Stores
{
    public class StoreProductPanel : BaseBehaviour
    {
        public GText PriceAmount;
        public GText Name;
        public GText Description;
        public GameObject RewardAnchor;
        public GButton PurchaseButton;

        const string RewardPanelPrefabName = "StoreRewardPanel";

        private List<StoreRewardPanel> _rewards = new List<StoreRewardPanel>();


        private PlayerStoreOfferItem _item = null;
        private StoreTheme _theme;
        private CancellationToken _token;

        public long Index()
        {
            return _item.Index;
        }

        public void Init(PlayerStoreOfferItem product, string screenName, StoreTheme theme,CancellationToken token)
        {
            _item = product;
            _theme = theme;
            _token = token;

            _uiService.SetText(Name, product.StoreItem.Name);
            _uiService.SetText(PriceAmount, "$" + product.Sku.DollarPrice);
            _uiService.SetText(Description, product.StoreItem.Desc);
            _uiService.SetButton(PurchaseButton, screenName, OnPurchaseItem);

            if (RewardAnchor != null)
            {
                foreach (SpawnItem spawnItem in product.StoreItem.Rewards)
                {
                    _assetService.LoadAsset(AssetCategoryNames.Stores, RewardPanelPrefabName, OnDownloadReward, spawnItem, RewardAnchor, token, theme.Art);
                }
            }
        }

        private void OnDownloadReward(object obj, object data, CancellationToken token)
        {
            if (obj == null)
            {
                return;
            }

            GameObject go = obj as GameObject;

            if (go == null)
            {
                return;
            }

            StoreRewardPanel rewardPanel = go.GetComponent<StoreRewardPanel>();

            _rewards.Add(rewardPanel);

            rewardPanel.Init(data as SpawnItem, token);

        }

        private void OnPurchaseItem()
        {

        }
    }
}
