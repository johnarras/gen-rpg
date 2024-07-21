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
    public class SetWarpTargetSpecialMagicHelper : BaseSpecialMagicHelper
    {
        public override long GetKey() { return SpecialMagics.SetWarpTarget; }


        public override async Awaitable<CrawlerStateData> HandleEffect(CrawlerStateData stateData,
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
            stateData.Actions.Add(new CrawlerStateAction("Press " + CrawlerUIUtils.HighlightText("Space") + " to return to the map.",KeyCode.Space, ECrawlerStates.ExploreWorld));

            await Task.CompletedTask;
            return stateData;
        }
    }
}
