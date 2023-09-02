using MessagePack;

using Genrpg.Shared.DataStores.Categories;
using Genrpg.Shared.GameSettings;

namespace Genrpg.Shared.NPCs.Entities
{
    [MessagePackObject]
    public class VendorSettings : BaseGameData
    {
        public override void Set(GameData gameData) { gameData.Set(this); }
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public float BuyFromVendorPriceMult { get; set; }
        [Key(2)] public float SellToVendorPriceMult { get; set; }
        [Key(3)] public float VendorRefreshMinutes { get; set; }
    }
}
