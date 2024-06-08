using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Setup.Services;

namespace Genrpg.MonsterServer.Setup
{
    public class MonsterSetupService : SetupService
    {
        public override void SetupServiceLocator(IGameState gs)
        {
            MonsterLocatorSetup iss = new MonsterLocatorSetup();
            iss.Setup(gs);
        }
    }
}
