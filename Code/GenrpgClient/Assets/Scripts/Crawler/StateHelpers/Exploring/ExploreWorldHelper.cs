using Assets.Scripts.Crawler.CrawlerStates;
using Assets.Scripts.Crawler.Services;
using Assets.Scripts.Crawler.StateHelpers.PartyMembers;
using Assets.Scripts.Crawler.StateHelpers.Selection.Entities;
using Assets.Scripts.UI.Crawler.States;
using Cysharp.Threading.Tasks;
using Genrpg.Shared.Crawler.Combat.Entities;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.Spells.Entities;
using Genrpg.Shared.Inventory.PlayerData;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using UI.Screens.Constants;
using UnityEngine;

namespace Assets.Scripts.Crawler.StateHelpers.Exploring
{
    public class ExploreWorldHelper : BasePartyMemberSelectHelper
    {
        public override ECrawlerStates GetKey() { return ECrawlerStates.ExploreWorld; }
        public override bool IsTopLevelState() { return true; }
        protected override bool ShowSelectText() { return true; }

        public override async UniTask<CrawlerStateData> Init(UnityGameState gs, CrawlerStateData currentData, CrawlerStateAction action, CancellationToken token)
        {
            CrawlerStateData stateData = await base.Init(gs, currentData, action, token);

            PartyData party = _crawlerService.GetParty();
            party.Combat = null;

            UnitAction unitAction = new UnitAction();
            SelectAction selectAction = new SelectAction()
            {
                ReturnState = ECrawlerStates.ExploreWorld,
                NextState = ECrawlerStates.WorldCast,
                Action = unitAction,
                Member = null,
            };

            stateData.Actions.Add(new CrawlerStateAction("Inn", KeyCode.I, ECrawlerStates.TavernMain));
            stateData.Actions.Add(new CrawlerStateAction("Train", KeyCode.T, ECrawlerStates.TrainingMain));
            stateData.Actions.Insert(0, new CrawlerStateAction("Cast", KeyCode.C, ECrawlerStates.SelectAlly, 
                extraData: selectAction));
            stateData.Actions.Add(new CrawlerStateAction("Vendor", KeyCode.V, ECrawlerStates.Vendor));

            stateData.Actions.Add(new CrawlerStateAction("Fight", KeyCode.F, ECrawlerStates.StartCombat));
          

            await UniTask.CompletedTask;
            return stateData;

        }
    }
}
