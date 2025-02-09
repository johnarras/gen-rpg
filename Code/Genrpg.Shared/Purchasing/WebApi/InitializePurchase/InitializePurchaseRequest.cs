using MessagePack;
using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Website.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Purchasing.WebApi.InitializePurchase
{
    [MessagePackObject]
    public class InitializePurchaseRequest : IClientUserRequest
    {
        [Key(0)] public string StoreOfferId { get; set; }
        [Key(1)] public string UniqueStoreItemId { get; set; }
    }
}
