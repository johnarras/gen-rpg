using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Purchasing.Entities
{
    [MessagePackObject]
    public class ClientStoreOffer
    {
        [Key(0)] public long IdKey { get; set; }
        [Key(1)] public string UniqueId { get; set; }
        [Key(2)] public string Name { get; set; }
        [Key(3)] public string Desc { get; set; }
        [Key(4)] public string Art { get; set; }
        [Key(5)] public string Icon { get; set; }
        [Key(6)] public long StoreFeatureTypeId { get; set; }
        [Key(7)] public long StoreSlotTypeId { get; set; }
     

        [Key(8)] public List<OfferProduct> Products { get; set; }
    }
}
