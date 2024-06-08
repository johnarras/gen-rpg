using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Setup.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.PlayerServer.Setup
{
    public class PlayerSetupService : SetupService
    {
        public override void SetupServiceLocator(IGameState gs)
        {
            PlayerLocatorSetup iss = new PlayerLocatorSetup();
            iss.Setup(gs);
        }
    }
}
