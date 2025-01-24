using Genrpg.RequestServer.Purchasing.Entities;
using Genrpg.Shared.Purchasing.Constants;

namespace Genrpg.RequestServer.Purchasing.ValidationHelpers
{
    public class EditorPurchaseValidationHelper : IPurchaseValidationHelper
    {
        public EPurchasePlatforms GetKey() { return EPurchasePlatforms.Editor; }

        public Task Initialize(CancellationToken token)
        {
            return Task.CompletedTask;
        }

        public async Task<PurchaseValidationResult> ValidatePurchase(string productId, string uniquePurchaseId)
        {
            await Task.CompletedTask;

            return new PurchaseValidationResult()
            {
                State = EPurchaseValidationStates.Success,
            };


        }
    }
}
