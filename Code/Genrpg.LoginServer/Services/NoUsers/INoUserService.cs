using Genrpg.LoginServer.Core;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Login.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

namespace Genrpg.LoginServer.Services.NoUsers
{
    public interface INoUserService : IInjectable
    {
        Task HandleNoUserCommand(LoginContext context, string postData, CancellationToken token);
    }
}
