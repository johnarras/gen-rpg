using Genrpg.LoginServer.CommandHandlers.Core;
using Genrpg.LoginServer.Maps;
using Genrpg.Shared.Interfaces;
using System;
using System.Threading.Tasks;

namespace Genrpg.LoginServer.Services.LoginServer
{
    public interface ILoginServerService : IInitializable
    {
        IClientCommandHandler GetCommandHandler(Type type);
        INoUserCommandHandler GetNoUserCommandHandler(Type type);
        Task ResetCommandHandlers();
        MapStubList GetMapStubs();
    }
}
