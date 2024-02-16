using Assets.Scripts.Crawler.CrawlerStates;
using Assets.Scripts.Crawler.StateHelpers.Selection.Entities;
using Assets.Scripts.UI.Crawler.States;
using Cysharp.Threading.Tasks;
using Genrpg.Shared.Crawler.Spells.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Assets.Scripts.Crawler.StateHelpers.Casting
{
    public class WorldCastingStateHelper : BaseStateHelper
    {
        ICrawlerSpellService _crawlerSpellService = null;

        public override ECrawlerStates GetKey() { return ECrawlerStates.WorldCast; }

        public override async UniTask<CrawlerStateData> Init(UnityGameState gs, CrawlerStateData currentData, CrawlerStateAction action, CancellationToken token)
        {
            CrawlerStateData stateData = CreateStateData();

            SelectSpellAction selectSpellAction = action.ExtraData as 
                SelectSpellAction;

            if (selectSpellAction == null ||
                selectSpellAction.Spell == null ||
                selectSpellAction.Action == null ||
                selectSpellAction.Action.Action == null ||
                selectSpellAction.Action.Action.FinalTargets == null)
            {
                return new CrawlerStateData(ECrawlerStates.Error, true) { ErrorMessage = "World spell had bad data" };
            }

            await _crawlerSpellService.CastSpell(gs, _crawlerService.GetParty(), selectSpellAction.Action.Action);

            stateData = new CrawlerStateData(ECrawlerStates.ExploreWorld, true);

            await UniTask.CompletedTask;
            return stateData;
        }
    }
}
