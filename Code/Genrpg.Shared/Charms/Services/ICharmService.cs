using Genrpg.Shared.Charms.PlayerData;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Charms.Services
{
    public interface ICharmService : IService
    {
        List<PlayerCharmBonusList> CalcBonuses(GameState gs, string charmId);

        List<string> PrintBonuses(GameState gs, PlayerCharmBonusList list);
    }
}
