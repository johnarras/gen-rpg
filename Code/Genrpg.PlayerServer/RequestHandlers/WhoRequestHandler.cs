using Genrpg.PlayerServer.MessageHandlers;
using Genrpg.ServerShared.CloudComms.Requests.Entities;
using Genrpg.ServerShared.CloudComms.Servers.PlayerServer.Requests;
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
        protected override async Task<IResponse> InnerHandleRequest(ServerGameState gs, WhoListRequest message, ResponseEnvelope envelope)
        {
            await Task.CompletedTask;
            return _playerService.GetWhoList(gs, message);

        }
    }
}
