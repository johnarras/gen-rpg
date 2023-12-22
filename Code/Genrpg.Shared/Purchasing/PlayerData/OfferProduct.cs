using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Purchasing.PlayerData
{
    [MessagePackObject]
    public class OfferProduct
    {
        [Key(0)] public long Index { get; set; }
        [Key(1)] public long StoreProductTypeId { get; set; }
        [Key(2)] public long ProductSkuTypeId { get; set; }
    }
}
