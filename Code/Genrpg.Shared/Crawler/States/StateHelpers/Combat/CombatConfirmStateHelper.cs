
using Genrpg.Shared.Crawler.Combat.Constants;
using Genrpg.Shared.Crawler.Monsters.Entities;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.States.Constants;
using Genrpg.Shared.Crawler.States.Entities;
using Genrpg.Shared.Utils;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.Shared.Crawler.States.StateHelpers.Combat
{
    public class CombatConfirmStateHelper : BaseCombatStateHelper
    {
        public override ECrawlerStates GetKey() { return ECrawlerStates.CombatConfirm; }

        public override async Task<CrawlerStateData> Init(CrawlerStateData currentData, CrawlerStateAction action, CancellationToken token)
        {
            CrawlerStateData stateData = CreateStateData();

            PartyData party = _crawlerService.GetParty();

            if (party.Combat == null)
            {
                return new CrawlerStateData(ECrawlerStates.Error, true) { ExtraData = "Party is not in combat." };
            }

            if (party.Combat.PartyGroup.CombatGroupAction == ECombatGroupActions.Advance)
            {
                stateData.Actions.Add(new CrawlerStateAction("Are you sure you wish to advance?", CharCodes.None, ECrawlerStates.None));
            }
            else if (party.Combat.PartyGroup.CombatGroupAction == ECombatGroupActions.Run)
            {
                stateData.Actions.Add(new CrawlerStateAction("Are you sure you wish to run?", CharCodes.None, ECrawlerStates.None));
            }
            else
            {
                foreach (CrawlerUnit combatUnit in party.Combat.PartyGroup.Units)
                {
                    stateData.Actions.Add(new CrawlerStateAction(combatUnit.Name + ": " + combatUnit.Action.Text));
                }
                stateData.Actions.Add(new CrawlerStateAction("Use these actions?", CharCodes.None, ECrawlerStates.None));
            }

            stateData.Actions.Add(new CrawlerStateAction("Yes", 'Y', ECrawlerStates.ProcessCombatRound));
            stateData.Actions.Add(new CrawlerStateAction("No", 'N', ECrawlerStates.CombatFightRun,
                onClickAction: delegate ()
                {
                    // Need to reset all combat round data and start over.
                    _combatService.EndCombatRound(party);
                }));


            await Task.CompletedTask;
            return stateData;


        }
    }
}
