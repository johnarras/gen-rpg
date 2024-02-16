using Genrpg.Shared.DataStores.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.Interfaces;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Purchasing.Settings
{
    [MessagePackObject]
    public class ProductSkuSettings : ParentSettings<ProductSku>
    {
        [Key(0)] public override string Id { get; set; }
    }

    [MessagePackObject]
    public class ProductSkuSettingsApi : ParentSettingsApi<ProductSkuSettings, ProductSku> { }
    [MessagePackObject]
    public class ProductSkuSettingsLoader : ParentSettingsLoader<ProductSkuSettings, ProductSku, ProductSkuSettingsApi> { }


    [MessagePackObject]
    public class ProductSku : ChildSettings, IIdName
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
