using Genrpg.ServerShared.Core;
using Genrpg.ServerShared.GameSettings.Services;
using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.GameSettings.PlayerData;
using Genrpg.Shared.GameSettings.Settings;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.PlayerFiltering.Interfaces;
using Genrpg.Shared.PlayerFiltering.Utils;
using Genrpg.Shared.Purchasing.PlayerData;
using Genrpg.Shared.Purchasing.Settings;
using Genrpg.Shared.Users.Entities;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Versions.Settings;
using Microsoft.Extensions.Azure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.ServerShared.Purchasing.Services
{
    public class PurchasingService : IPurchasingService
    {
        protected IRepositoryService _repoService = null;
        private IGameData _gameData = null;
        private IGameDataService _gameDataService = null;

        public async Task Initialize( CancellationToken token)
        {
            await Task.CompletedTask;
        }

        public async Task<PlayerStoreOfferData> GetCurrentStores(User user, bool forceRefresh)
        {

            PlayerStoreOfferData storeOfferData = await _repoService.Load<PlayerStoreOfferData>(user.Id);

            if (storeOfferData == null)
            {
                storeOfferData = new PlayerStoreOfferData() { Id = user.Id };
            }

            DateTime currentTime = DateTime.UtcNow;

            VersionSettings versionSettings = _gameData.Get<VersionSettings>(user);

            StoreOfferSettings storeOfferSettings = _gameData.Get<StoreOfferSettings>(user);
            ProductSkuSettings skuSettings = _gameData.Get<ProductSkuSettings>(user);
            StoreFeatureSettings featureSettings = _gameData.Get<StoreFeatureSettings>(user);
            StoreSlotSettings slotSettings = _gameData.Get<StoreSlotSettings>(user);
            StoreProductSettings productSettings = _gameData.Get<StoreProductSettings>(user);

            if (storeOfferSettings.NextUpdateTime <= DateTime.UtcNow)
            {
                storeOfferSettings.SetPrevNextUpdateTimes();
            }

            if (!forceRefresh &&
            versionSettings.GameDataSaveTime == storeOfferData.GameDataSaveTime &&
            storeOfferData.LastTimeSet >= storeOfferSettings.PrevUpdateTime &&
            storeOfferData.LastTimeSet < storeOfferSettings.NextUpdateTime)
            {
                // stores are the same
                return storeOfferData;
            }

            IReadOnlyList<StoreOffer> storeOffers = _gameData.Get<StoreOfferSettings>(user).GetData();

            Dictionary<long, StoreOffer> storeDict = new Dictionary<long, StoreOffer>();

            storeOfferData.StoreOffers.Clear();

            PurchaseHistoryData historyData = await _repoService.Load<PurchaseHistoryData>(user.Id);

            if (historyData == null)
            {
                historyData = new PurchaseHistoryData() { Id = user.Id };
                await _repoService.Save(historyData);
            }

            foreach (StoreOffer offer in storeOffers)
            {
                TryAddOffer(offer, storeDict, user, historyData);
            }

            foreach (StoreOffer storeOffer in storeDict.Values)
            {
                StoreSlot slot = slotSettings.Get(storeOffer.StoreSlotId);
                StoreFeature feature = featureSettings.Get(storeOffer.StoreFeatureId);

                if (slot == null || feature == null)
                {
                    continue;
                }

                List<OfferProduct> validProducts = storeOffer.Products.Where(x => x.Enabled).ToList();

                if (validProducts.Count < 1)
                {
                    continue;
                }

                PlayerStoreOffer playerStoreOffer = new PlayerStoreOffer()
                {
                    StoreFeatureId = storeOffer.StoreFeatureId,
                    StoreSlotId = storeOffer.StoreSlotId,
                    StoreThemeId = storeOffer.StoreThemeId,
                    EndDate = storeOffer.EndDate,
                    Art = storeOffer.Art,
                    Desc = storeOffer.Desc,
                    Icon = storeOffer.Icon,
                    IdKey = storeOffer.IdKey,
                    Name = storeOffer.Name,
                    OfferId = storeOffer.OfferId,
                    UniqueId = HashUtils.NewGuid(),
                };

                for (int p = 0; p < validProducts.Count; p++)
                {
                    OfferProduct offerProduct = validProducts[p];
                    StoreProduct storeProduct = productSettings.Get(offerProduct.StoreProductId);
                    ProductSku sku = skuSettings.Get(offerProduct.ProductSkuId);

                    if (storeProduct != null && sku != null)
                    {
                        PlayerOfferProduct playerOfferProduct = new PlayerOfferProduct()
                        {
                            Index = p,
                            Product = storeProduct,
                            Sku = sku,
                        };
                        playerStoreOffer.Products.Add(playerOfferProduct);
                    }
                }

                if (playerStoreOffer.Products.Count > 0)
                {
                    storeOfferData.StoreOffers.Add(playerStoreOffer);
                }

            }

            storeOfferData.GameDataSaveTime = versionSettings.GameDataSaveTime;
            storeOfferData.LastTimeSet = currentTime;

            await _repoService.Save(storeOfferData);

            return storeOfferData;
        }

        protected void TryAddOffer (StoreOffer offer, Dictionary<long,StoreOffer> currentOffers, IFilteredObject user, PurchaseHistoryData historyData)
        {

            if (!_gameDataService.AcceptedByFilter(user, offer))
            {
                return;
            }

            if (currentOffers.TryGetValue(offer.StoreSlotId, out StoreOffer currentOffer) && 
                currentOffer.Priority >= offer.Priority)
            {
                return;
            }

            bool forceAddThroughId = false;

            if (offer.AllowedPlayers.Count > 0)
            {
                if (offer.AllowedPlayers.Any(x=>x.PlayerId == user.Id))
                {
                    forceAddThroughId = true;
                }
            }

            if (!forceAddThroughId)
            {

                if (offer.MinPurchaseCount > 0 && historyData.PurchaseCount < offer.MinPurchaseCount)
                {
                    return;
                }

                if (offer.MaxPurchaseCount > 0 && historyData.PurchaseCount < offer.MaxPurchaseCount)
                {
                    return;
                }

                if (offer.MinPurchaseTotal > 0 && historyData.PurchaseTotal < offer.MinPurchaseTotal)
                {
                    return;
                }

                if (offer.MaxPurchaseTotal > 0 && historyData.PurchaseTotal > offer.MaxPurchaseTotal)
                {
                    return;
                }

            }

            currentOffers[offer.StoreSlotId] = offer;
        }
    }
}
