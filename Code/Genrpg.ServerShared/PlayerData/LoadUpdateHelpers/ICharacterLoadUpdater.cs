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
    public interface ICharacterLoadUpdater
    {
        int Priority { get; }
        Task Setup(IGameState gs);
        Task Update(IRandom rand, Character ch);
    }
}
