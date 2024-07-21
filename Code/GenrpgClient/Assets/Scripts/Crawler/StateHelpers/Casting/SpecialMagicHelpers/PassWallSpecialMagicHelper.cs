using Assets.Scripts.Crawler.StateHelpers.Selection.Entities;
using Assets.Scripts.UI.Crawler.States;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.Spells.Settings;
using Genrpg.Shared.Spells.Constants;
using Genrpg.Shared.Spells.Settings.SpecialMagic;
using Genrpg.Shared.Utils.Data;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Assets.Scripts.Crawler.Maps.Entities;
using System.Configuration;
using System.Drawing.Drawing2D;

namespace Assets.Scripts.Crawler.StateHelpers.Casting.SpecialMagicHelpers
{
    public class PassWallSpecialMagicHelper : BaseSpecialMagicHelper
    {
        public override long GetKey() { return SpecialMagics.PassWall; }


        public override async Awaitable<CrawlerStateData> HandleEffect(CrawlerStateData stateData,
            SelectSpellAction action, CrawlerSpell spell, CrawlerSpellEffect effect, CancellationToken token)
        {

            PartyData party = _crawlerService.GetParty();

            CrawlerMap map = _worldService.GetMap(party.MapId);

            PointXZ dir = DirUtils.AxisAngleToDirDelta((((party.MapRot + 90) % 360) / 90) * 90);

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

            if (map.Get(nx,nz,CellIndex.Terrain) == 0)
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
