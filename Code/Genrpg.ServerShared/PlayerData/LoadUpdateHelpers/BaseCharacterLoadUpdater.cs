using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.ServerShared.PlayerData.LoadUpdateHelpers
{
    public abstract class BaseCharacterLoadUpdater : ICharacterLoadUpdater
    {
        public virtual int Priority => 0;

        public abstract Task Update(IRandom rand, Character ch);

        public virtual async Task Setup(IGameState gs)
        {
            await Task.CompletedTask;
        }
    }
}
