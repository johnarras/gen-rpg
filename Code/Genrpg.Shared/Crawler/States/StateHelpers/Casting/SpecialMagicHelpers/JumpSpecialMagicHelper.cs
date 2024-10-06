using Genrpg.Shared.Crawler.Maps.Constants;
using Genrpg.Shared.Crawler.Maps.Entities;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.Spells.Settings;
using Genrpg.Shared.Spells.Constants;
using Genrpg.Shared.Spells.Settings.SpecialMagic;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Utils.Data;
using System.Threading;
using System.Threading.Tasks;

using Genrpg.Shared.Crawler.States.StateHelpers.Selection.Entities;
using Genrpg.Shared.Crawler.States.Entities;
using Genrpg.Shared.Crawler.States.Constants;
using Genrpg.Shared.UI.Services;
using Genrpg.Shared.UI.Constants;

namespace Genrpg.Shared.Crawler.States.StateHelpers.Casting.SpecialMagicHelpers
{
    public class JumpSpecialMagicHelper : BaseSpecialMagicHelper
    {
        public override long GetKey() { return SpecialMagics.Jump; }


        public override async Task<CrawlerStateData> HandleEffect(CrawlerStateData stateData,
            SelectSpellAction action, CrawlerSpell spell, CrawlerSpellEffect effect, CancellationToken token)
        {

            SpecialMagic magic = _gameData.Get<SpecialMagicSettings>(null).Get(effect.EntityId);

            PartyData partyData = _crawlerService.GetParty();
            CrawlerWorld world = await _worldService.GetWorld(partyData.WorldId);

            CrawlerMap map = world.GetMap(partyData.MapId);

            if (!string.IsNullOrEmpty(action.PreviousError))
            {

                stateData.Actions.Add(new CrawlerStateAction(_textService.HighlightText(action.PreviousError, TextColors.ColorRed)));
            }

            stateData.Actions.Add(new CrawlerStateAction("Jump from 1-9 squares"));

            stateData.AddInputField("Jump: ", delegate (string text)
            {
                if (!string.IsNullOrEmpty(text))
                {
                    OnJump(partyData, action.Action.Member, map, action, text, token);
                }
            });

            stateData.Actions.Add(new CrawlerStateAction("Escape", CharCodes.Escape, ECrawlerStates.SelectSpell));

            await Task.CompletedTask;
            return stateData;
        }

        private void OnJump(PartyData partyData, PartyMember member, CrawlerMap map, SelectSpellAction action, string text,
            CancellationToken token)
        {
            if (!int.TryParse(text, out int distance) || distance < 1 || distance > 9)
            {
                CrawlerStateData stateData = new CrawlerStateData(ECrawlerStates.SpecialSpellCast, true)
                {
                    ExtraData = action,
                };
                action.PreviousError = "The jump distance must be between 1 and 9 spaces.";
                _crawlerService.ChangeState(ECrawlerStates.SpecialSpellCast, token, action);

            }

            _logService.Info("Ok Jump Distance Input" + distance);


            PointXZ dir = DirUtils.AxisAngleToDirDelta((partyData.MapRot + 90) % 360 / 90 * 90);

            if (dir == null)
            {

                _crawlerService.ChangeState(ECrawlerStates.Error, token, "Bad party direction");
                return;
            }
            _spellService.RemoveSpellPowerCost(partyData, member, action.Spell);

            int cx = partyData.MapX;
            int cz = partyData.MapZ;
            for (int i = 0; i < distance; i++)
            {
                if (_mapService.GetBlockingBits(cx, cz, cx + dir.X, cz + dir.Z, false) != WallTypes.None)
                {

                    _crawlerService.ChangeState(ECrawlerStates.Error, token, "Path is blocked");
                    return;
                }
                cx += dir.X;
                cz += dir.Z;
            }

            _mapService.MovePartyTo(partyData, cx, cz, partyData.MapRot, token);
            _crawlerService.ChangeState(ECrawlerStates.ExploreWorld, token);
        }
    }
}
