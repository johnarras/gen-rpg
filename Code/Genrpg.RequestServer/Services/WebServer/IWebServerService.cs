using Genrpg.RequestServer.Auth.RequestHandlers;
using Genrpg.RequestServer.ClientUser.RequestHandlers;
using Genrpg.RequestServer.Maps;
using Genrpg.RequestServer.NoUserRequests.RequestHandlers;
using Genrpg.Shared.Interfaces;
using System;
using System.Threading.Tasks;

namespace Genrpg.RequestServer.Services.WebServer
{
    public interface IWebServerService : IInitializable
    {
        IClientUserRequestHandler GetClientCommandHandler(Type type);
        INoUserRequestHandler GetNoUserCommandHandler(Type type);
        IAuthRequestHandler GetAuthCommandHandler(Type type);
        Task ResetRequestHandlers();
        MapStubList GetMapStubs();
    }
}
