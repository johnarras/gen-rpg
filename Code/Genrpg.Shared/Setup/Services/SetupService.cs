
using Genrpg.Shared.Constants;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.Shared.Setup.Services
{
    public class SetupService : IService
    {
        public virtual void SetupObjectLocator(GameState gs)
        {
            LocatorSetup ls = new LocatorSetup();
            ls.Setup(gs);
        }

        public virtual async Task SetupGame(GameState gs, CancellationToken token)
        {
            SetupObjectLocator(gs);

            gs.loc.ResolveSelf();

            gs.loc.Resolve(this);
            List<IService> vals = gs.loc.GetVals();


            List<Task> setupTasks = new List<Task>();
            foreach (IService val in vals)
            {
                if (val is ISetupService setupVal)
                {
                    setupTasks.Add(setupVal.Setup(gs, token));
                }
            }

            await Task.WhenAll(setupTasks);
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
