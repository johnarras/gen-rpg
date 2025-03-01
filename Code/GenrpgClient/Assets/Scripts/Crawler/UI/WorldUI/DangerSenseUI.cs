using Genrpg.Shared.Crawler.Buffs.Constants;
using Genrpg.Shared.Crawler.Maps.Constants;
using Genrpg.Shared.Crawler.Maps.Entities;
using Genrpg.Shared.Crawler.Maps.Services;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.States.Services;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Crawler.UI.WorldUI
{
    public class DangerSenseUI : AnimatedPartyBuffUI
    {
        protected override void FrameUpdateInternal(PartyData partyData)
        {

            CrawlerMap map = _worldService.GetMap(partyData.MapId);

            if (map == null)
            {
                return;
            }

            float sin = (float)Math.Round(MathF.Sin(-partyData.MapRot * Mathf.PI / 180f));
            float cos = (float)Math.Round(Mathf.Cos(-partyData.MapRot * Mathf.PI / 180f));

            float nx = cos * 1;
            float nz = sin * 1;

            int sx = partyData.MapX;
            int sz = partyData.MapZ;

            int ex = (int)(partyData.MapX + nx);
            int ez = (int)(partyData.MapZ + nz);

            int dx = ex - sx;
            int dz = ez - sz;

            int distance = 1;

            bool haveDanger = false;
            for (int d = 1; d <= distance; d++)
            {
                int cx = sx + dx * d;
                int cz = sz + dz * d;

                if (cx < 0 || cz < 0 || cz >= map.Width || cx > map.Height)
                {
                    if (!map.Looping)
                    {
                        return;
                    }
                    cx = (cx + map.Width) % map.Width;
                    cz = (cz + map.Height) % map.Height;
                }

                if (partyData.CurrentMap.Cleansed.HasBit(map.GetIndex(cx,cz)))
                {
                    continue;
                }

                if (!partyData.CurrentMap.Visited.HasBit(map.GetIndex(cx, cz)))
                {

                    int encounter = map.Get(cx, cz, CellIndex.Encounter);

                    if (encounter > 0 && encounter != MapEncounters.Treasure && encounter != MapEncounters.Stats)
                    {
                        haveDanger = true;
                        break;
                    }
                }

                if (_crawlerMapService.GetMagicBits(partyData.MapId, cx, cz) > 0)
                {
                    haveDanger = true;
                    break;
                }
            }

            Sprite.OnlyShowFirstFrame = !haveDanger;
        }
    }
}
