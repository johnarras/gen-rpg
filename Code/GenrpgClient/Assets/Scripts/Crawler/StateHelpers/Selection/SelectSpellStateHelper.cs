using Assets.Scripts.Crawler.CrawlerStates;
using Assets.Scripts.Crawler.StateHelpers.Combat;
using Assets.Scripts.Crawler.StateHelpers.Selection.Entities;
using Assets.Scripts.UI.Crawler.States;

using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.Spells.Entities;
using Genrpg.Shared.Crawler.Spells.Settings;
using Genrpg.Shared.Stats.Constants;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Crawler.StateHelpers.Selection
{
    public class SelectSpellStateHelper : BaseCombatStateHelper
    {
        public override ECrawlerStates GetKey() { return ECrawlerStates.SelectSpell; }

        public override async Awaitable<CrawlerStateData> Init(CrawlerStateData currentData, CrawlerStateAction action, CancellationToken token)
        {
            CrawlerStateData stateData = CreateStateData();

            PartyData party = _crawlerService.GetParty();

            SelectAction data = action.ExtraData as SelectAction;

            if (data == null)
            {
                return new CrawlerStateData(ECrawlerStates.Error, true) { ExtraData = "Cannot select a spell without a select action" };
            }

            bool inCombat = party.Combat != null;

            long level = data.Member.Level;

            long currMana = data.Member.Stats.Curr(StatTypes.Mana);

            List<CrawlerSpell> spells = _spellService.GetSpellsForMember(party, data.Member, inCombat);

            for (int s = 0; s < spells.Count; s++)
            {
                CrawlerSpell spell = spells[s];

                SelectSpellAction selectSpell = new SelectSpellAction()
                {
                    Action = data,
                    Spell = spell,
                    PowerCost = spell.PowerCost + spell.PowerPerLevel * level,
                };

                string spellText = spell.Name + "    (" + selectSpell.PowerCost + " Mana)";
                ECrawlerStates nextState = ECrawlerStates.OnSelectSpell;
                object extra = selectSpell;
                if (selectSpell.PowerCost > currMana)
                {
                    spellText = spell.Name + "    <color=red>(" + selectSpell.PowerCost + " Mana)</color>";
                    nextState = ECrawlerStates.None;
                    extra = null;
                }

                stateData.Actions.Add(new CrawlerStateAction(spellText, KeyCode.None, nextState, extraData: extra));
            }

            stateData.Actions.Add(new CrawlerStateAction("", KeyCode.Escape, currentData.Id, extraData:data));


            await Task.CompletedTask;

            return stateData;
        }
    }
}
