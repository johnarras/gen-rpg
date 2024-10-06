using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.States.Constants;
using Genrpg.Shared.Crawler.States.Entities;
using Genrpg.Shared.Units.Settings;
using Genrpg.Shared.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.Shared.Crawler.States.StateHelpers.Tavern.CreateMember
{
    public class ChoosePortraitHelper : BaseStateHelper
    {
        public override ECrawlerStates GetKey() { return ECrawlerStates.ChoosePortrait; }

        public override async Task<CrawlerStateData> Init(CrawlerStateData currentState, CrawlerStateAction action, CancellationToken token)
        {
            CrawlerStateData stateData = CreateStateData();
            IReadOnlyList<UnitType> allUnitTypes = _gameData.Get<UnitSettings>(null).GetData();

            PartyMember member = action.ExtraData as PartyMember;

            allUnitTypes = allUnitTypes.OrderBy(x => x.Name).ToList();

            foreach (UnitType unitType in allUnitTypes)
            {
                if (unitType.IdKey < 1 || string.IsNullOrEmpty(unitType.Icon))
                {
                    continue;
                }
                stateData.Actions.Add(new CrawlerStateAction(unitType.Name, CharCodes.None, ECrawlerStates.ChooseName,
                   delegate
                   {
                       member.PortraitName = unitType.Icon;
                   }, member, unitType.Icon
                   ));
            }

            stateData.Actions.Add(new CrawlerStateAction("Escape", CharCodes.Escape, ECrawlerStates.ChooseClass,
                delegate { member.PortraitName = null; }, member));


            await Task.CompletedTask;

            return stateData;
        }
    }
}
