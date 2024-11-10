using Genrpg.RequestServer.ClientCommands;
using Genrpg.RequestServer.Core;
using Genrpg.RequestServer.Resets.Services;
using Genrpg.Shared.UserEnergy.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.RequestServer.Resets.Commands
{
    public class UpdateUserEnergyCommandHandler : BaseClientCommandHandler<UpdateUserEnergyCommand>
    {

        private IHourlyUpdateService _hourlyUpdateService = null;
        protected override async Task InnerHandleMessage(WebContext context, UpdateUserEnergyCommand command, CancellationToken token)
        {
            await _hourlyUpdateService.CheckHourlyUpdate(context);
        }
    }
}
