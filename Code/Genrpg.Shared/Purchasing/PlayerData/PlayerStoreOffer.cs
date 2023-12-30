using Genrpg.Shared.Purchasing.Settings;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Purchasing.PlayerData
{
    [MessagePackObject]
    public class PlayerStoreOffer
    {
        [Key(0)] public long IdKey { get; set; }
        [Key(1)] public string OfferId { get; set; }
        [Key(2)] public string UniqueId { get; set; }
        [Key(3)] public string Name { get; set; }
        [Key(4)] public string Desc { get; set; }
        [Key(5)] public string Art { get; set; }
        [Key(6)] public string Icon { get; set; }
        [Key(7)] public long StoreFeatureId { get; set; }
        [Key(8)] public long StoreSlotId { get; set; }    
        [Key(9)] public long StoreThemeId { get; set; }
        [Key(10)] public DateTime EndDate { get; set; }

        [Key(11)] public List<PlayerOfferProduct> Products { get; set; } = new List<PlayerOfferProduct>();
    }
}
