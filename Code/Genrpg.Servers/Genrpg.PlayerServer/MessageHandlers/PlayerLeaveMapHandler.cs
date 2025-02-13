﻿using Genrpg.ServerShared.CloudComms.Servers.PlayerServer.Queues;
using Genrpg.ServerShared.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.PlayerServer.MessageHandlers
{
    public class PlayerLeaveMapHandler : BasePlayerMessageHandler<PlayerLeaveMap>
    {
        protected override async Task InnerHandleMessage(PlayerLeaveMap message)
        {
            _playerService.OnPlayerLeaveMap(message);
            await Task.CompletedTask;
        }
    }
}
