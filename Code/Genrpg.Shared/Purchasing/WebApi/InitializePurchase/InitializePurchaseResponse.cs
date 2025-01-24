using Genrpg.Shared.Purchasing.Constants;
using Genrpg.Shared.Website.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Purchasing.WebApi.InitializePurchase
{
    public class InitializePurchaseResponse : IWebResponse
    {
        public EInitializePurchaseStates State { get; set; }
        public string StoreOfferId { get; set; }
        public string UniqueStoreItemId { get; set; }
    }
}
