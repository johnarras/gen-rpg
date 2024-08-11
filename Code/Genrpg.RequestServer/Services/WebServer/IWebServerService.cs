using Genrpg.RequestServer.AuthCommandHandlers;
using Genrpg.RequestServer.ClientCommandHandlers;
using Genrpg.RequestServer.Maps;
using Genrpg.RequestServer.NoUserCommandHandlers;
using Genrpg.Shared.Interfaces;
using System;
using System.Threading.Tasks;

namespace Genrpg.RequestServer.Services.WebServer
{
    public interface IWebServerService : IInitializable
    {
        IClientCommandHandler GetClientCommandHandler(Type type);
        INoUserCommandHandler GetNoUserCommandHandler(Type type);
        IAuthCommandHandler GetAuthCommandHandler(Type type);
        Task ResetCommandHandlers();
        MapStubList GetMapStubs();
    }
}
