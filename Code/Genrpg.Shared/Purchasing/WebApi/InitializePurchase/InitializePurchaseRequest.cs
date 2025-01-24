using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Website.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Purchasing.WebApi.InitializePurchase
{
    public class InitializePurchaseRequest : IClientUserRequest
    {
        public string StoreOfferId { get; set; }
        public string UniqueStoreItemId { get; set; }
    }
}
