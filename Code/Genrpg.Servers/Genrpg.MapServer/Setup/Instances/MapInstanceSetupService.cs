using Genrpg.ServerShared.Core;
using Genrpg.ServerShared.Maps;
using Genrpg.ServerShared.MapSpawns;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.Shared.Setup.Services;
using Genrpg.Shared.Spawns.Entities;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Genrpg.MapServer.Setup
{
    public class MapInstanceSetupService : SetupService
    {
        public override void SetupServiceLocator(GameState gs)
        {
            MapInstanceLocatorSetup ss = new MapInstanceLocatorSetup();
            ss.Setup(gs);
        }

        public override async Task FinalSetup(GameState gs)
        {
            await base.FinalSetup(gs);
        }
    }
}
