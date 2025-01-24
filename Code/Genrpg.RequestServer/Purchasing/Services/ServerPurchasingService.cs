using Genrpg.RequestServer.Core;
using Genrpg.RequestServer.Purchasing.Entities;
using Genrpg.RequestServer.Purchasing.ValidationHelpers;
using Genrpg.ServerShared.Core;
using Genrpg.ServerShared.Crypto.Services;
using Genrpg.ServerShared.GameSettings.Services;
using Genrpg.Shared.Accounts.PlayerData;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.DataStores.Indexes;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.HelperClasses;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Logging.Interfaces;
using Genrpg.Shared.PlayerFiltering.Interfaces;
using Genrpg.Shared.Purchasing.Constants;
using Genrpg.Shared.Purchasing.PlayerData;
using Genrpg.Shared.Purchasing.Settings;
using Genrpg.Shared.Purchasing.WebApi.InitializePurchase;
using Genrpg.Shared.Purchasing.WebApi.ValidatePurchase;
using Genrpg.Shared.Users.PlayerData;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Versions.Settings;

namespace Genrpg.RequestServer.Purchasing.Services
{
    public interface IServerPurchasingService : IInitializable
    {
        Task<PlayerStoreOfferData> GetCurrentStores(User user, bool forceRefresh);
        Task InitializePurchase(WebContext context, InitializePurchaseRequest request);
        Task ValidateReceipt(WebContext context, ValidatePurchaseRequest request);

    }
    public class ServerPurchasingService : IServerPurchasingService
    {
        protected IRepositoryService _repoService = null;
        private IGameData _gameData = null;
        private IGameDataService _gameDataService = null;
        private ILogService _logService = null;
        private ICryptoService _cryptoService;

        private SetupDictionaryContainer<EPurchasePlatforms, IPurchaseValidationHelper> _validationHelpers = new SetupDictionaryContainer<EPurchasePlatforms, IPurchaseValidationHelper>();

        // Try to connect to apple and google?
        public async Task Initialize(CancellationToken token)
        {

            CreateIndexData data = new CreateIndexData();
            data.Configs.Add(new IndexConfig() { MemberName = nameof(CompletedPurchaseData.ReceiptHash)});
            await _repoService.CreateIndex<Account>(data);
        }

        #region GetStores
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
            StoreItemSettings productSettings = _gameData.Get<StoreItemSettings>(user);

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

                List<OfferItem> validItems = storeOffer.Products.Where(x => x.Enabled).ToList();

                if (validItems.Count < 1)
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
                };

                for (int p = 0; p < validItems.Count; p++)
                {
                    OfferItem offerProduct = validItems[p];
                    StoreItem storeProduct = productSettings.Get(offerProduct.StoreProductId);
                    ProductSku sku = skuSettings.Get(offerProduct.ProductSkuId);

                    if (storeProduct != null && sku != null)
                    {
                        PlayerStoreOfferItem playerOfferProduct = new PlayerStoreOfferItem()
                        {
                            Index = p,
                            StoreItem = storeProduct,
                            Sku = sku,
                            UniqueStoreItemId = HashUtils.NewGuid(),
                        };
                        playerStoreOffer.Items.Add(playerOfferProduct);
                    }
                }

