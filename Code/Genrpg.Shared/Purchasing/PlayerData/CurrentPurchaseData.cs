using MessagePack;
using Genrpg.Shared.DataStores.Categories.PlayerData;
using Genrpg.Shared.DataStores.PlayerData;
using Genrpg.Shared.Units.Loaders;
using System;
using System.Collections.Generic;
using System.Text;
using Genrpg.Shared.Purchasing.Settings;

namespace Genrpg.Shared.Purchasing.PlayerData
{

    public class CurrentPurchaseLoader : UnitDataLoader<CurrentPurchaseData>
    {
        protected override bool IsUserData() { return true; }
    }

    [MessagePackObject]
    public class CurrentPurchaseData : BasePlayerData, IUnitData
    {

        [Key(0)] public override string Id { get; set; }

        [Key(1)] public PlayerStoreOffer CurrentOffer { get; set; }

        [Key(2)] public PlayerOfferProduct CurrentProduct { get; set; }
    }
}
