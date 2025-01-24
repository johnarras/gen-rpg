using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Purchasing.Constants
{
    public enum EInitializePurchaseStates
    {
        Failed = 0,
        Success = 1,
        MissingStoreOffer = 2,
        MissingOfferProduct = 3,
        MissingPlayerStoreOffer = 4,
        OfferIsAlreadyInitialized=5,
        MissingPlayerStoreOfferItem = 6,
        MissingOfferItemSku = 7,
        MissingGameDataSku = 8,
        MissingPlayerStoreItem=9,
    }
}
