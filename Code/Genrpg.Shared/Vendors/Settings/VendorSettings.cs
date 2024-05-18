using MessagePack;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Interfaces;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.Core.Settings;
using Genrpg.Shared.GameSettings.Mappers;

namespace Genrpg.Shared.Vendors.Settings
{
    [MessagePackObject]
    public class VendorSettings : NoChildSettings // No List
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public float BuyFromVendorPriceMult { get; set; }
        [Key(2)] public float SellToVendorPriceMult { get; set; }
        [Key(3)] public float VendorRefreshMinutes { get; set; }
    }

    [MessagePackObject]
    public class VendorSettingsLoader : NoChildSettingsLoader<VendorSettings> { }



    [MessagePackObject]
    public class VendorettingsMapper : NoChildSettingsMapper<VendorSettings> { }
}
