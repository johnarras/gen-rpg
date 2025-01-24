using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Purchasing.Constants
{
    public enum EPurchaseValidationStates
    {
        Failed = 0,
        Success = 1,
        MissingStoreOffer = 2,
        MissingOfferProduct = 3,
        MissingPlayerStoreOffer = 4,
        ReceiptHasBeenValidated = 5,
        MissingPlayerStoreOfferItem = 6,
        MissingOfferItemSku = 7,
        MissingGameDataSku = 8,
        MissingPlayerStoreItem = 9,
        InvalidPlatform = 10,
        NoReceipt = 11,
    }
}
