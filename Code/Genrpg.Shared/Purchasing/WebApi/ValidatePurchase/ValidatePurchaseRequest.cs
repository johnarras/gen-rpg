using Genrpg.Shared.Purchasing.Constants;
using Genrpg.Shared.Website.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Purchasing.WebApi.ValidatePurchase
{
    public class ValidatePurchaseRequest : IClientUserRequest
    {
        public string StoreOfferId { get; set; }
        public string StoreItemId { get; set; }
        public string ReceiptData { get; set; }
        public string ProductSkuId { get; set; }
        public EPurchasePlatforms Platform { get; set; }
    }
}
