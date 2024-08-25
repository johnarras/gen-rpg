using Genrpg.RequestServer.Core;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Website.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.RequestServer.ClientCommands
{
    public interface IClientCommandHandler : ISetupDictionaryItem<Type>
    {
        Task Reset();
        Task Execute(WebContext context, IWebCommand command, CancellationToken token);
    }
}
