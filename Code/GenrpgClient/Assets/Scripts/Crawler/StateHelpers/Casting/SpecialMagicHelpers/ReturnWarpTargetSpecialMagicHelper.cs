using Assets.Scripts.Crawler.CrawlerStates;
using Assets.Scripts.Crawler.Maps.Entities;
using Assets.Scripts.Crawler.StateHelpers.Selection.Entities;
using Assets.Scripts.Crawler.UI.Utils;
using Assets.Scripts.UI.Crawler.States;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.Spells.Settings;
using Genrpg.Shared.Spells.Constants;
using Genrpg.Shared.Spells.Settings.SpecialMagic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace Assets.Scripts.Crawler.StateHelpers.Casting.SpecialMagicHelpers
{
    public class ReturnWarpTargetSpecialMagicHelper : BaseSpecialMagicHelper
    {
        public override long GetKey() { return SpecialMagics.ReturnWarpTarget; }

        private void ClearWarpData(PartyMember member)
        {
            member.WarpMapZ = 0;
            member.WarpMapX = 0;   
            member.WarpMapId = 0;
            member.WarpRot = 0;
        }
        public override async Awaitable<CrawlerStateData> HandleEffect(CrawlerStateData stateData,
            SelectSpellAction action, CrawlerSpell spell, CrawlerSpellEffect effect, CancellationToken token)
        {
            PartyData party = _crawlerService.GetParty();
            CrawlerWorld world = await _worldService.GetWorld(party.WorldId);

            PartyMember member = action.Action.Member;

            long mapId = member.WarpMapId;

            if (mapId == 0)
            {
                ClearWarpData(member);
                return new CrawlerStateData(ECrawlerStates.Error, true) { ExtraData = "Warp target was never set." };
            }


            CrawlerMap map = world.GetMap(action.Action.Member.WarpMapId);

            if (map == null)
            {
                ClearWarpData(member);
                return new CrawlerStateData(ECrawlerStates.Error, true) { ExtraData = "That map doesn't exist." };
            }

            int mapx = action.Action.Member.WarpMapX;
            int mapz = action.Action.Member.WarpMapZ;


            if (mapx < 0 || mapx >= map.Width ||
                mapz < 0 || mapz >= map.Height ||
                map.Get((int)mapx,(int)mapz,CellIndex.Terrain) == 0)
            {
                ClearWarpData(member);
                return new CrawlerStateData(ECrawlerStates.Error, true) { ExtraData = "That is not a valid target location." };
            }

            EnterCrawlerMapData mapData = new EnterCrawlerMapData()
            {
                MapId = member.WarpMapId,
                MapX = member.WarpMapX,
                MapZ = member.WarpMapZ,
                MapRot = member.WarpRot,
                World = world,
                Map = map,
            };



            stateData = new CrawlerStateData(ECrawlerStates.ExploreWorld, true) { ExtraData = mapData };
            return stateData;
        }
    }
}
