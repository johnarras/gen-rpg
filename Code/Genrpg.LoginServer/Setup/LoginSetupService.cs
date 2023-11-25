using Genrpg.LoginServer.CommandHandlers.Core;
using Genrpg.LoginServer.Core;
using Genrpg.ServerShared.CloudComms.Services;
using Genrpg.ServerShared.Maps;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Reflection.Services;
using Genrpg.Shared.Setup.Services;
using System;
using System.Threading.Tasks;

namespace Genrpg.LoginServer.Setup
{
    public class LoginSetupService : SetupService
    {
        private IMapDataService _mapDataService = null;
        private IReflectionService _reflectionService = null;
        private ICloudCommsService _cloudCommsService = null;
        public override void SetupObjectLocator(GameState gs)
        {
            LoginLocatorSetup els = new LoginLocatorSetup();
            els.Setup(gs);
            gs.loc.ResolveSelf();
        }

        public override async Task FinalSetup(GameState gs)
        {
            await base.FinalSetup(gs);
            if (gs is LoginGameState lgs)
            {
                gs.loc.Resolve(this);
                lgs.commandHandlers = _reflectionService.SetupDictionary<Type, ILoginCommandHandler>(gs);
                lgs.mapStubs.Stubs = await _mapDataService.GetMapStubs(lgs);
                _cloudCommsService.SetupPubSubMessageHandlers(lgs);
            }
        }
    }
}
