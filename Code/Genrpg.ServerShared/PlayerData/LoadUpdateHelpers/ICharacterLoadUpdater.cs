using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Core.Entities;
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
        Task Setup(GameState gs);
        Task Update(GameState gs, Character ch);
    }
}
