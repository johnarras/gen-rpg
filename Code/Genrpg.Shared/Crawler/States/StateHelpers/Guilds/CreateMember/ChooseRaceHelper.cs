using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.Roles.Constants;
using Genrpg.Shared.Crawler.Roles.Settings;
using Genrpg.Shared.Crawler.States.Constants;
using Genrpg.Shared.Crawler.States.Entities;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.Shared.Crawler.States.StateHelpers.Guilds.CreateMember
{
    public class ChooseRaceHelper : BaseRoleStateHelper
    {
        public override ECrawlerStates GetKey() { return ECrawlerStates.ChooseRace; }


        public override async Task<CrawlerStateData> Init(CrawlerStateData currentState, CrawlerStateAction action, CancellationToken token)
        {
            CrawlerStateData stateData = CreateStateData();

            PartyMember member = action.ExtraData as PartyMember;

            List<Role> races = _gameData.Get<RoleSettings>(_gs.ch).GetData().Where(x => x.RoleCategoryId == RoleCategories.Origin).ToList();

            foreach (Role race in races)
            {

                stateData.Actions.Add(new CrawlerStateAction(race.Name, CharCodes.None, ECrawlerStates.ChooseClass,
                    delegate
                    {
                        member.Roles.Add(new UnitRole() { RoleId = race.IdKey });
                        member.UnitTypeId = 1;
                    }, member, null, () => { OnPointerEnter(race); }
                    ));
            }

            stateData.Actions.Add(new CrawlerStateAction("Escape", CharCodes.Escape, ECrawlerStates.ChooseSex,
                delegate
                {
                    member.Spawn = null;
                    member.Roles.Clear();
                    member.UnitTypeId = 0;
                }, member));

            await Task.CompletedTask;
            return stateData;

        }
    }
}
