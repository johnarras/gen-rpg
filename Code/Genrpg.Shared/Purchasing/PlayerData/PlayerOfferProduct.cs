using Genrpg.Shared.Purchasing.Settings;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Purchasing.PlayerData
{
    [MessagePackObject]
    public class PlayerOfferProduct
    {
        [Key(0)] public long Index { get; set; }
        [Key(1)] public StoreProduct Product { get; set; }
        [Key(2)] public ProductSku Sku { get; set; }
    }
}
