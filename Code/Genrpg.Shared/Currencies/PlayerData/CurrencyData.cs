using MessagePack;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.Units.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using Genrpg.Shared.Spells.Casting;
using Genrpg.Shared.DataStores.PlayerData;
using Genrpg.Shared.DataStores.Categories.PlayerData;
using Genrpg.Shared.Interfaces;

namespace Genrpg.Shared.Currencies.PlayerData
{
    /// <summary>
    /// Used to contain a list of currencies on objects that need it (like user and character)
    /// </summary>

    [MessagePackObject]
    public class CurrencyData : OwnerIdObjectList<CurrencyStatus>
    {
        [Key(0)] public override string Id { get; set; }

        public long GetQuantity(long currencyTypeId)
        {
            return Get(currencyTypeId).Quantity;
        }

    }
    [MessagePackObject]
    public class CurrencyStatus : OwnerPlayerData, IId
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string OwnerId { get; set; }
        [Key(2)] public long IdKey { get; set; }
        [Key(3)] public long Quantity { get; set; }

    }

    [MessagePackObject]
    public class CurrencyApi : OwnerApiList<CurrencyData, CurrencyStatus> { }
}
