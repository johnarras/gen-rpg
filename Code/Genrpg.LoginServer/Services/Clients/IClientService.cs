using Genrpg.LoginServer.Core;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Login.Interfaces;
using Genrpg.Shared.Login.Messages;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.LoginServer.Services.Clients
{
    public interface IClientService : IInjectable
    {
        Task HandleClient(LoginContext context, string postData, CancellationToken token);
    }
}
