using Genrpg.RequestServer.Core;
using Genrpg.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.RequestServer.Resets.Services
{
    public interface IHourlyUpdateService : IInjectable
    {
        Task CheckHourlyUpdate(WebContext context);
    }
}
