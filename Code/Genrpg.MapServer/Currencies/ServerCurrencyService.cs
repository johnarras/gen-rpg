﻿using Genrpg.MapServer.MapMessaging.Interfaces;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Currencies.Entities;
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

namespace Genrpg.MapServer.Currencies
{
    public class ServerCurrencyService : CurrencyService, ISetupService
    {
        private IMapMessageService _messageService;
        public async Task Setup(GameState gs, CancellationToken token)
        {
            await Task.CompletedTask;
        }

        protected override void OnSetCurrency(GameState gs, Unit unit, CurrencyData currencyData, long currencyTypeId, long diff)
        {
            if (diff == 0)
            {
                return;
            }

            currencyData.SetDirty(true);

            OnAddCurrency onAdd = new OnAddCurrency()
            {
                CharId = unit.Id,
                CurrencyTypeId = currencyTypeId,
                QuantityAdded = diff,
            };

            _messageService.SendMessage(gs, unit, onAdd);
        }
    }
}