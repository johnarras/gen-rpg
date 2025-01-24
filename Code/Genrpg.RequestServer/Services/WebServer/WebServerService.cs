using Genrpg.ServerShared.Maps;
using Genrpg.Shared.HelperClasses;
using Genrpg.RequestServer.Maps;
using Genrpg.RequestServer.Auth.RequestHandlers;
using Genrpg.RequestServer.NoUserRequests.RequestHandlers;
using Genrpg.RequestServer.ClientUser.RequestHandlers;

namespace Genrpg.RequestServer.Services.WebServer
{
    public class WebServerService : IWebServerService
    {

        private IMapDataService _mapDataService = null!;

        private MapStubList _mapStubs { get; set; } = new MapStubList();
        private SetupDictionaryContainer<Type, IClientUserRequestHandler> _clientCommandHandlers = new SetupDictionaryContainer<Type, IClientUserRequestHandler>();
        private SetupDictionaryContainer<Type, INoUserRequestHandler> _noUserCommandHandlers = new SetupDictionaryContainer<Type, INoUserRequestHandler>();
        private SetupDictionaryContainer<Type, IAuthRequestHandler> _authCommandHandlers = new SetupDictionaryContainer<Type, IAuthRequestHandler>();

        public async Task Initialize(CancellationToken token)
        {
            _mapStubs.Stubs = await _mapDataService.GetMapStubs();
            await Task.CompletedTask;
        }

        public MapStubList GetMapStubs()
        {
            return _mapStubs;
        }

        public IAuthRequestHandler GetAuthCommandHandler(Type type)
        {
            if (_authCommandHandlers.TryGetValue(type, out IAuthRequestHandler handler))
            {
                return handler;
            }
            return null;
        }

        public IClientUserRequestHandler GetClientCommandHandler(Type type)
        {
            if (_clientCommandHandlers.TryGetValue(type, out IClientUserRequestHandler commandHandler))
            {
                return commandHandler;
            }

            return null;
        }

        public INoUserRequestHandler GetNoUserCommandHandler(Type type)
        {
            if (_noUserCommandHandlers.TryGetValue(type, out INoUserRequestHandler commandHandler))
            {
                return commandHandler;
            }
            return null;
        }

        public async Task ResetRequestHandlers()
        {
            foreach (IClientUserRequestHandler handler in _clientCommandHandlers.GetDict().Values)
            {
                await handler.Reset();
            }

            foreach (INoUserRequestHandler handler in _noUserCommandHandlers.GetDict().Values)
            {
                await handler.Reset();
            }
        }
    }
}
