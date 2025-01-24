using Genrpg.RequestServer.Core;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Website.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.RequestServer.NoUserRequests.RequestHandlers
{
    public interface INoUserRequestHandler : ISetupDictionaryItem<Type>
    {
        Task Reset();
        Task Execute(WebContext context, INoUserRequest command, CancellationToken token);
    }
}
