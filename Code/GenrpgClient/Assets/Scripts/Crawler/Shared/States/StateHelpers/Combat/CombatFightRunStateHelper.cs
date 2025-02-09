
using Genrpg.Shared.Core.Constants;
using Genrpg.Shared.Crawler.Combat.Constants;
using Genrpg.Shared.Crawler.Combat.Entities;
using Genrpg.Shared.Crawler.Constants;
using Genrpg.Shared.Crawler.Monsters.Entities;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.States.Constants;
using Genrpg.Shared.Crawler.States.Entities;
using Genrpg.Shared.Units.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.Shared.Crawler.States.StateHelpers.Combat
{
    public class CombatFightRunStateHelper : BaseCombatStateHelper
    {
        public override ECrawlerStates GetKey() { return ECrawlerStates.CombatFightRun; }

        public override async Task<CrawlerStateData> Init(CrawlerStateData currentData, CrawlerStateAction action, CancellationToken token)
        {
            CrawlerStateData stateData = CreateStateData();

            PartyData partyData = _crawlerService.GetParty();

            if (partyData.Combat == null)
            {
                return new CrawlerStateData(ECrawlerStates.Error, true) { ExtraData = "Party is not in combat." };
            }

            bool didShowPortrait = false;
            stateData.Actions.Add(new CrawlerStateAction("You face: "));

            foreach (CombatGroup group in partyData.Combat.Enemies)
            {
                if (group.Units.Count < 1)
                {
                    continue;
                }

                if (!didShowPortrait)
                {
                    partyData.WorldPanel.SetPicture(group.Units[0].PortraitName, false);
                    stateData.WorldSpriteName = group.Units[0].PortraitName;
                    didShowPortrait = true;
                }

                stateData.Actions.Add(new CrawlerStateAction(group.ShowStatus()));
            }

            List<Monster> alliedMonsters = new List<Monster>();

            foreach (CombatGroup group in partyData.Combat.Allies)
            {
                foreach (CrawlerUnit unit in group.Units)
                {
                    if (unit is Monster monster)
                    {
                        alliedMonsters.Add(monster);
                    }
                }
            }

            List<IGrouping<long,Monster>> alliedMonsterGroups = alliedMonsters.GroupBy(x => x.UnitTypeId).ToList();

            if (alliedMonsterGroups.Count > 0)
            {

                UnitSettings settings = _gameData.Get<UnitSettings>(_gs.ch);
                StringBuilder sb = new StringBuilder();
                sb.Append("Your Allies: ");

                for (int i = 0; i < alliedMonsterGroups.Count; i++)
                {
                    IGrouping<long,Monster> group = alliedMonsterGroups[i];
                
                    UnitType utype = settings.Get(group.Key);
                    if (utype != null)
                    {
                        if (i > 0)
                        {
                            sb.Append(", ");
                        }
                        sb.Append(group.Count() + " ");
                        if (group.Count() == 1)
                        {
                            sb.Append(utype.Name);
                        }
                        else
                        {
                            sb.Append(utype.PluralName);
                        }
                    }
                }
                stateData.Actions.Add(new CrawlerStateAction(sb.ToString()));
            }


            stateData.Actions.Add(new CrawlerStateAction(" "));

            if (partyData.Combat.RoundsComplete == 0)
            {
                stateData.Actions.Add(new CrawlerStateAction("Prepare", 'P', ECrawlerStates.CombatPlayer,
                       onClickAction: delegate ()
                       {
                           partyData.Combat.PartyGroup.CombatGroupAction = ECombatGroupActions.Prepare;
                       }));
            }

            stateData.Actions.Add(new CrawlerStateAction("Fight", 'F', ECrawlerStates.CombatPlayer,
                   onClickAction: delegate ()
                   {
                       partyData.Combat.PartyGroup.CombatGroupAction = ECombatGroupActions.Fight;
                   }));



            if (partyData.GameMode != ECrawlerGameModes.Roguelite)
            {
                stateData.Actions.Add(new CrawlerStateAction("Run", 'R', ECrawlerStates.CombatConfirm,
                    onClickAction: delegate ()
                    {
                        partyData.Combat.PartyGroup.CombatGroupAction = ECombatGroupActions.Run;
                    }));
            }

            long minRange = CrawlerCombatConstants.MaxRange;

            foreach (CombatGroup group in partyData.Combat.Enemies)
            {
                minRange = Math.Min(minRange, group.Range);
            }

            if (minRange > CrawlerCombatConstants.MinRange)
            {
                stateData.Actions.Add(new CrawlerStateAction("Advance", 'A', ECrawlerStates.CombatConfirm,
               onClickAction: delegate ()
               {
                   partyData.Combat.PartyGroup.CombatGroupAction = ECombatGroupActions.Advance;
               }));
            }


            await Task.CompletedTask;
            return stateData;
        }
    }
}
