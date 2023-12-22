using Genrpg.ServerShared.Core;
using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Purchasing.PlayerData;
using Genrpg.Shared.Purchasing.Settings;
using Genrpg.Shared.Users.Entities;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Versions.Settings;
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

        public async Task<ClientStoreOfferData> GetCurrentStores(ServerGameState gs, User user, Character ch, bool forceRefresh)
        {

            CurrentStoresData storesData = await gs.repo.Load<CurrentStoresData>(user.Id);

            DateTime topOfHour = DateTime.Today.AddHours(DateTime.UtcNow.Hour);

            VersionSettings versionSettings = gs.data.GetGameData<VersionSettings>(ch);

            List<StoreOfferType> storeOfferTypes = gs.data.GetGameData<StoreOfferTypeSettings>(ch).GetData();

            if (forceRefresh ||
                versionSettings.GameDataSaveTime == storesData.LastGameDataSaveTime ||
                storesData.LastTimeSet == topOfHour ||
                storesData.Stores.Count == 0)
            {
                storesData.Stores = new List<CurrentStore>();

                Dictionary<long, StoreOfferType> storeDict = new Dictionary<long, StoreOfferType>();

                PurchaseHistoryData historyData = await gs.repo.Load<PurchaseHistoryData>(user.Id);

                foreach (StoreOfferType offer in storeOfferTypes)
                {
                    TryAddOffer(gs, offer, storeDict, user, ch, historyData);
                }

                foreach (StoreOfferType storeOffer in storeDict.Values)
                {
                    storesData.Stores.Add(new CurrentStore()
                    {
                        StoreOfferTypeId = storeOffer.IdKey,
                        UniqueId = HashUtils.NewGuid().ToString(),
                    });
                }

                storesData.LastGameDataSaveTime = versionSettings.GameDataSaveTime;
                storesData.LastTimeSet = topOfHour;
                storesData.SetDirty(true);
            }

            List<ClientStoreOffer> clientOffers = new List<ClientStoreOffer>();

            foreach (CurrentStore currentStore in storesData.Stores)
            {
                StoreOfferType offerType = storeOfferTypes.FirstOrDefault(x => x.IdKey == currentStore.StoreOfferTypeId);

                clientOffers.Add(new ClientStoreOffer()
                {
                    IdKey = offerType.IdKey,
                    Art = offerType.Art,
                    Desc = offerType.Desc,
                    Icon = offerType.Icon,
                    Name = offerType.Name,
                    Products = offerType.Products,
                    StoreSlotTypeId = offerType.StoreSlotTypeId,
                    StoreFeatureTypeId = offerType.StoreFeatureTypeId,
                    UniqueId = currentStore.UniqueId,                    
                });
            }

            return new ClientStoreOfferData() { Id = user.Id, Offers = clientOffers };
        }

        protected void TryAddOffer (ServerGameState gs, StoreOfferType offer, Dictionary<long,StoreOfferType> currentOffers, User user, Character ch, PurchaseHistoryData historyData)
        {

            if (offer.UseDateRange &&
                (offer.StartDate > DateTime.UtcNow ||
                offer.EndDate < DateTime.UtcNow))
            {
                return;
            }

            if (currentOffers.TryGetValue(offer.StoreSlotTypeId, out StoreOfferType currentOffer) && 
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

            if (offer.MaxCharDaysSinceInstall > 0 && (DateTime.UtcNow - ch.CreationDate).Days > offer.MaxCharDaysSinceInstall)
            {
                return;
            }

            if (offer.MinCharDaysSinceInstall > 0 && (DateTime.UtcNow - ch.CreationDate).Days < offer.MinCharDaysSinceInstall)
            {
                return;
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

            if (offer.MinLevel > 0 && ch.Level < offer.MinLevel)
            {
                return;
            }

            if (offer.MaxLevel > 0 && ch.Level > offer.MaxLevel)
            {
                return;
            }

            currentOffers[offer.StoreSlotTypeId] = offer;
        }
    }
}
