using Genrpg.RequestServer.ClientUser.RequestHandlers;
using Genrpg.RequestServer.Core;
using Genrpg.RequestServer.PlayMultiplier.Services;
using Genrpg.Shared.PlayMultiplier.WebApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.RequestServer.PlayMultiplier.Commands
{
    public class SetPlayMultCommandHandler : BaseClientUserRequestHandler<SetPlayMultRequest>
    {
        IServerPlayMultService _playMultService = null;
        protected override async Task InnerHandleMessage(WebContext context, SetPlayMultRequest command, CancellationToken token)
        {
            await _playMultService.SetPlayMult(context, command.PlayMult);
        }
    }
}
