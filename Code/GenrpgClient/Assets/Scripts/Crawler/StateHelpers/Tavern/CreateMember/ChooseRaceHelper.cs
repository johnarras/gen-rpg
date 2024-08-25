using Assets.Scripts.Crawler.CrawlerStates;
using Assets.Scripts.Crawler.StateHelpers;
using Assets.Scripts.Crawler.StateHelpers.Tavern.CreateMember;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.Roles.Constants;
using Genrpg.Shared.Crawler.Roles.Settings;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Sexes.Settings;
using Genrpg.Shared.Spawns.WorldData;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.Units.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.UI.Crawler.States
{
    public class ChooseRaceHelper : BaseRoleStateHelper
    {
        public override ECrawlerStates GetKey() { return ECrawlerStates.ChooseRace; }


        public override async Awaitable<CrawlerStateData> Init(CrawlerStateData currentState, CrawlerStateAction action, CancellationToken token)
        {
            CrawlerStateData stateData = CreateStateData();

            PartyMember member = action.ExtraData as PartyMember;

            List<Role> races = _gameData.Get<RoleSettings>(_gs.ch).GetData().Where(x => x.RoleCategoryId == RoleCategories.Origin).ToList();

            foreach (Role race in races)
            { 

                stateData.Actions.Add(new CrawlerStateAction(race.Name, KeyCode.None, ECrawlerStates.RollStats,
                    delegate 
                    {
                        member.Roles.Add(new UnitRole() { RoleId = race.IdKey });
                        member.UnitTypeId = 1;
                    }, member, null, () => { OnPointerEnter(race); }
                    ));
            }

            stateData.Actions.Add(new CrawlerStateAction("Escape", KeyCode.Escape, ECrawlerStates.ChooseSex,
                delegate { member.Spawn = null;
                    member.Roles.Clear();
                    member.UnitTypeId = 0;
                }, member));

            await Task.CompletedTask;
            return stateData;

        }
    }
}
