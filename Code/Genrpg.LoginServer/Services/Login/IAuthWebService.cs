using Genrpg.LoginServer.Core;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Website.Interfaces;
using Genrpg.Shared.Website.Messages.Login;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.LoginServer.Services.Login
{
    public interface IAuthWebService : IInjectable
    {
        Task HandleAuthCommand(WebContext context, string postData, CancellationToken token);
    }
}
