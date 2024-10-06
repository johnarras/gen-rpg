using Genrpg.Shared.Crawler.Maps.Entities;
using Genrpg.Shared.Crawler.Spells.Settings;
using Genrpg.Shared.Spells.Constants;
using System.Threading;
using System.Threading.Tasks;
using Genrpg.Shared.Crawler.States.StateHelpers.Selection.Entities;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.States.Constants;

namespace Genrpg.Shared.Crawler.States.StateHelpers.Casting.SpecialMagicHelpers
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
        public override async Task<CrawlerStateData> HandleEffect(CrawlerStateData stateData,
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
                map.Get(mapx, mapz, CellIndex.Terrain) == 0)
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
