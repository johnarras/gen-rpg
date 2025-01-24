using Genrpg.Shared.Purchasing.Constants;
using Genrpg.Shared.Rewards.Entities;
using Genrpg.Shared.Website.Interfaces;

namespace Genrpg.Shared.Purchasing.WebApi.ValidatePurchase
{
    public class ValidatePurchaseResponse : IWebResponse
    {
        public EPurchaseValidationStates State { get; set; }

        public string ErrorMessage { get; set; }

        public RewardData Rewards { get; set; }
    }
}
