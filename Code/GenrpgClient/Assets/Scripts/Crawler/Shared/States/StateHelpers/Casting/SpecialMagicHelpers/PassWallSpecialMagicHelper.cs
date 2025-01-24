using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.Spells.Settings;
using Genrpg.Shared.Spells.Constants;
using Genrpg.Shared.Utils.Data;
using Genrpg.Shared.Utils;
using System.Threading;
using System.Threading.Tasks;
using Genrpg.Shared.Crawler.Maps.Entities;
using Genrpg.Shared.Crawler.States.StateHelpers.Selection.Entities;
using Genrpg.Shared.Crawler.States.Constants;

namespace Genrpg.Shared.Crawler.States.StateHelpers.Casting.SpecialMagicHelpers
{
    public class PassWallSpecialMagicHelper : BaseSpecialMagicHelper
    {
        public override long GetKey() { return SpecialMagics.PassWall; }


        public override async Task<CrawlerStateData> HandleEffect(CrawlerStateData stateData,
            SelectSpellAction action, CrawlerSpell spell, CrawlerSpellEffect effect, CancellationToken token)
        {

            PartyData party = _crawlerService.GetParty();

            CrawlerMap map = _worldService.GetMap(party.MapId);

            PointXZ dir = DirUtils.AxisAngleToDirDelta((party.MapRot + 90) % 360 / 90 * 90);

            if (dir == null)
            {
                return new CrawlerStateData(ECrawlerStates.Error, true) { ExtraData = "Improper map data found." };
            }

            int nx = party.MapX + dir.X;
            int nz = party.MapZ + dir.Z;

            if (map.Looping)
            {
                nx = (nx + map.Width) % map.Width;
                nz = (nz + map.Height) % map.Height;
            }

            if (nx < 0 || nz < 0 || nx >= map.Width || nz >= map.Height)
            {
                return new CrawlerStateData(ECrawlerStates.Error, true) { ExtraData = "That is out of bounds." };
            }

            if (map.Get(nx, nz, CellIndex.Terrain) == 0)
            {
                return new CrawlerStateData(ECrawlerStates.Error, true) { ExtraData = "The world does not exist there." };
            }

            _spellService.RemoveSpellPowerCost(party, action.Action.Member, action.Spell);

            _mapService.MovePartyTo(party, nx, nz, party.MapRot, token);
            await Task.CompletedTask;
            return new CrawlerStateData(ECrawlerStates.ExploreWorld, true);
        }
    }
}
