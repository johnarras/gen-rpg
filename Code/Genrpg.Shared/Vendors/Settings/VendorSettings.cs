using MessagePack;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;

namespace Genrpg.Shared.Vendors.Settings
{
    [MessagePackObject]
    public class VendorSettings : BaseGameSettings // No List
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public float BuyFromVendorPriceMult { get; set; }
        [Key(2)] public float SellToVendorPriceMult { get; set; }
        [Key(3)] public float VendorRefreshMinutes { get; set; }
    }

    [MessagePackObject]
    public class VendorSettingsLoader : GameSettingsLoader<VendorSettings> { }
}
