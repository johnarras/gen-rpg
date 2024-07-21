using Genrpg.LoginServer.Core;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Website.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

namespace Genrpg.LoginServer.Services.NoUsers
{
    public interface INoUserWebService : IInjectable
    {
        Task HandleNoUserCommand(WebContext context, string postData, CancellationToken token);
    }
}
