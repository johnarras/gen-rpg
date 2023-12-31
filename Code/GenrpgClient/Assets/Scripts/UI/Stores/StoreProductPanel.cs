﻿using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Triggers;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Purchasing.PlayerData;
using Genrpg.Shared.Purchasing.Settings;
using Genrpg.Shared.Spawns.Settings;
using System.Collections.Generic;
using System.Threading;
using GEntity = UnityEngine.GameObject;

namespace Assets.Scripts.UI.Stores
{
    public class StoreProductPanel : BaseBehaviour
    {
        public GText PriceAmount;
        public GText Name;
        public GText Description;
        public GEntity RewardAnchor;
        public GButton PurchaseButton;

        const string RewardPanelPrefabName = "StoreRewardPanel";

        private List<StoreRewardPanel> _rewards = new List<StoreRewardPanel>();


        private PlayerOfferProduct _product = null;
        private StoreTheme _theme;
        private CancellationToken _token;

        public long Index()
        {
            return _product.Index;
        }

        public void Init(PlayerOfferProduct product, string screenName, StoreTheme theme,CancellationToken token)
        {
            _product = product;
            _theme = theme;
            _token = token;

            UIHelper.SetText(Name, product.Product.Name);
            UIHelper.SetText(PriceAmount, "$" + product.Sku.DollarPrice);
            UIHelper.SetText(Description, product.Product.Desc);
            UIHelper.SetButton(PurchaseButton, screenName, OnPurchaseItem);

            if (RewardAnchor != null)
            {
                foreach (SpawnItem spawnItem in product.Product.Rewards)
                {
                    _assetService.LoadAsset(_gs, AssetCategoryNames.Stores, RewardPanelPrefabName, OnDownloadReward, spawnItem, RewardAnchor, token, theme.Art);
                }
            }
        }

        private void OnDownloadReward(GameState gs, string url, object obj, object data, CancellationToken token)
        {
            if (obj == null)
            {
                return;
            }

            GEntity go = obj as GEntity;

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
