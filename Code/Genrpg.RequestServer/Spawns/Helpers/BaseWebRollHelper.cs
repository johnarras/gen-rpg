using Genrpg.RequestServer.Core;
using Genrpg.Shared.Rewards.Entities;
using Genrpg.Shared.Spawns.Entities;
using Genrpg.Shared.Spawns.Settings;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.RequestServer.Spawns.Helpers
{
    public abstract class BaseWebRollHelper : IWebRollHelper
    {
        public abstract long GetKey();
        public virtual async Task<List<Reward>> Roll<SI>(WebContext context, RollData rollData, SI si) where SI : ISpawnItem
        {
            long mult = await GetQuantityMult(context, rollData, si);

            long quantity = MathUtils.LongRange(si.MinQuantity*mult, si.MaxQuantity*mult, context.rand);

            List<Reward> retval = new List<Reward>();

            Reward rew = new Reward();
            rew.EntityId = si.EntityId;
            rew.EntityTypeId = si.EntityTypeId;
            rew.Quantity = quantity;
            rew.QualityTypeId = rollData.QualityTypeId;
            rew.Level = rollData.Level;
            retval.Add(rew);

            return retval;
        }

        protected virtual async Task<long> GetQuantityMult<SI>(WebContext context, RollData rollData, SI si) where SI : ISpawnItem
        {
            await Task.CompletedTask;
            return 1;
        }
    }
}
