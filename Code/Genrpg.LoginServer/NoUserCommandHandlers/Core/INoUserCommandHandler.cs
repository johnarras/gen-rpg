using Genrpg.LoginServer.Core;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Login.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.LoginServer.CommandHandlers.Core
{
    public interface INoUserCommandHandler : ISetupDictionaryItem<Type>
    {
        Task Reset();
        Task Execute(LoginContext context, INoUserCommand command, CancellationToken token);
    }
}
