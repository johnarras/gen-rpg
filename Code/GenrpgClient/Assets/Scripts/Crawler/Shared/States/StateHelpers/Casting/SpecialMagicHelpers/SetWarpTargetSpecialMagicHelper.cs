using Genrpg.Shared.Crawler.Maps.Entities;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.Spells.Settings;
using Genrpg.Shared.Spells.Constants;
using Genrpg.Shared.Spells.Settings.SpecialMagic;
using System.Threading;
using System.Threading.Tasks;

using Genrpg.Shared.Utils;
using Genrpg.Shared.Crawler.States.StateHelpers.Selection.Entities;
using Genrpg.Shared.Crawler.States.Entities;
using Genrpg.Shared.UI.Services;
using Genrpg.Shared.Crawler.States.Constants;

namespace Genrpg.Shared.Crawler.States.StateHelpers.Casting.SpecialMagicHelpers
{
    public class SetWarpTargetSpecialMagicHelper : BaseSpecialMagicHelper
    {
        public override long GetKey() { return SpecialMagics.SetWarpTarget; }


        public override async Task<CrawlerStateData> HandleEffect(CrawlerStateData stateData,
            SelectSpellAction action, CrawlerSpell spell, CrawlerSpellEffect effect, CancellationToken token)
        {
            SpecialMagic magic = _gameData.Get<SpecialMagicSettings>(null).Get(effect.EntityId);

            PartyData partyData = _crawlerService.GetParty();
            CrawlerWorld world = await _worldService.GetWorld(partyData.WorldId);

            CrawlerMap map = world.GetMap(partyData.MapId);

            action.Action.Member.WarpMapId = partyData.MapId;
            action.Action.Member.WarpMapX = partyData.MapX;
            action.Action.Member.WarpMapZ = partyData.MapZ;
            action.Action.Member.WarpRot = partyData.MapRot;

            stateData.Actions.Add(new CrawlerStateAction("Warp terget set."));
            stateData.Actions.Add(new CrawlerStateAction("Press " + _textService.HighlightText("Space") + " to return to the map.", CharCodes.Space, ECrawlerStates.ExploreWorld));

            await Task.CompletedTask;
            return stateData;
        }
    }
}
