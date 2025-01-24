using Genrpg.RequestServer.Core;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Website.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.RequestServer.Auth.RequestHandlers
{
    public interface IAuthRequestHandler : ISetupDictionaryItem<Type>
    {
        Task Reset();
        Task Execute(WebContext context, IAuthRequest request, CancellationToken token);
    }
}
