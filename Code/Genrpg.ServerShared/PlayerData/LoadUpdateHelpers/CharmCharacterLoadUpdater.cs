using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Charms.PlayerData;
using Genrpg.Shared.Charms.Services;
using Genrpg.Shared.Core.Entities;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.ServerShared.PlayerData.LoadUpdateHelpers
{
    public class CharmCharacterLoadUpdater : BaseCharacterLoadUpdater
    {
        private ICharmService _charmService = null;
        private IStatService _statService = null;
        public override int Priority => 3;

        public override async Task Update(GameState gs, Character ch)
        {
            PlayerCharmData playerCharmData = ch.Get<PlayerCharmData>();

            foreach (PlayerCharm status in playerCharmData.GetData())
            {
                status.Bonuses = _charmService.CalcBonuses(gs, status.Hash);
            }

            _statService.CalcStats(ch, false);
            await Task.CompletedTask;
        }
    }
}
