using Genrpg.LoginServer.AuthCommandHandlers;
using Genrpg.LoginServer.ClientCommandHandlers;
using Genrpg.LoginServer.Maps;
using Genrpg.LoginServer.NoUserCommandHandlers;
using Genrpg.Shared.Interfaces;
using System;
using System.Threading.Tasks;

namespace Genrpg.LoginServer.Services.LoginServer
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
