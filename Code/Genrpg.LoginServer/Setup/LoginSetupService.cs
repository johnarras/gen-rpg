using Genrpg.LoginServer.CommandHandlers.Core;
using Genrpg.LoginServer.Core;
using Genrpg.ServerShared.CloudComms.Services;
using Genrpg.ServerShared.Maps;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Setup.Services;
using Genrpg.Shared.Utils;
using System;
using System.Threading.Tasks;

namespace Genrpg.LoginServer.Setup
{
    public class LoginSetupService : SetupService
    {
        private IMapDataService _mapDataService = null;
        private ICloudCommsService _cloudCommsService = null;
        public override void SetupServiceLocator(IGameState gs)
        {
            LoginLocatorSetup els = new LoginLocatorSetup();
            els.Setup(gs);
            gs.loc.ResolveSelf();
        }

        public override async Task FinalSetup(IGameState gs)
        {
            await base.FinalSetup(gs);
            if (gs is LoginGameState lgs)
            {
                gs.loc.Resolve(this);
                lgs.commandHandlers = ReflectionUtils.SetupDictionary<Type, IClientCommandHandler>(gs);
                lgs.noUserCommandHandlers = ReflectionUtils.SetupDictionary<Type, INoUserCommandHandler>(gs);
                lgs.mapStubs.Stubs = await _mapDataService.GetMapStubs();
                _cloudCommsService.SetupPubSubMessageHandlers(lgs);
            }
        }
    }
}
