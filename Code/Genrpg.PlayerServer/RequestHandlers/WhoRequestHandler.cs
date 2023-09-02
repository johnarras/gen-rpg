using Genrpg.PlayerServer.MessageHandlers;
using Genrpg.ServerShared.CloudMessaging.Requests;
using Genrpg.ServerShared.CloudMessaging.Servers.PlayerServer.Requests;
using Genrpg.ServerShared.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.PlayerServer.RequestHandlers
{
    public class WhoRequestHandler : BasePlayerRequestHandler<WhoListRequest>
    {
        protected override async Task<ICloudResponse> InnerHandleRequest(ServerGameState gs, WhoListRequest message)
        {
            return _playerService.GetWhoList(gs, message);

            await Task.CompletedTask;
        }
    }
}
