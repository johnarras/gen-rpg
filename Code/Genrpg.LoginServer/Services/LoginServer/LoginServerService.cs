using Genrpg.LoginServer.CommandHandlers.Core;
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

namespace Genrpg.LoginServer.Services.LoginServer
{
    public class LoginServerService : ILoginServerService
    {

        private IMapDataService _mapDataService;

        private MapStubList _mapStubs { get; set; } = new MapStubList();
        private SetupDictionaryContainer<Type, IClientCommandHandler> _commandHandlers = new();
        private SetupDictionaryContainer<Type, INoUserCommandHandler> _noUserCommandHandlers = new();

        public async Task Initialize(CancellationToken token)
        {
            _mapStubs.Stubs = await _mapDataService.GetMapStubs();
            await Task.CompletedTask;
        }

        public MapStubList GetMapStubs() 
        { 
            return _mapStubs; 
        }

        public IClientCommandHandler GetCommandHandler(Type type)
        {
            if (_commandHandlers.TryGetValue(type, out IClientCommandHandler commandHandler))
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
            foreach (IClientCommandHandler handler in _commandHandlers.GetDict().Values)
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
