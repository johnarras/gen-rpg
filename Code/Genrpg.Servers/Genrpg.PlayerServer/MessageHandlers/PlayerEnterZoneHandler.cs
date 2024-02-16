using Genrpg.ServerShared.CloudComms.Servers.PlayerServer.Queues;
using Genrpg.ServerShared.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.PlayerServer.MessageHandlers
{
    public class PlayerEnterZoneHandler : BasePlayerMessageHandler<PlayerEnterZone>
    {
        protected override async Task InnerHandleMessage(ServerGameState gs, PlayerEnterZone message)
        {
            _playerService.OnPlayerEnterZone(gs, message);
            await Task.CompletedTask;
        }
    }
}
