using Genrpg.LoginServer.CommandHandlers;
using Genrpg.LoginServer.Core;
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
                IReflectionService reflectionService = gs.loc.Get<IReflectionService>();
                lgs.commandHandlers = reflectionService.SetupDictionary<Type, ILoginCommandHandler>(gs);
                IMapDataService mapDataService = gs.loc.Get<IMapDataService>();
                lgs.mapStubs = await mapDataService.GetMapStubs(lgs);
            }
        }
    }
}
