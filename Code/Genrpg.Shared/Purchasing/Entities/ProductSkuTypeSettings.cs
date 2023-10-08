using Genrpg.Shared.DataStores.GameSettings;
using Genrpg.Shared.GameSettings.Loading;
using Genrpg.Shared.Interfaces;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Purchasing.Entities
{
    [MessagePackObject]
    public class ProductSkuTypeSettings : ParentSettings<ProductSkuType>
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override List<ProductSkuType> Data { get; set; } = new List<ProductSkuType>();

        public ProductSkuType GetProductSkuType(long idkey) { return _lookup.Get<ProductSkuType>(idkey); }
    }

    [MessagePackObject]
    public class ProductSkuTypeSettingsApi : ParentSettingsApi<ProductSkuTypeSettings, ProductSkuType> { }
    [MessagePackObject]
    public class ProductSkuTypeSettingsLoader : ParentSettingsLoader<ProductSkuTypeSettings, ProductSkuType, ProductSkuTypeSettingsApi> { }


    [MessagePackObject]
    public class ProductSkuType : ChildSettings, IIdName
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string ParentId { get; set; }
        [Key(2)] public long IdKey { get; set; }
        [Key(3)] public override string Name { get; set; }
        [Key(4)] public double DollarPrice { get; set; }
        [Key(5)] public long GemPrice { get; set; }
        [Key(6)] public string GoogleProductId { get; set; }
        [Key(7)] public string AppleProductId { get; set; }
    }
}
