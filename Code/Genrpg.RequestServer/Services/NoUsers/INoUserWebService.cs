using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Website.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using Genrpg.RequestServer.Core;

namespace Genrpg.RequestServer.Services.NoUsers
{
    public interface INoUserWebService : IInjectable
    {
        Task HandleNoUserRequest(WebContext context, string postData, CancellationToken token);
    }
}
