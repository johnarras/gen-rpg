﻿using Genrpg.MapServer.MapMessaging.MessageHandlers;
using Genrpg.MapServer.Trades.Services;
using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.Shared.Trades.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.MapServer.Trades.MessageHandlers
{
    public class OnUpdateTradeHandler : BaseServerMapMessageHandler<OnUpdateTrade>
    {
        private ITradeService _tradeService;
        protected override void InnerProcess(GameState gs, MapMessagePackage pack, MapObject obj, OnUpdateTrade message)
        {
            if (_objectManager.GetChar(obj.Id, out Character ch))
            {
                _tradeService.HandleOnUpdateTrade(ch, message);
            }
        }
    }
}