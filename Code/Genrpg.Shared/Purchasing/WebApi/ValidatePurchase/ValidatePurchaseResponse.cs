using MessagePack;
using Genrpg.Shared.Purchasing.Constants;
using Genrpg.Shared.Rewards.Entities;
using Genrpg.Shared.Website.Interfaces;

namespace Genrpg.Shared.Purchasing.WebApi.ValidatePurchase
{
    [MessagePackObject]
    public class ValidatePurchaseResponse : IWebResponse
    {
        [Key(0)] public EPurchaseValidationStates State { get; set; }

        [Key(1)] public string ErrorMessage { get; set; }

        [Key(2)] public RewardData Rewards { get; set; }
    }
}
