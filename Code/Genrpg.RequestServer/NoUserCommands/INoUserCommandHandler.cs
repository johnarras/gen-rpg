using Genrpg.RequestServer.Core;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Website.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.RequestServer.NoUserCommands
{
    public interface INoUserCommandHandler : ISetupDictionaryItem<Type>
    {
        Task Reset();
        Task Execute(WebContext context, INoUserCommand command, CancellationToken token);
    }
}
