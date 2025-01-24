using Genrpg.RequestServer.Core;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Website.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.RequestServer.ClientUser.RequestHandlers
{
    public interface IClientUserRequestHandler : ISetupDictionaryItem<Type>
    {
        Task Reset();
        Task Execute(WebContext context, IWebRequest request, CancellationToken token);
    }
}
