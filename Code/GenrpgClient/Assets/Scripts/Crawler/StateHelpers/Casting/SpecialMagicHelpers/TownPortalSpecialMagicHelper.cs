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
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Crawler.MapGen.Constants;

namespace Assets.Scripts.Crawler.StateHelpers.Casting.SpecialMagicHelpers
{
    public class TownPortalSpecialMagicHelper : BaseSpecialMagicHelper
    {
        public override long GetKey() { return SpecialMagics.TownPortal; }


        public override async Awaitable<CrawlerStateData> HandleEffect(CrawlerStateData stateData,
            SelectSpellAction action, CrawlerSpell spell, CrawlerSpellEffect effect, CancellationToken token)
        {
            SpecialMagic magic = _gameData.Get<SpecialMagicSettings>(null).Get(effect.EntityId);

            PartyData party = _crawlerService.GetParty();
            CrawlerWorld world = await _worldService.GetWorld(party.WorldId);

            CrawlerMap overworld = world.GetMap(1);

            List<CrawlerMap> cities = world.Maps.Where(x => x.CrawlerMapTypeId == CrawlerMapTypes.City).OrderBy(x => x.Level).ToList();

            foreach (CrawlerMap cityMap in cities) 
            {
                MapCellDetail entrance = overworld.Details.FirstOrDefault(x => x.EntityTypeId == EntityTypes.Map && x.EntityId == cityMap.IdKey);

                if (entrance == null)
                {
                    continue;
                }

                int ptx = 0;
                int ptz = 0;

                if (entrance.ToX == 0)
                {
                    ptx = 1;
                }
                else if (entrance.ToX == cityMap.Width-1)
                {
                    ptx = -1;
                }
                else if (entrance.ToZ == 0)
                {
                    ptz = 1;
                }
                else
                {
                    ptz = -1;
                }

                int newRot = (DirUtils.DirDeltaToAngle(ptx, ptz)+270)%360;

                EnterCrawlerMapData mapData = new EnterCrawlerMapData()
                {
                    MapId = entrance.EntityId,
                    MapX = entrance.ToX,
                    MapZ = entrance.ToZ,
                    MapRot = newRot,
                    World = world,
                    Map = cityMap,
                };

                stateData.Actions.Add(new CrawlerStateAction(cityMap.Name, KeyCode.None, ECrawlerStates.ExploreWorld,
                   () =>
                   {
                       _spellService.RemoveSpellPowerCost(party, action.Action.Member, action.Spell);
                   },
                   mapData));
            }

            if (!String.IsNullOrEmpty(action.PreviousError))
            {

                stateData.Actions.Add(new CrawlerStateAction(CrawlerUIUtils.HighlightText(action.PreviousError, CrawlerUIUtils.ColorRed)));
            }

            stateData.Actions.Add(new CrawlerStateAction("Escape", KeyCode.Escape, ECrawlerStates.SelectSpell));

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


            PointXZ dir = DirUtils.AxisAngleToDirDelta((((partyData.MapRot + 90) % 360) / 90) * 90);

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
