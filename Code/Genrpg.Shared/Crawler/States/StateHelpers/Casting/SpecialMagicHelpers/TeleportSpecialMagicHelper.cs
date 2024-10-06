using Genrpg.Shared.Crawler.Maps.Entities;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.Spells.Settings;
using Genrpg.Shared.Spells.Constants;
using Genrpg.Shared.Spells.Settings.SpecialMagic;
using Genrpg.Shared.Utils;
using System.Threading;
using System.Threading.Tasks;
using Genrpg.Shared.Crawler.States.StateHelpers.Selection.Entities;
using Genrpg.Shared.Crawler.States.Entities;
using Genrpg.Shared.Crawler.States.Constants;
using Genrpg.Shared.UI.Constants;

namespace Genrpg.Shared.Crawler.States.StateHelpers.Casting.SpecialMagicHelpers
{
    public class TeleportSpecialMagicHelper : BaseSpecialMagicHelper
    {
        public override long GetKey() { return SpecialMagics.Teleport; }


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

            stateData.Actions.Add(new CrawlerStateAction("Teleport to any (X,Y) coordinate\nin the current map."));

            stateData.AddInputField("X: ", delegate (string text)
            {
                if (!string.IsNullOrEmpty(text))
                {
                    OnJump(partyData, action.Action.Member, map, action, text, stateData, token);
                }
            });

            stateData.AddInputField("Y: ", delegate (string text)
            {
                if (!string.IsNullOrEmpty(text))
                {
                    OnJump(partyData, action.Action.Member, map, action, text, stateData, token);
                }
            });

            stateData.Actions.Add(new CrawlerStateAction("Escape", CharCodes.Escape, ECrawlerStates.SelectSpell));

            await Task.CompletedTask;
            return stateData;
        }

        private void OnJump(PartyData partyData, PartyMember member, CrawlerMap map, SelectSpellAction action, string text,
            CrawlerStateData currState,
         CancellationToken token)
        {
            if (currState.Inputs.Count < 2)
            {
                _crawlerService.ChangeState(ECrawlerStates.ExploreWorld, token);
            }

            if (!int.TryParse(currState.Inputs[0].InputField.GetText(), out int x))
            {
                return;
            }

            if (!int.TryParse(currState.Inputs[1].InputField.GetText(), out int z))
            {
                return;
            }

            if (x < 0 || x >= map.Width || z < 0 || z >= map.Height)
            {
                CrawlerStateData stateData = new CrawlerStateData(ECrawlerStates.SpecialSpellCast, true)
                {
                    ExtraData = action,
                };
                action.PreviousError = "Those coordinates are out of bounds.";
                _crawlerService.ChangeState(ECrawlerStates.SpecialSpellCast, token, action);
                return;

            }

            if (map.Get(x, z, CellIndex.Terrain) == 0)
            {
                CrawlerStateData stateData = new CrawlerStateData(ECrawlerStates.SpecialSpellCast, true)
                {
                    ExtraData = action,
                };
                action.PreviousError = "That is not a valid target location.";
                _crawlerService.ChangeState(ECrawlerStates.SpecialSpellCast, token, action);
                return;

            }

            _spellService.RemoveSpellPowerCost(partyData, member, action.Spell);
            _mapService.MovePartyTo(partyData, x, z, partyData.MapRot, token);
            _crawlerService.ChangeState(ECrawlerStates.ExploreWorld, token);
        }
    }
}
