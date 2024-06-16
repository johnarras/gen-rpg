using Genrpg.ServerShared.CloudComms.Servers.PlayerServer.Queues;
using Genrpg.ServerShared.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.PlayerServer.MessageHandlers
{
    public class LoginUserHandler : BasePlayerMessageHandler<LoginUser>
    {
        protected override async Task InnerHandleMessage(LoginUser message)
        {
            _playerService.OnLoginUser(message);
            await Task.CompletedTask;
        }
    }
}
