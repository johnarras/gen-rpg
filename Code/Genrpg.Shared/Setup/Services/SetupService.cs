
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Utils;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.Shared.Setup.Services
{
    public class SetupService : IService
    {
        public virtual void SetupServiceLocator(GameState gs)
        {
            LocatorSetup ls = new LocatorSetup();
            ls.Setup(gs);
        }

        public virtual async Task SetupGame(GameState gs, CancellationToken token)
        {
            SetupServiceLocator(gs);

            gs.loc.ResolveSelf();
            gs.loc.Resolve(this);

            await ReflectionUtils.SetupServices(gs, gs.loc.GetVals(), token);

        }

        public virtual async Task FinalSetup(GameState gs)
        {
            await Task.CompletedTask;
        }

        public virtual bool CreateMissingGameData()
        {
            return false;
        }
    }
}
