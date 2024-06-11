using Assets.Scripts.Crawler.CrawlerStates;
using Assets.Scripts.Crawler.StateHelpers;

using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Factions.Constants;
using Genrpg.Shared.Sexes.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.UI.Crawler.States
{
    public class ChooseSexHelper : BaseStateHelper
    {
        public override ECrawlerStates GetKey() { return ECrawlerStates.ChooseSex; }


        public override async Awaitable<CrawlerStateData> Init(CrawlerStateData currentState, CrawlerStateAction action, CancellationToken token)
        {
            CrawlerStateData stateData = CreateStateData();

            IReadOnlyList<SexType> sexes = _gameData.Get<SexTypeSettings>(null).GetData();

            PartyMember member = new PartyMember(_repoService)
            {
                FactionTypeId = FactionTypes.Player,
            };
            
            foreach (SexType sex in sexes)
            {
                if (sex.IdKey < 1)
                {
                    continue;
                }
                stateData.Actions.Add(new CrawlerStateAction(sex.Name, (KeyCode)char.ToLower(sex.Name[0]), ECrawlerStates.ChooseRace,
                    delegate { member.SexTypeId = sex.IdKey; }, member));
            }

            stateData.Actions.Add(new CrawlerStateAction("Escape", KeyCode.Escape, ECrawlerStates.TavernMain, null, null));
            
            return stateData;

        }
    }
}
