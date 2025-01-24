using Genrpg.RequestServer.Purchasing.Entities;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Purchasing.Constants;

namespace Genrpg.RequestServer.Purchasing.ValidationHelpers
{
    public interface IPurchaseValidationHelper : ISetupDictionaryItem<EPurchasePlatforms>, IInitializable
    {
        Task<PurchaseValidationResult> ValidatePurchase(string productId, string receiptData);
    }
}
