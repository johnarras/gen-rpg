using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Setup.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.MapServer.Setup
{
    public class MapServerSetupService : SetupService
    {
        public override void SetupObjectLocator(GameState gs)
        {
            MapServerLocatorSetup ms = new MapServerLocatorSetup();
            ms.Setup(gs);
        }
    }
}
