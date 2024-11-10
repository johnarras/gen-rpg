using Genrpg.RequestServer.ClientCommands;
using Genrpg.RequestServer.Core;
using Genrpg.RequestServer.PlayMultiplier.Services;
using Genrpg.Shared.PlayMultiplier.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.RequestServer.PlayMultiplier.Commands
{
    public class SetPlayMultCommandHandler : BaseClientCommandHandler<SetPlayMultCommand>
    {
        IServerPlayMultService _playMultService = null;
        protected override async Task InnerHandleMessage(WebContext context, SetPlayMultCommand command, CancellationToken token)
        {
            await _playMultService.SetPlayMult(context, command.PlayMult);
        }
    }
}
