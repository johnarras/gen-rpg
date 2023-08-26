using MessagePack;
using Genrpg.Shared.DataStores.Core;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.Units.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using Genrpg.Shared.Spells.Entities;

namespace Genrpg.Shared.Currencies.Entities
{
    /// <summary>
    /// Used to contain a list of currencies on objects that need it (like user and character)
    /// </summary>

    [MessagePackObject]
    public class CurrencyData : IdObjectList<CurrencyStatus>
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override List<CurrencyStatus> Data { get; set; } = new List<CurrencyStatus>();
        public override void AddTo(Unit unit) { unit.Set(this); }
        protected override bool CreateIfMissingOnGet()
        {
            return true;
        }
        public override void Delete(IRepositorySystem repoSystem) { repoSystem.Delete(this); }
        public long GetQuantity(long currencyTypeId)
        {
            return Get(currencyTypeId).Quantity;
        }

        public bool Add(long currencyTypeId, long quantity)
        {
            return Set(currencyTypeId, GetQuantity(currencyTypeId) + quantity);
        }

        public bool Set(long currencyTypeId, long newQuantity)
        {
            if (newQuantity < 0)
            {
                newQuantity = 0;
            }

            CurrencyStatus status = Get(currencyTypeId);
            long oldQuantity = Math.Max(0, status.Quantity);
            status.Quantity = newQuantity;
            return true;
        }
    }
}
