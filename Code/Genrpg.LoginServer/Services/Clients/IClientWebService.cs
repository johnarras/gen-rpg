using Genrpg.LoginServer.Core;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Website.Interfaces;
using Genrpg.Shared.Website.Messages;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.LoginServer.Services.Clients
{
    public interface IClientWebService : IInjectable
    {
        Task HandleWebCommand(WebContext context, string postData, CancellationToken token);
    }
}
