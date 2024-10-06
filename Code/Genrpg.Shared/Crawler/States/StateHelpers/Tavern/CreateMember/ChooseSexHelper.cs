using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.States.Constants;
using Genrpg.Shared.Crawler.States.Entities;
using Genrpg.Shared.Crawler.States.StateHelpers;
using Genrpg.Shared.Factions.Constants;
using Genrpg.Shared.Sexes.Settings;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.Shared.Crawler.States.StateHelpers.Tavern.CreateMember
{
    public class ChooseSexHelper : BaseStateHelper
    {
        public override ECrawlerStates GetKey() { return ECrawlerStates.ChooseSex; }

        public override async Task<CrawlerStateData> Init(CrawlerStateData currentState, CrawlerStateAction action, CancellationToken token)
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
                stateData.Actions.Add(new CrawlerStateAction(sex.Name, char.ToLower(sex.Name[0]), ECrawlerStates.ChooseRace,
                    delegate { member.SexTypeId = sex.IdKey; }, member));
            }

            stateData.Actions.Add(new CrawlerStateAction("Escape", CharCodes.Escape, ECrawlerStates.TavernMain, null, null));

            await Task.CompletedTask;
            return stateData;

        }
    }
}
