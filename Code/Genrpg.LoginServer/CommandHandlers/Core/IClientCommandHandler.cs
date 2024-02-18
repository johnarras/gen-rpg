using Genrpg.LoginServer.Core;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Login.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.LoginServer.CommandHandlers.Core
{
    public interface IClientCommandHandler : ISetupDictionaryItem<Type>
    {
        Task Reset();
        Task Execute(LoginGameState gs, ILoginCommand command, CancellationToken token);
    }
}
