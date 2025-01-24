using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.Roles.Constants;
using Genrpg.Shared.Crawler.Roles.Services;
using Genrpg.Shared.Crawler.Roles.Settings;
using Genrpg.Shared.Crawler.States.Constants;
using Genrpg.Shared.Crawler.States.Entities;
using Genrpg.Shared.Crawler.States.StateHelpers.Combat;
using Genrpg.Shared.Crawler.States.StateHelpers.Selection.Entities;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.Shared.Crawler.States.StateHelpers.Selection
{
    public class SelectAllyStateHelper : BaseCombatStateHelper
    {
        public override ECrawlerStates GetKey() { return ECrawlerStates.SelectAlly; }

        public override async Task<CrawlerStateData> Init(CrawlerStateData currentData, CrawlerStateAction action, CancellationToken token)
        {
            CrawlerStateData stateData = CreateStateData();

            PartyData party = _crawlerService.GetParty();
            List<PartyMember> partyMembers = party.GetActiveParty();

            SelectSpellAction spellAction = new SelectSpellAction();

            for (int m = 0; m < partyMembers.Count; m++)
            {
                PartyMember partyMember = partyMembers[m];
                char c = (char)('a' + m);

                SelectAction selectAction = new SelectAction()
                {
                    Member = partyMember,
                    ReturnState = ECrawlerStates.SelectAlly,
                    NextState = ECrawlerStates.WorldCast,
                };


                Role classRole = _gameData.Get<RoleSettings>(_gs.ch).GetRoles(partyMember.Roles).FirstOrDefault(x => x.RoleCategoryId == RoleCategories.Class);


                Action ptrEnterAction = null;

                if (classRole != null)
                {
                    ptrEnterAction = () => { ShowInfo(EntityTypes.Role, classRole.IdKey); };
                }

                stateData.Actions.Add(new CrawlerStateAction(char.ToUpper(c) + partyMember.Name, c,
                  ECrawlerStates.SelectSpell, extraData: selectAction,

                    pointerEnterAction: ptrEnterAction));
            }

            stateData.Actions.Add(new CrawlerStateAction("", CharCodes.Escape, ECrawlerStates.ExploreWorld));


            await Task.CompletedTask;
            return stateData;
        }
    }
}
