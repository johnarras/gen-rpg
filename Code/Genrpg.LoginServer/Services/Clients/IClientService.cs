using Genrpg.LoginServer.Core;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Login.Interfaces;
using Genrpg.Shared.Login.Messages;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.LoginServer.Services.Clients
{
    public interface IClientService : IInitializable
    {
        Task<List<ILoginResult>> HandleClient(LoginGameState gs, string postData, CancellationToken token);
    }
}
