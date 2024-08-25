using Genrpg.RequestServer.Core;
using Genrpg.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.RequestServer.PlayMultiplier.Services
{
    public interface IServerPlayMultService : IInjectable
    {
        Task SetPlayMult(WebContext context, long newPlayMult);
    }
}
