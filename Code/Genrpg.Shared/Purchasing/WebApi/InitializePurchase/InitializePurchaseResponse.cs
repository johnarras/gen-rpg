using MessagePack;
using Genrpg.Shared.Purchasing.Constants;
using Genrpg.Shared.Website.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Purchasing.WebApi.InitializePurchase
{
    [MessagePackObject]
    public class InitializePurchaseResponse : IWebResponse
    {
        [Key(0)] public EInitializePurchaseStates State { get; set; }
        [Key(1)] public string StoreOfferId { get; set; }
        [Key(2)] public string UniqueStoreItemId { get; set; }
    }
}
