﻿using Genrpg.Shared.Crawler.Spells.Services;
using Genrpg.Shared.Crawler.States.Constants;
using Genrpg.Shared.Crawler.States.Entities;
using Genrpg.Shared.Crawler.States.StateHelpers.Selection.Entities;
using System.Threading;
using System.Threading.Tasks;


namespace Genrpg.Shared.Crawler.States.StateHelpers.Casting
{
    public class WorldCastingStateHelper : BaseStateHelper
    {
        ICrawlerSpellService _crawlerSpellService = null;

        public override ECrawlerStates GetKey() { return ECrawlerStates.WorldCast; }

        public override async Task<CrawlerStateData> Init(CrawlerStateData currentData, CrawlerStateAction action, CancellationToken token)
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
                return new CrawlerStateData(ECrawlerStates.Error, true) { ExtraData = "World spell had bad data" };
            }

            await _crawlerSpellService.CastSpell(_crawlerService.GetParty(), selectSpellAction.Action.Action);

            stateData = new CrawlerStateData(ECrawlerStates.ExploreWorld, true);


            return stateData;
        }
    }
}