using Genrpg.RequestServer.Core;
using Genrpg.Shared.Interfaces;

namespace Genrpg.RequestServer.Services.Clients
{
    public interface IClientWebService : IInjectable
    {
        Task HandleUserClientRequest(WebContext context, string postData, CancellationToken token);
    }
}
