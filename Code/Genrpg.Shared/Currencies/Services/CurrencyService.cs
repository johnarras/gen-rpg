
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Currencies.Entities;
using Genrpg.Shared.Units.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Currencies.Services
{
    public class CurrencyService : ICurrencyService
    {
        public bool Add(GameState gs, Unit unit, long currencyTypeId, long quantity)
        {
            return Set(gs, unit, currencyTypeId, unit.Get<CurrencyData>().GetQuantity(currencyTypeId) + quantity);
        }

        public bool Set(GameState gs, Unit unit, long currencyTypeId, long newQuantity)
        {
            if (newQuantity < 0)
            {
                newQuantity = 0;
            }
            CurrencyData currencyData = unit.Get<CurrencyData>();
            CurrencyStatus status = currencyData.Get(currencyTypeId);
            long oldQuantity = Math.Max(0, status.Quantity);
            status.Quantity = newQuantity;

            long diff = newQuantity - oldQuantity;

            OnSetCurrency(gs, unit, currencyData, status, diff);
            return true;
        }

        protected virtual void OnSetCurrency(GameState gs, Unit unit, CurrencyData currencyData, CurrencyStatus status, long diff)
        {
            if (diff != 0)
            {
                gs.repo.QueueSave(status);
            }
        }
    }
}
