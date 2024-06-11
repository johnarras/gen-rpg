using Assets.Scripts.Atlas.Constants;
using Assets.Scripts.Crawler.Constants;
using Assets.Scripts.Crawler.CrawlerStates;
using Assets.Scripts.UI.Crawler.States;

using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Units.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Analytics;

namespace Assets.Scripts.Crawler.StateHelpers
{
    public class ChoosePortraitHelper : BaseStateHelper
    {
        public override ECrawlerStates GetKey() { return ECrawlerStates.ChoosePortrait; }

        public override async Awaitable<CrawlerStateData> Init(CrawlerStateData currentState, CrawlerStateAction action, CancellationToken token)
        {
            CrawlerStateData stateData = CreateStateData();

            IReadOnlyList<UnitType> allUnitTypes = _gameData.Get<UnitSettings>(null).GetData();

            PartyMember member = action.ExtraData as PartyMember;

            foreach (UnitType unitType in allUnitTypes)
            {
                if (unitType.IdKey < 1 || string.IsNullOrEmpty(unitType.Icon))
                {
                    continue;
                }
                stateData.Actions.Add(new CrawlerStateAction(unitType.Icon, KeyCode.None, ECrawlerStates.ChooseName,
                   delegate
                   {
                       member.PortraitName = unitType.Icon;
                   }, member, unitType.Icon
                   ));
            }

            stateData.Actions.Add(new CrawlerStateAction("Escape", KeyCode.Escape, ECrawlerStates.ChooseClass,
                delegate { member.PortraitName = null; }, member));
            
            

            return stateData;
        }
    }
}
