using Genrpg.RequestServer.AuthCommandHandlers;
using Genrpg.RequestServer.ClientCommands;
using Genrpg.RequestServer.Maps;
using Genrpg.RequestServer.NoUserCommands;
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
