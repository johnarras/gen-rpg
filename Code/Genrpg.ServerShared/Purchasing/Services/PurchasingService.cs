using Genrpg.ServerShared.Core;
using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.GameSettings.PlayerData;
using Genrpg.Shared.GameSettings.Settings;
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
        public async Task Setup(GameState gs, CancellationToken token)
        {
            await Task.CompletedTask;
        }

        public async Task<PlayerStoreOfferData> GetCurrentStores(ServerGameState gs, User user, Character ch, bool forceRefresh)
        {

            PlayerStoreOfferData storeOfferData = await gs.repo.Load<PlayerStoreOfferData>(user.Id);

            if (storeOfferData == null)
            {
                storeOfferData = new PlayerStoreOfferData() { Id = user.Id };
            }

            DateTime currentTime = DateTime.UtcNow;

            VersionSettings versionSettings = gs.data.Get<VersionSettings>(ch);

            StoreOfferSettings storeOfferSettings = gs.data.Get<StoreOfferSettings>(ch);
            ProductSkuSettings skuSettings = gs.data.Get<ProductSkuSettings>(ch);
            StoreFeatureSettings featureSettings = gs.data.Get<StoreFeatureSettings>(ch);
            StoreSlotSettings slotSettings = gs.data.Get<StoreSlotSettings>(ch);
            StoreProductSettings productSettings = gs.data.Get<StoreProductSettings>(ch);

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

            IReadOnlyList<StoreOffer> storeOffers = gs.data.Get<StoreOfferSettings>(ch).GetData();

            Dictionary<long, StoreOffer> storeDict = new Dictionary<long, StoreOffer>();

            storeOfferData.StoreOffers.Clear();

            PurchaseHistoryData historyData = await gs.repo.Load<PurchaseHistoryData>(user.Id);

            if (historyData == null)
            {
                historyData = new PurchaseHistoryData() { Id = user.Id };
                await gs.repo.Save(historyData);
            }

            foreach (StoreOffer offer in storeOffers)
            {
                TryAddOffer(gs, offer, storeDict, user, ch, historyData);
            }

            foreach (StoreOffer storeOffer in storeDict.Values)
            {
                StoreSlot slot = slotSettings.Get(storeOffer.StoreSlotId);
                StoreFeature feature = featureSettings.Get(storeOffer.StoreFeatureId);

                if (slot == null || feature == null)
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

                foreach (OfferProduct offerProduct in storeOffer.Products)
                {
                    StoreProduct storeProduct = productSettings.Get(offerProduct.StoreProductId);
                    ProductSku sku = skuSettings.Get(offerProduct.ProductSkuId);

                    if (storeProduct != null && sku != null)
                    {
                        PlayerOfferProduct playerOfferProduct = new PlayerOfferProduct()
                        {
                            Index = offerProduct.Index,
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

            await gs.repo.Save(storeOfferData);

            return storeOfferData;
        }

        protected void TryAddOffer (ServerGameState gs, StoreOffer offer, Dictionary<long,StoreOffer> currentOffers, User user, Character ch, PurchaseHistoryData historyData)
        {

            if (offer.UseDateRange &&
                (offer.StartDate > DateTime.UtcNow ||
                offer.EndDate < DateTime.UtcNow))
            {
                return;
            }

            if (currentOffers.TryGetValue(offer.StoreSlotId, out StoreOffer currentOffer) && 
                currentOffer.Priority >= offer.Priority)
            {
                return;
            }
            
         
            if (offer.MaxUserDaysSinceInstall > 0 && (DateTime.UtcNow-user.CreationDate).Days > offer.MaxUserDaysSinceInstall)
            {
                return;
            }

            if (offer.MinUserDaysSinceInstall > 0 && (DateTime.UtcNow-user.CreationDate).Days < offer.MinUserDaysSinceInstall)
            {
                return;
            }

            if (ch != null)
            {
                if (offer.MaxCharDaysSinceInstall > 0 && (DateTime.UtcNow - ch.CreationDate).Days > offer.MaxCharDaysSinceInstall)
                {
                    return;
                }

                if (offer.MinCharDaysSinceInstall > 0 && (DateTime.UtcNow - ch.CreationDate).Days < offer.MinCharDaysSinceInstall)
                {
                    return;
                }
            }

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

            if (offer.MaxPurchaseTotal > 0 && historyData.PurchaseTotal >  offer.MaxPurchaseTotal)
            {
                return;
            }

            if (ch != null)
            {
                if (offer.MinLevel > 0 && ch.Level < offer.MinLevel)
                {
                    return;
                }

                if (offer.MaxLevel > 0 && ch.Level > offer.MaxLevel)
                {
                    return;
                }
            }

            currentOffers[offer.StoreSlotId] = offer;
        }
    }
}
