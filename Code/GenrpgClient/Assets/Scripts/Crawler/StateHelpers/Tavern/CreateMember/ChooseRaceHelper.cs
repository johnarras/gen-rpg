using Assets.Scripts.Crawler.CrawlerStates;
using Assets.Scripts.Crawler.StateHelpers;

using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Sexes.Settings;
using Genrpg.Shared.Spawns.WorldData;
using Genrpg.Shared.Units.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.UI.Crawler.States
{
    public class ChooseRaceHelper : BaseStateHelper
    {
        public override ECrawlerStates GetKey() { return ECrawlerStates.ChooseRace; }


        public override async Awaitable<CrawlerStateData> Init(CrawlerStateData currentState, CrawlerStateAction action, CancellationToken token)
        {
            CrawlerStateData stateData = CreateStateData();

            PartyMember member = action.ExtraData as PartyMember;

            IReadOnlyList<UnitType> unitTypes = _gameData.Get<UnitSettings>(null).GetData();

            foreach (UnitType utype in unitTypes)
            {
                if (utype.IdKey < 1 || !utype.PlayerRace)
                {
                    continue;
                }

                stateData.Actions.Add(new CrawlerStateAction(utype.Name, (KeyCode)char.ToLower(utype.Name[0]), ECrawlerStates.RollStats,
                    delegate 
                    {
                        member.RaceId = utype.IdKey;
                        member.UnitTypeId = utype.IdKey;
                    }, member
                    ));
            }

            stateData.Actions.Add(new CrawlerStateAction("Escape", KeyCode.Escape, ECrawlerStates.ChooseSex,
                delegate { member.Spawn = null;
                   member.RaceId = 0;
                }, member));

            await Task.CompletedTask;
            return stateData;

        }
    }
}
