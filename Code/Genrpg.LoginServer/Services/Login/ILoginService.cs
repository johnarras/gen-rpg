using Genrpg.LoginServer.Core;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Login.Interfaces;
using Genrpg.Shared.Login.Messages.Login;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.LoginServer.Services.Login
{
    public interface ILoginService : IInjectable
    {
        Task Login(LoginContext context, string postData, CancellationToken token);
    }
}
