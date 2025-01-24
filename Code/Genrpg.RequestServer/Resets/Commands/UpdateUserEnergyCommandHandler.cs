using Genrpg.RequestServer.ClientUser.RequestHandlers;
using Genrpg.RequestServer.Core;
using Genrpg.RequestServer.Resets.Services;
using Genrpg.Shared.UserEnergy.WebApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.RequestServer.Resets.Commands
{
    public class UpdateUserEnergyCommandHandler : BaseClientUserRequestHandler<UpdateUserEnergyRequest>
    {

        private IHourlyUpdateService _hourlyUpdateService = null;
        protected override async Task InnerHandleMessage(WebContext context, UpdateUserEnergyRequest command, CancellationToken token)
        {
            await _hourlyUpdateService.CheckHourlyUpdate(context);
        }
    }
}
