using Genrpg.MapServer.MapMessaging.MessageHandlers;
using Genrpg.MapServer.Trades.Services;
using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.Shared.Trades.Messages;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.MapServer.Trades.MessageHandlers
{
    public class UpdateTradeHandler : BaseCharacterServerMapMessageHandler<UpdateTrade>
    {
        private ITradeService _tradeService = null;
        protected override void InnerProcess(IRandom rand, MapMessagePackage pack, Character ch, UpdateTrade message)
        {
            _tradeService.HandleUpdateTrade(ch, message);
        }
    }
}
