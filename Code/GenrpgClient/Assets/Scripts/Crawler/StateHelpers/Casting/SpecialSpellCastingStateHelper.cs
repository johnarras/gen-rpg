using Assets.Scripts.Crawler.CrawlerStates;
using Assets.Scripts.Crawler.StateHelpers.Selection.Entities;
using Assets.Scripts.UI.Crawler.States;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Assets.Scripts.Crawler.StateHelpers.Casting
{
    public class SpecialSpellCastingStateHelper : BaseStateHelper
    {
        public override ECrawlerStates GetKey() { return ECrawlerStates.SpecialSpellCast; }

        public override async UniTask<CrawlerStateData> Init(CrawlerStateData currentData, CrawlerStateAction action, CancellationToken token)
        {
            CrawlerStateData stateData = CreateStateData();

            _logService.Info("Special spellcast");

            SelectSpellAction selectSpellAction = action.ExtraData as SelectSpellAction;

            if (selectSpellAction == null)
            {
                return new CrawlerStateData(ECrawlerStates.Error, true) { ErrorMessage = "Missing Special Select Spell" };
            }

            await UniTask.CompletedTask;
            return new CrawlerStateData(ECrawlerStates.ExploreWorld, true);
        }
    }
}
