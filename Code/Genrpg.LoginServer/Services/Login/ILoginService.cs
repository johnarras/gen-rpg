using Genrpg.LoginServer.Core;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Login.Interfaces;
using Genrpg.Shared.Login.Messages.Login;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.LoginServer.Services.Login
{
    public interface ILoginService : IInitializable
    {
        Task<List<ILoginResult>> Login(LoginGameState gs, string postData, CancellationToken token);
    }
}
