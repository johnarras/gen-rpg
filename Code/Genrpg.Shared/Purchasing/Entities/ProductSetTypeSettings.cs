using Genrpg.Shared.DataStores.GameSettings;
using Genrpg.Shared.GameSettings.Loading;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Spells.Entities;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Purchasing.Entities
{
    [MessagePackObject]
    public class ProductSetTypeSettings : ParentSettings<ProductSetType>
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override List<ProductSetType> Data { get; set; } = new List<ProductSetType>();

        public ProductSetType GetProductSetType(long idkey) { return _lookup.Get<ProductSetType>(idkey); }
    }

    [MessagePackObject]
    public class ProductSetTypeSettingsApi : ParentSettingsApi<ProductSetTypeSettings, ProductSetType> { }
    [MessagePackObject]
    public class ProductSetTypeSettingsLoader : ParentSettingsLoader<ProductSetTypeSettings, ProductSetType, ProductSetTypeSettingsApi> { }


    [MessagePackObject]
    public class ProductSetType : ChildSettings, IIdName
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string ParentId { get; set; }
        [Key(2)] public long IdKey { get; set; }
        [Key(3)] public override string Name { get; set; }
        [Key(4)] public List<ProductSetItem> StoreProducts { get; set; }
    }


    [MessagePackObject]
    public class ProductSetItem
    {
        [Key(0)] public long StoreProductTypeId { get; set; }
    }
}
