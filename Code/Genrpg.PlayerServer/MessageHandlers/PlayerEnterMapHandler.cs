using Genrpg.ServerShared.CloudComms.Servers.PlayerServer.Queues;
using Genrpg.ServerShared.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.PlayerServer.MessageHandlers
{
    public class PlayerEnterMapHandler : BasePlayerMessageHandler<PlayerEnterMap>
    {
        protected override async Task InnerHandleMessage(ServerGameState gs, PlayerEnterMap message)
        {
            _playerService.OnPlayerEnterMap(gs, message);
            await Task.CompletedTask;
        }
    }
}
