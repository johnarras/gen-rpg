using Assets.Scripts.Crawler.CrawlerStates;
using Assets.Scripts.Crawler.Maps.Constants;
using Assets.Scripts.Crawler.Maps.Entities;
using Assets.Scripts.Crawler.StateHelpers.Selection.Entities;
using Assets.Scripts.Crawler.UI.Utils;
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
using TMPro;
using UnityEngine;

namespace Assets.Scripts.Crawler.StateHelpers.Casting.SpecialMagicHelpers
{
    public class TeleportSpecialMagicHelper : BaseSpecialMagicHelper
    {
        public override long GetKey() { return SpecialMagics.Teleport; }


        public override async Awaitable<CrawlerStateData> HandleEffect(CrawlerStateData stateData,
            SelectSpellAction action, CrawlerSpell spell, CrawlerSpellEffect effect, CancellationToken token)
        {
            SpecialMagic magic = _gameData.Get<SpecialMagicSettings>(null).Get(effect.EntityId);

            PartyData partyData = _crawlerService.GetParty();
            CrawlerWorld world = await _worldService.GetWorld(partyData.WorldId);

            CrawlerMap map = world.GetMap(partyData.MapId);

            if (!String.IsNullOrEmpty(action.PreviousError))
            {
                stateData.Actions.Add(new CrawlerStateAction(CrawlerUIUtils.HighlightText(action.PreviousError, CrawlerUIUtils.ColorRed)));
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

            stateData.Actions.Add(new CrawlerStateAction("Escape", KeyCode.Escape, ECrawlerStates.SelectSpell));

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

            if (!Int32.TryParse(currState.Inputs[0].InputField.Input.Text, out int x))
            {
                return;
            }

            if (!Int32.TryParse(currState.Inputs[1].InputField.Input.Text, out int z))
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

            if (map.Get(x,z,CellIndex.Terrain) == 0)
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
