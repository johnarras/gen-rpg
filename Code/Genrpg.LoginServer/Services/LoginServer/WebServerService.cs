using Genrpg.LoginServer.Maps;
using System.Collections.Generic;
using System;
using Genrpg.ServerShared.Maps;
using System.Threading.Tasks;
using Genrpg.Shared.Core.Entities;
using System.Threading;
using System.Runtime.InteropServices;
using Genrpg.Shared.Utils;
using ZstdSharp.Unsafe;
using Genrpg.LoginServer.Core;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.HelperClasses;
using Genrpg.LoginServer.NoUserCommandHandlers;
using Genrpg.LoginServer.ClientCommandHandlers;
using Genrpg.Shared.Website.Interfaces;
using Genrpg.LoginServer.AuthCommandHandlers;

namespace Genrpg.LoginServer.Services.LoginServer
{
    public class WebServerService : IWebServerService
    {

        private IMapDataService _mapDataService;

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
