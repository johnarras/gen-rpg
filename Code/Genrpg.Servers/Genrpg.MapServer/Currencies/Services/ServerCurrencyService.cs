using Genrpg.MapServer.MapMessaging.Interfaces;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Currencies.PlayerData;
using Genrpg.Shared.Currencies.Messages;
using Genrpg.Shared.Currencies.Services;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Units.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Genrpg.Shared.DataStores.Entities;

namespace Genrpg.MapServer.Currencies.Services
{
    public class ServerCurrencyService : CurrencyService, IInitializable
    {
        private IMapMessageService _messageService = null;

        protected override void OnSetCurrency(Unit unit, CurrencyData currencyData, CurrencyStatus status, long diff)
        {
            if (diff == 0)
            {
                return;
            }

            _repoService.QueueSave(status);

            OnAddCurrency onAdd = new OnAddCurrency()
            {
                CharId = unit.Id,
                CurrencyTypeId = status.IdKey,
                QuantityAdded = diff,
            };

            _messageService.SendMessage(unit, onAdd);
        }
    }
}
