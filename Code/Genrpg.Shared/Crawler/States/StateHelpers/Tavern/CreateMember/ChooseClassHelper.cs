
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.Roles.Constants;
using Genrpg.Shared.Crawler.Roles.Settings;
using Genrpg.Shared.Crawler.States.Constants;
using Genrpg.Shared.Crawler.States.Entities;
using Genrpg.Shared.Stats.Entities;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;


namespace Genrpg.Shared.Crawler.States.StateHelpers.Tavern.CreateMember
{
    public class ChooseClassHelper : BaseRoleStateHelper
    {
        public override ECrawlerStates GetKey() { return ECrawlerStates.ChooseClass; }

        public override async Task<CrawlerStateData> Init(CrawlerStateData currentState, CrawlerStateAction action, CancellationToken token)
        {
            CrawlerStateData stateData = CreateStateData();
            PartyMember member = action.ExtraData as PartyMember;

            IReadOnlyList<Role> roles = _gameData.Get<RoleSettings>(null).GetData().Where(x => x.RoleCategoryId == RoleCategories.Class).ToList();

            foreach (Role role in roles)
            {
                if (role.IdKey < 1)
                {
                    continue;
                }

                string desc = role.Desc;

                stateData.Actions.Add(new CrawlerStateAction(role.Name, CharCodes.None, ECrawlerStates.ChoosePortrait,
                    delegate
                    {
                        member.Roles.Add(new UnitRole() { RoleId = role.IdKey });
                    }, member, null, () => { OnPointerEnter(role); }

                    ));
            }

            stateData.Actions.Add(new CrawlerStateAction("Escape", CharCodes.Escape, ECrawlerStates.RollStats,
                delegate
                {
                    member.Stats = new StatGroup();
                    while (member.Roles.Count > 0)
                    {
                        member.Roles.RemoveAt(1);
                    }
                },
                extraData: member));
            await Task.CompletedTask;
            return stateData;

        }
    }
}
