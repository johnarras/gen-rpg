using Assets.Scripts.Crawler.Constants;
using Assets.Scripts.Interfaces;
using Genrpg.Shared.Crawler.Combat.Entities;
using Genrpg.Shared.Crawler.Maps.Services;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.States.Constants;
using Genrpg.Shared.Crawler.States.Entities;
using Genrpg.Shared.UI.Services;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.Shared.Crawler.States.StateHelpers.Combat
{
    public class StartCombatStateHelper : BaseCombatStateHelper
    {
        private IAudioService _audioService;
        public override ECrawlerStates GetKey() { return ECrawlerStates.StartCombat; }

        public override async Task<CrawlerStateData> Init(CrawlerStateData currentData, CrawlerStateAction action, CancellationToken token)
        {
            CrawlerStateData stateData = null;

            PartyData party = _crawlerService.GetParty();

            InitialCombatState initialState = action.ExtraData as InitialCombatState;

            if (initialState == null)
            {
                initialState = new InitialCombatState();
            }

            if (await _combatService.StartCombat(_crawlerService.GetParty(), initialState))
            {
                _audioService.PlaySound(CrawlerAudio.StartCombat);
                stateData = new CrawlerStateData(ECrawlerStates.CombatFightRun, true);
            }
            else
            {
                stateData = new CrawlerStateData(ECrawlerStates.Error, true) { ExtraData = "Failed to start combat." };
            }

            await Task.CompletedTask;
            return stateData;
        }
    }
}
