using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Settings;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.PlayerFiltering.Interfaces;
using Genrpg.Shared.PlayerFiltering.Settings;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Purchasing.Settings
{
    [MessagePackObject]
    public class StoreOfferSettings : BaseDataOverrideSettings<StoreOffer>
    {
        [Key(0)] public override string Id { get; set; }
    }

    [MessagePackObject]
    public class StoreOfferSettingsApi : ParentSettingsApi<StoreOfferSettings, StoreOffer> { }
    [MessagePackObject]
    public class StoreOfferSettingsLoader : ParentSettingsLoader<StoreOfferSettings, StoreOffer, StoreOfferSettingsApi> 
    {
        public override bool SendToClient() { return false; }
    }


    [MessagePackObject]
    public class StoreOffer : ChildSettings, IPlayerFilter, IComplexCopy
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string ParentId { get; set; }
        [Key(2)] public long IdKey { get; set; }
        [Key(3)] public override string Name { get; set; }

        [Key(4)] public bool Enabled { get; set; } = true;
        [Key(5)] public string Desc { get; set; }
        [Key(6)] public string Icon { get; set; }
        [Key(7)] public string Art { get; set; }
        [Key(8)] public string OfferId { get; set; } = Guid.NewGuid().ToString();

        [Key(9)] public long TotalModSize { get; set; }
        [Key(10)] public long MaxAcceptableModValue { get; set; }
        [Key(11)] public long Priority { get; set; }
        [Key(12)] public long StoreSlotId { get; set; }
        [Key(13)] public long StoreFeatureId { get; set; }
        [Key(14)] public long StoreThemeId { get; set; }

        [Key(15)] public double MinUserDaysSinceInstall { get; set; }
        [Key(16)] public double MaxUserDaysSinceInstall { get; set; }
        [Key(17)] public double MinCharDaysSinceInstall { get; set; }
        [Key(18)] public double MaxCharDaysSinceInstall { get; set; }
        [Key(19)] public long MinLevel { get; set; }
        [Key(20)] public long MaxLevel { get; set; }
        [Key(21)] public long MinPurchaseCount { get; set; }
        [Key(22)] public long MaxPurchaseCount { get; set; }
        [Key(23)] public double MinPurchaseTotal { get; set; }
        [Key(24)] public double MaxPurchaseTotal { get; set; }
        
        [Key(25)] public bool UseDateRange { get; set; }
        [Key(26)] public DateTime StartDate { get; set; }
        [Key(27)] public DateTime EndDate { get; set; }
        [Key(28)] public int RepeatHours { get; set; }
        [Key(29)] public bool RepeatMonthly { get; set; }

        [Key(30)] public List<OfferProduct> Products { get; set; } = new List<OfferProduct>();

        [Key(31)] public List<AllowedPlayer> AllowedPlayers { get; set; } = new List<AllowedPlayer>();
        public void DeepCopyFrom(IComplexCopy from)
        {
            OfferId = Guid.NewGuid().ToString();
        }

        public object GetDeepCopyData()
        {
            return null;
        }

        public void OrderSelf()
        {

        }
    }

    [MessagePackObject]
    public class OfferProduct
    {
        [Key(0)] public bool Enabled { get; set; } = true;
        [Key(1)] public long Index { get; set; }
        [Key(2)] public long StoreProductId { get; set; }
        [Key(3)] public long ProductSkuId { get; set; }
    }
}
