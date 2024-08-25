using Genrpg.RequestServer.Core;
using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Charms.PlayerData;
using Genrpg.Shared.Charms.Services;
using Genrpg.Shared.Utils;
using System.Threading.Tasks;

namespace Genrpg.RequestServer.PlayerData.LoadUpdateHelpers
{
    public class CharmCharacterLoadUpdater : BaseCharacterLoadUpdater
    {
        private ICharmService _charmService = null;
        private IStatService _statService = null;
        public override int Order => 3;

        public override async Task Update(WebContext context, Character ch)
        {
            PlayerCharmData playerCharmData = ch.Get<PlayerCharmData>();

            foreach (PlayerCharm status in playerCharmData.GetData())
            {
                status.Bonuses = _charmService.CalcBonuses(status.Hash);
            }

            _statService.CalcStats(ch, false);
            await Task.CompletedTask;
        }
    }
}
