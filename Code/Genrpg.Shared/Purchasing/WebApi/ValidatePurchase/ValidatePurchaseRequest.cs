using MessagePack;
using Genrpg.Shared.Purchasing.Constants;
using Genrpg.Shared.Website.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Purchasing.WebApi.ValidatePurchase
{
    [MessagePackObject]
    public class ValidatePurchaseRequest : IClientUserRequest
    {
        [Key(0)] public string StoreOfferId { get; set; }
        [Key(1)] public string StoreItemId { get; set; }
        [Key(2)] public string ReceiptData { get; set; }
        [Key(3)] public string ProductSkuId { get; set; }
        [Key(4)] public EPurchasePlatforms Platform { get; set; }
    }
}
