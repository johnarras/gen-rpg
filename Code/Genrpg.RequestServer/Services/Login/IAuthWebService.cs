using Genrpg.RequestServer.Core;
using Genrpg.Shared.Interfaces;

namespace Genrpg.RequestServer.Services.Login
{
    public interface IAuthWebService : IInjectable
    {
        Task HandleAuthRequest(WebContext context, string postData, CancellationToken token);
    }
}
