using Genrpg.Shared.Charms.PlayerData;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Charms.Services
{
    public interface ICharmService : IInitializable
    {
        List<PlayerCharmBonusList> CalcBonuses(string charmId);

        List<string> PrintBonuses(PlayerCharmBonusList list);
    }
}
