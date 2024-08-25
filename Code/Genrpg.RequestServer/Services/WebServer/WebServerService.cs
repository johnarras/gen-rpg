using Genrpg.ServerShared.Maps;
using Genrpg.Shared.HelperClasses;
using Genrpg.RequestServer.AuthCommandHandlers;
using Genrpg.RequestServer.Maps;
using Genrpg.RequestServer.NoUserCommands;
using Genrpg.RequestServer.ClientCommands;

namespace Genrpg.RequestServer.Services.WebServer
{
    public class WebServerService : IWebServerService
    {

        private IMapDataService _mapDataService = null!;

        private MapStubList _mapStubs { get; set; } = new MapStubList();
        private SetupDictionaryContainer<Type, IClientCommandHandler> _clientCommandHandlers = new SetupDictionaryContainer<Type, IClientCommandHandler>();
        private SetupDictionaryContainer<Type, INoUserCommandHandler> _noUserCommandHandlers = new SetupDictionaryContainer<Type, INoUserCommandHandler>();
        private SetupDictionaryContainer<Type, IAuthCommandHandler> _authCommandHandlers = new SetupDictionaryContainer<Type, IAuthCommandHandler>();

        public async Task Initialize(CancellationToken token)
        {
            _mapStubs.Stubs = await _mapDataService.GetMapStubs();
            await Task.CompletedTask;
        }

        public MapStubList GetMapStubs()
        {
            return _mapStubs;
        }

        public IAuthCommandHandler GetAuthCommandHandler(Type type)
        {
            if (_authCommandHandlers.TryGetValue(type, out IAuthCommandHandler handler))
            {
                return handler;
            }
            return null;
        }

        public IClientCommandHandler GetClientCommandHandler(Type type)
        {
            if (_clientCommandHandlers.TryGetValue(type, out IClientCommandHandler commandHandler))
            {
                return commandHandler;
            }

            return null;
        }

        public INoUserCommandHandler GetNoUserCommandHandler(Type type)
        {
            if (_noUserCommandHandlers.TryGetValue(type, out INoUserCommandHandler commandHandler))
            {
                return commandHandler;
            }
            return null;
        }

        public async Task ResetCommandHandlers()
        {
            foreach (IClientCommandHandler handler in _clientCommandHandlers.GetDict().Values)
            {
                await handler.Reset();
            }

            foreach (INoUserCommandHandler handler in _noUserCommandHandlers.GetDict().Values)
            {
                await handler.Reset();
            }
        }
    }
}
