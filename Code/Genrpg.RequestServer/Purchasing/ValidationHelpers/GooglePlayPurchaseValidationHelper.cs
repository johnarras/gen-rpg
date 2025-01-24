using Genrpg.RequestServer.Purchasing.Entities;
using Genrpg.ServerShared.Config;
using Genrpg.ServerShared.Config.Constants;
using Genrpg.ServerShared.Secrets.Services;
using Genrpg.Shared.Constants;
using Genrpg.Shared.Purchasing.Constants;
using Google.Apis.AndroidPublisher.v3;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using System.Text;

namespace Genrpg.RequestServer.Purchasing.ValidationHelpers
{
    public class GooglePlayPurchaseValidationHelper : IPurchaseValidationHelper
    {
        private ISecretsService _secretsService;
        private IServerConfig _serverConfig;


        public EPurchasePlatforms GetKey() { return EPurchasePlatforms.GooglePlay; }

        private string _packageName;
        private AndroidPublisherService _publisherService;
        public async Task Initialize(CancellationToken token)
        {

            _packageName = _serverConfig.PackageName;
            GoogleCredential credential = GoogleCredential.FromStream(new MemoryStream(Encoding.ASCII.GetBytes(await _secretsService.GetSecret(ServerConfigKeys.GooglePlaySecret))));
            _publisherService = new AndroidPublisherService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = Game.Prefix,
            });
            await Task.CompletedTask;
        }

        public async Task<PurchaseValidationResult> ValidatePurchase(string productId, string purchaseToken)
        {

            try
            {
                var purchase = await _publisherService.Purchases.Products.Get(_packageName, productId, purchaseToken).ExecuteAsync();
                if (purchase.PurchaseState == 0)
                {
                    return new PurchaseValidationResult()
                    {
                        State = EPurchaseValidationStates.Success,
                    };
                }
                else
                {
                    PurchaseValidationResult result = new PurchaseValidationResult()
                    {
                        State = EPurchaseValidationStates.Failed,
                        ErrorMessage = $"Validation failed with purchase state: {purchase.PurchaseState}"
                    };
                    return result;
                }
            }
            catch (Google.GoogleApiException ex)
            {
                PurchaseValidationResult result = new PurchaseValidationResult()
                {
                    State = EPurchaseValidationStates.Failed,
                    ErrorMessage = $"Google API error: {ex.Message}"
                };

                return result;
            }
            catch (Exception ex)
            {
                PurchaseValidationResult result = new PurchaseValidationResult()
                {
                    State = EPurchaseValidationStates.Failed,
                    ErrorMessage = $"Unexpected error: {ex.Message}"
                };
                return result;
            }
        }
    }
}