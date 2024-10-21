using Genrpg.Shared.Crawler.Combat.Settings;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.Roles.Settings;
using Genrpg.Shared.Crawler.States.Constants;
using Genrpg.Shared.Crawler.States.Entities;
using Genrpg.Shared.Crawler.Stats.Services;
using Genrpg.Shared.ProcGen.Settings.Names;
using Genrpg.Shared.Stats.Constants;
using Genrpg.Shared.Stats.Entities;
using Genrpg.Shared.Stats.Settings.Stats;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace Genrpg.Shared.Crawler.States.StateHelpers.Guild.CreateMember
{
    public class RollStatsHelper : BaseStateHelper
    {

        public override ECrawlerStates GetKey() { return ECrawlerStates.RollStats; }

        public override async Task<CrawlerStateData> Init(CrawlerStateData currentState, CrawlerStateAction action, CancellationToken token)
        {
            CrawlerStateData stateData = CreateStateData();

            PartyMember member = action.ExtraData as PartyMember;

            member.ClearPermStats();
            member.Stats = new StatGroup();

            List<NameIdValue> stats = _statService.GetInitialStats(member);

            List<List<NameIdValue>> roleBonuses = new List<List<NameIdValue>>();

            List<Role> allRoles = _gameData.Get<RoleSettings>(_gs.ch).GetRoles(member.Roles);


            Dictionary<string, List<NameIdValue>> roleStatDict = new Dictionary<string, List<NameIdValue>>();

            foreach (Role role in allRoles)
            {
                List<NameIdValue> currBonuses = _statService.GetInitialStatBonuses(role.IdKey);
                if (currBonuses.Count > 0)
                {
                    roleStatDict[role.Name] = currBonuses;
                }
            }

            foreach (NameIdValue nid in stats)
            {
                member.AddPermStat(nid.IdKey, nid.Val);

                string textToShow = nid.Name + ": " + nid.Val + " ";

                foreach (string roleName in roleStatDict.Keys)
                {
                    List<NameIdValue> roleVals = roleStatDict[roleName];

                    NameIdValue currBonus = roleVals.FirstOrDefault(x => x.IdKey == nid.IdKey);

                    if (currBonus != null)
                    {
                        textToShow += "[" + (currBonus.Val > 0 ? "+" : "") + currBonus.Val + " " + roleName + "] ";
                    }
                }


                stateData.Actions.Add(new CrawlerStateAction(textToShow, CharCodes.None, ECrawlerStates.None));
            }

            stateData.Actions.Add(new CrawlerStateAction("Accept", 'A', ECrawlerStates.ChoosePortrait, extraData: member));

            stateData.Actions.Add(new CrawlerStateAction("Escape", CharCodes.Escape, ECrawlerStates.ChooseClass,
                delegate
                {
                    member.Stats = new StatGroup();
                    while (member.Roles.Count > 1)
                    {
                        member.Roles.RemoveAt(1);
                    }

                }, member));

            await Task.CompletedTask;
            return stateData;

        }

    }
}
