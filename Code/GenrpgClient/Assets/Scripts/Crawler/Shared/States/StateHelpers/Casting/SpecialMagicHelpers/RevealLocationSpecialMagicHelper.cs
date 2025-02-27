using Genrpg.Shared.Crawler.GameEvents;
using Genrpg.Shared.Crawler.Maps.Entities;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.Roles.Constants;
using Genrpg.Shared.Crawler.Spells.Settings;
using Genrpg.Shared.Crawler.States.Constants;
using Genrpg.Shared.Crawler.States.StateHelpers.Selection.Entities;
using Genrpg.Shared.Spells.Constants;
using Genrpg.Shared.Spells.Settings.SpecialMagic;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.Shared.Crawler.States.StateHelpers.Casting.SpecialMagicHelpers
{
    public class RevealLocationSpecialMagicHelper : BaseSpecialMagicHelper
    {


       
        public override long GetKey() { return SpecialMagics.RevealLocation; }


        public override async Task<CrawlerStateData> HandleEffect(CrawlerStateData stateData,
            SelectSpellAction action, CrawlerSpell spell, CrawlerSpellEffect effect, CancellationToken token)
        {
            SpecialMagic magic = _gameData.Get<SpecialMagicSettings>(null).Get(effect.EntityId);

            PartyData party = _crawlerService.GetParty();

            CrawlerMap map = _worldService.GetMap(party.MapId);

            double utilityTier = _roleService.GetScalingTier(party, action.Action.Member, RoleScalingTypes.Utility);

            int radius = (int)Math.Floor(Math.Sqrt(utilityTier));

            int cx = party.MapX;
            int cz = party.MapZ;

            for (int x = cx-radius; x <= cx+radius; x++)
            {
                int nx = x;

                if (nx < 0 || nx >= map.Width)
                {
                    if (!map.Looping)
                    {
                        continue;
                    }
                    nx = (nx + map.Width) % map.Width;
                }
                for (int z = cz - radius; z <= cz+radius; z++)
                {
                    int nz = z;
                    if (nz < 0 || nz >= map.Height)
                    {
                        if (!map.Looping)
                        {
                            continue;
                        }
                        nz = (nz + map.Height) % map.Height;
                    }

                    if (map.Get(nx, nz, CellIndex.Terrain) > 0)
                    {
                        _mapService.MarkCellVisited(party.MapId, nx, nz);
                    }
                }
            }

            _dispatcher.Dispatch(new ShowPartyMinimap() { Party = party });
            await Task.CompletedTask;
            return new CrawlerStateData(ECrawlerStates.ExploreWorld, true);
        }
    }
}
