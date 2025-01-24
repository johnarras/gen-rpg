using Genrpg.RequestServer.Purchasing.Entities;
using Genrpg.Shared.Purchasing.Constants;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.RequestServer.Purchasing.ValidationHelpers
{
    using Genrpg.ServerShared.Config;
    using Genrpg.ServerShared.Config.Constants;
    using Genrpg.ServerShared.Secrets.Services;
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using ZstdSharp.Unsafe;

    public class IOSValidationResponse
    {
        public int Status { get; set; }
        public string Environment { get; set; }
        public IOSReceipt Receipt { get; set; }
        public List<object> LatestReceiptInfo { get; set; }
        public string LatestReceipt { get; set; }
        public List<object> PendingRenewalInfo { get; set; }
    }

    public class IOSReceipt
    {
        public string ReceiptType { get; set; }
        public int AdamId { get; set; }
        public int AppItemId { get; set; }
        public string BundleId { get; set; }
        public string ApplicationVersion { get; set; }
        public int DownloadId { get; set; }
        public int VersionExternalIdentifier { get; set; }
        public string ReceiptCreationDate { get; set; }
        public string ReceiptCreationDateMs { get; set; }
        public string ReceiptCreationDatePst { get; set; }
        public string RequestDate { get; set; }
        public string RequestDateMs { get; set; }
        public string RequestDatePst { get; set; }
        public string OriginalPurchaseDate { get; set; }
        public string OriginalPurchaseDateMs { get; set; }
        public string OriginalPurchaseDatePst { get; set; }
        public string OriginalApplicationVersion { get; set; }
        public List<IOSInApp> InApp { get; set; }
    }

    public class IOSInApp
    {
        public string Quantity { get; set; }
        public string ProductId { get; set; }
        public string TransactionId { get; set; }
        public string OriginalTransactionId { get; set; }
        public string PurchaseDate { get; set; }
        public string PurchaseDateMs { get; set; }
        public string PurchaseDatePst { get; set; }
        public string OriginalPurchaseDate { get; set; }
        public string OriginalPurchaseDateMs { get; set; }
        public string OriginalPurchaseDatePst { get; set; }
        public string IsTrialPeriod { get; set; }
        public string IsInIntroOfferPeriod { get; set; }
    }






    public class IOSPurchaseValidationHelper : IPurchaseValidationHelper
    {

        private ISecretsService _secretsService;
        private IServerConfig _serverConfig;

        public EPurchasePlatforms GetKey() { return EPurchasePlatforms.IOS; }
        private const string AppleValidationURL = "https://buy.itunes.apple.com/verifyReceipt"; // Use sandbox URL for testing




        private string _iosSecret;
        private string _packageName;
        private string _buyURL;
        private string _sandboxURL;
        public async Task Initialize(CancellationToken token)
        {
            _iosSecret = await _secretsService.GetSecret(ServerConfigKeys.IOSSecret);
            _packageName = _serverConfig.PackageName;
            _buyURL = _serverConfig.IOSBuyValidationURL;
            _sandboxURL = _serverConfig.IOSSandboxValidationURL;    

            throw new NotImplementedException();
        }


        public async Task<PurchaseValidationResult> ValidatePurchase(string productId, string receiptData)
        {

            using (HttpClient client = new HttpClient())
            {
                var requestPayload = new
                {
                    receiptData,
                    password = _iosSecret
                };

                var content = new StringContent(SerializationUtils.Serialize(requestPayload));

                HttpResponseMessage httpResponse = await client.PostAsync(AppleValidationURL, content);

                if (!httpResponse.IsSuccessStatusCode)
                {
                    throw new Exception("HTTP request failed with status code: " + httpResponse.StatusCode);
                }

                string responseString = await httpResponse.Content.ReadAsStringAsync();

                IOSValidationResponse validationResponse = SerializationUtils.Deserialize<IOSValidationResponse>(responseString);

                int status = validationResponse.Status;

                if (status == 0)
                {
                    return new PurchaseValidationResult() { State = EPurchaseValidationStates.Success };
                }

                PurchaseValidationResult result = new PurchaseValidationResult()
                {
                    ErrorMessage = $"Validation failed with status: {status}",
                    State = EPurchaseValidationStates.Failed
                };

                // Do better error messages here.


                return result;
            }
        }
    }
}
