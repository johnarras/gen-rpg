using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Setup.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.InstanceServer.Setup
{
    public class InstanceSetupService : SetupService
    {
        public override void SetupObjectLocator(GameState gs)
        {
            InstanceLocatorSetup iss = new InstanceLocatorSetup();
            iss.Setup(gs);
        }
    }
}
