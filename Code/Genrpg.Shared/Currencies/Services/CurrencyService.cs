
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Currencies.PlayerData;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.Logging.Interfaces;
using Genrpg.Shared.Units.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Genrpg.Shared.Currencies.Services
{
    public class CurrencyService : ICurrencyService
    {
        protected IRepositoryService _repoService = null;

        public bool Add(Unit unit, long currencyTypeId, long quantity)
        {
            return Set(unit, currencyTypeId, unit.Get<CurrencyData>().GetQuantity(currencyTypeId) + quantity);
        }

        public bool Set(Unit unit, long currencyTypeId, long newQuantity)
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

            OnSetCurrency(unit, currencyData, status, diff);
            return true;
        }

        protected virtual void OnSetCurrency(Unit unit, CurrencyData currencyData, CurrencyStatus status, long diff)
        {
            if (diff != 0)
            {
                _repoService.QueueSave(status);
            }
        }
    }
}
