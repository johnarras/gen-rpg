using Genrpg.ServerShared.CloudComms.Servers.PlayerServer.Queues;
using Genrpg.ServerShared.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.PlayerServer.MessageHandlers
{
    public class LogoutUserHandler : BasePlayerMessageHandler<LogoutUser>
    {
        protected override async Task InnerHandleMessage(LogoutUser message)
        {
            _playerService.OnLogoutUser(message);
            await Task.CompletedTask;
        }
    }
}
