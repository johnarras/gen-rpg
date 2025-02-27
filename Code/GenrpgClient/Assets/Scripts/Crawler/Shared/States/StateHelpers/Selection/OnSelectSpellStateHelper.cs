
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.Spells.Entities;
using Genrpg.Shared.Crawler.States.Constants;
using Genrpg.Shared.Crawler.States.Entities;
using Genrpg.Shared.Crawler.States.StateHelpers.Combat;
using Genrpg.Shared.Crawler.States.StateHelpers.Selection.Entities;
using Genrpg.Shared.Spells.Constants;
using System.Threading;
using System.Threading.Tasks;


namespace Genrpg.Shared.Crawler.States.StateHelpers.Selection
{
    public class OnSelectSpellStateHelper : BaseCombatStateHelper
    {
        public override ECrawlerStates GetKey() { return ECrawlerStates.OnSelectSpell; }

        public override async Task<CrawlerStateData> Init(CrawlerStateData currentData, CrawlerStateAction action, CancellationToken token)
        {
            CrawlerStateData stateData = CreateStateData();

            SelectSpellAction selectSpellAction = action.ExtraData as SelectSpellAction;

            if (selectSpellAction == null)
            {
                return new CrawlerStateData(ECrawlerStates.Error, true) { ExtraData = "Select Spell Action missing on select" };
            }

            PartyData party = _crawlerService.GetParty();

            UnitAction newAction = _combatService.GetActionFromSpell(party, selectSpellAction.Action.Member,
                selectSpellAction.Spell, null);

            if (newAction == null)
            {
                return new CrawlerStateData(ECrawlerStates.Error, true) { ExtraData = "Failed to create action after selecting spell" };
            }

            selectSpellAction.Action.Action = newAction;
            selectSpellAction.Action.Member.Action = newAction;

            if (newAction.Spell.TargetTypeId == TargetTypes.Special)
            {
                return new CrawlerStateData(ECrawlerStates.SpecialSpellCast, true) { ExtraData = selectSpellAction };
            }

            ECrawlerStates nextState = selectSpellAction.Action.NextState;
            if (newAction.FinalTargets.Count < 1)
            {
                if (newAction.PossibleTargetGroups.Count > 0)
                {
                    return new CrawlerStateData(ECrawlerStates.SelectEnemyGroup, true) { ExtraData = selectSpellAction };
                }
                else if (newAction.PossibleTargetUnits.Count > 0)
                {
                    return new CrawlerStateData(ECrawlerStates.SelectAllyTarget, true) { ExtraData = selectSpellAction };
                }
                else
                {
                    return new CrawlerStateData(ECrawlerStates.Error, true) { ExtraData = "Selected spell but no targets available" };
                }
            }

            await Task.CompletedTask;
            return new CrawlerStateData(nextState, true) { ExtraData = selectSpellAction };
        }
    }
}