                if (playerStoreOffer.Items.Count > 0)
                {
                    storeOfferData.StoreOffers.Add(playerStoreOffer);
                }
            }

            storeOfferData.GameDataSaveTime = versionSettings.GameDataSaveTime;
            storeOfferData.LastTimeSet = currentTime;

            await _repoService.Save(storeOfferData);

            return storeOfferData;
        }

        protected void TryAddOffer(StoreOffer offer, Dictionary<long, StoreOffer> currentOffers, IFilteredObject user, PurchaseHistoryData historyData)
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
                if (offer.AllowedPlayers.Any(x => x.PlayerId == user.Id))
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

        #endregion

        #region InitializePurchase


        public async Task InitializePurchase(WebContext context, InitializePurchaseRequest request)
        {

            CurrentPurchaseData purchaseData = await context.GetAsync<CurrentPurchaseData>();

            PlayerStoreOfferData offerData = await context.GetAsync<PlayerStoreOfferData>();

            PlayerStoreOffer playerStoreOffer = offerData.StoreOffers.FirstOrDefault(x => x.OfferId == request.StoreOfferId);

            if (playerStoreOffer == null)
            {
                CreatePurchaseIntentErrorResponse(context, EInitializePurchaseStates.MissingPlayerStoreOffer);
                return;
            }

            StoreOfferSettings offerSettings = _gameData.Get<StoreOfferSettings>(context.user);

            StoreOffer storeOffer = offerSettings.GetData().FirstOrDefault(x => x.OfferId == request.StoreOfferId);

            if (storeOffer == null)
            {
                CreatePurchaseIntentErrorResponse(context, EInitializePurchaseStates.MissingStoreOffer);
                return;
            }

            if (purchaseData.StoreOffer != null && purchaseData.StoreOffer.OfferId == playerStoreOffer.OfferId)
            {
                CreatePurchaseIntentErrorResponse(context, EInitializePurchaseStates.OfferIsAlreadyInitialized);
                return;
            }

            if (purchaseData.StoreOffer != null)
            {
                _logService.Info(context.user.Id + " replaced offer with OfferId " + purchaseData.StoreOffer.OfferId + " with " +
                    playerStoreOffer.OfferId); 
            }

            PlayerStoreOfferItem playerStoreOfferItem = playerStoreOffer.Items.FirstOrDefault(x => x.UniqueStoreItemId == request.UniqueStoreItemId);

            if (playerStoreOfferItem == null)
            {
                CreatePurchaseIntentErrorResponse(context, EInitializePurchaseStates.MissingPlayerStoreOfferItem);
                return;
            }

            if (playerStoreOfferItem.StoreItem == null)
            {
                CreatePurchaseIntentErrorResponse(context, EInitializePurchaseStates.MissingPlayerStoreItem);
                return;
            }
            
            if (playerStoreOfferItem.Sku == null)
            {
                CreatePurchaseIntentErrorResponse(context, EInitializePurchaseStates.MissingOfferItemSku);
                return;
            }

            ProductSku currentSku = _gameData.Get<ProductSkuSettings>(context.user).Get(playerStoreOfferItem.Sku.IdKey);

            if (currentSku == null)
            {
                CreatePurchaseIntentErrorResponse(context, EInitializePurchaseStates.MissingGameDataSku);
                return;
            }


            // Have Store item and sku set up and it matches, so set this up to be ok.

            purchaseData.StoreOffer = playerStoreOffer;
            purchaseData.StoreItem = playerStoreOfferItem;
            purchaseData.State = ECurrentPurchaseStates.Initialized;

            CreatePurchaseIntentSuccessResponse(context, purchaseData); 


            await Task.CompletedTask;
        }

        private void CreatePurchaseIntentSuccessResponse(WebContext context, CurrentPurchaseData purchaseData)
        {
            context.Responses.Add(new InitializePurchaseResponse()
            {
                State = EInitializePurchaseStates.Success,
                StoreOfferId = purchaseData.StoreOffer.OfferId,
                UniqueStoreItemId = purchaseData.StoreItem.UniqueStoreItemId,
            });
        }

        private void CreatePurchaseIntentErrorResponse(WebContext context, EInitializePurchaseStates response)
        {
            context.Responses.Add(new InitializePurchaseResponse() { State = response });
        }

        #endregion


        #region ValidatePurchase


        private void CreateValidationErrorResponse(WebContext context, EPurchaseValidationStates state, string errorMessage = null)            
        {
            context.Responses.Add(new ValidatePurchaseResponse() { ErrorMessage = errorMessage, State = state });
        }

        public async Task ValidateReceipt(WebContext context, ValidatePurchaseRequest request)
        {

            if (string.IsNullOrEmpty(request.ReceiptData))
            {
                CreateValidationErrorResponse(context, EPurchaseValidationStates.NoReceipt);
                return;
            }

            string hashedReceipt = _cryptoService.SlowHash(request.ReceiptData);

            List<CompletedPurchaseData> allCompleted = await _repoService.Search<CompletedPurchaseData>(x=>x.ReceiptHash == hashedReceipt);

            if (allCompleted.Any(x => x.ReceiptData == request.ReceiptData))
            {
                CreateValidationErrorResponse(context, EPurchaseValidationStates.ReceiptHasBeenValidated);
                return;
            }

            PurchaseValidationResult result = null;

            if (_validationHelpers.TryGetValue(request.Platform, out IPurchaseValidationHelper helper))
            {
                result = await helper.ValidatePurchase(request.ProductSkuId, request.ReceiptData);
            }
            else
            {
                CreateValidationErrorResponse(context, EPurchaseValidationStates.InvalidPlatform);
                return;
            }

            if (result.State != EPurchaseValidationStates.Success)
            {
                CreateValidationErrorResponse(context, result.State, result.ErrorMessage);
                return;
            }


            await Task.CompletedTask;
        }


        #endregion
    }
}
