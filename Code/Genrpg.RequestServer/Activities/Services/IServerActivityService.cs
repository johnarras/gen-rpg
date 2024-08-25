using Genrpg.RequestServer.Core;
using Genrpg.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.RequestServer.Activities.Services
{
    public interface IServerActivityService : IInjectable
    {
        Task DailyReset(WebContext context, DateTime currentResetDay, double resetHours);
    }
}
