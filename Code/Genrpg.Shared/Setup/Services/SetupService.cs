
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Utils;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.Shared.Setup.Services
{
    public class SetupService : IInitializable
    {

        public async Task Initialize(IGameState gs, CancellationToken toke)
        {
            await Task.CompletedTask;
        }

        public virtual void SetupServiceLocator(IGameState gs)
        {
            LocatorSetup ls = new LocatorSetup();
            ls.Setup(gs);
        }

        public virtual async Task SetupGame(IGameState gs, CancellationToken token)
        {
            SetupServiceLocator(gs);

            gs.loc.ResolveSelf();
            gs.loc.Resolve(this);

            await ReflectionUtils.SetupServices(gs, gs.loc.GetVals(), token);

        }

        public virtual async Task FinalSetup(IGameState gs)
        {
            await Task.CompletedTask;
        }

        public virtual bool CreateMissingGameData()
        {
            return false;
        }
    }
}
