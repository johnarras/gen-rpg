using Genrpg.Shared.Crawler.Combat.Constants;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.Spells.Entities;
using Genrpg.Shared.Crawler.Spells.Settings;
using Genrpg.Shared.Crawler.States.Constants;
using Genrpg.Shared.Crawler.States.Entities;
using Genrpg.Shared.Crawler.States.StateHelpers.Combat;
using Genrpg.Shared.Crawler.States.StateHelpers.Selection.Entities;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Stats.Constants;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.Utils;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.Shared.Crawler.States.StateHelpers.Selection
{
    public class SelectSpellStateHelper : BaseCombatStateHelper
    {
        public override ECrawlerStates GetKey() { return ECrawlerStates.SelectSpell; }

        public override async Task<CrawlerStateData> Init(CrawlerStateData currentData, CrawlerStateAction action, CancellationToken token)
        {
            CrawlerStateData stateData = CreateStateData();

            PartyData party = _crawlerService.GetParty();

            SelectAction data = action.ExtraData as SelectAction;

            if (data == null)
            {
                return new CrawlerStateData(ECrawlerStates.Error, true) { ExtraData = "Cannot select a spell without a select action" };
            }

            long level = data.Member.Level;

            long currMana = data.Member.Stats.Curr(StatTypes.Mana);

            List<CrawlerSpell> spells = _crawlerSpellService.GetSpellsForMember(party, data.Member);

            for (int s = 0; s < spells.Count; s++)
            {
                CrawlerSpell spell = spells[s];

                SelectSpellAction selectSpell = new SelectSpellAction()
                {
                    Action = data,
                    Spell = spell,
                    PowerCost = _crawlerSpellService.GetPowerCost(party, data.Member, spell),
                };

                string spellText = spell.Name + " (" + selectSpell.PowerCost + ")";
                ECrawlerStates nextState = ECrawlerStates.OnSelectSpell;
                object extra = selectSpell;
                if (selectSpell.PowerCost > currMana)
                {
                    spellText = spell.Name + "    <color=red>(" + selectSpell.PowerCost + " Mana)</color>";
                    nextState = ECrawlerStates.None;
                    extra = null;
                }

                stateData.Actions.Add(new CrawlerStateAction(spellText, CharCodes.None, nextState, extraData: extra, forceButton: false,
                    pointerEnterAction: () => ShowInfo(EntityTypes.CrawlerSpell, spell.IdKey)));
            }

            stateData.Actions.Add(new CrawlerStateAction("", CharCodes.Escape, currentData.Id, extraData: data));


            await Task.CompletedTask;

            return stateData;
        }
    }
}
