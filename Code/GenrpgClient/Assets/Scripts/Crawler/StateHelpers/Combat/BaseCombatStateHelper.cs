using Genrpg.Shared.Crawler.Parties.PlayerData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Crawler.StateHelpers.Combat
{
    public abstract class BaseCombatStateHelper : BaseStateHelper
    {
        protected override CrawlerStateData CreateStateData()
        {
            CrawlerStateData stateData = base.CreateStateData();

            PartyData party = _crawlerService.GetParty();
            if (party.Combat != null && party.Combat.Enemies != null &&
                party.Combat.Enemies.Count > 0 &&
                party.Combat.Enemies[0].Units.Count > 0)
            {
                stateData.WorldSpriteName = party.Combat.Enemies[0].Units[0].PortraitName;
            }

            return stateData;
        }
    }
}
