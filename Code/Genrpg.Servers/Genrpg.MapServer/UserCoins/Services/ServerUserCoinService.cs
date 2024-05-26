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
using Genrpg.Shared.UserCoins.Messages;
using Genrpg.Shared.UserCoins.PlayerData;
using Genrpg.Shared.UserCoins.Services;
using Genrpg.Shared.DataStores.Entities;

namespace Genrpg.MapServer.UserCoins.Services
{
    public class ServerUserCoinService : UserCoinService, IInitializable
    {
        protected IRepositoryService _repoService = null;
        private IMapMessageService _messageService = null;
        public async Task Initialize(GameState gs, CancellationToken token)
        {
            await Task.CompletedTask;
        }

        protected override void OnSetUserCoin(GameState gs, Unit unit, UserCoinData userCoinData, UserCoinStatus status, long diff)
        {
            if (diff == 0)
            {
                return;
            }

            _repoService.QueueSave(status);

            OnAddUserCoin onAdd = new OnAddUserCoin()
            {
                CharId = unit.Id,
                UserCoinTypeId = status.IdKey,
                QuantityAdded = diff,
            };

            _messageService.SendMessage(unit, onAdd);
        }
    }
}
