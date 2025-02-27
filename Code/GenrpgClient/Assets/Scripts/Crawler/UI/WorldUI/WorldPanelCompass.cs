
using Genrpg.Shared.Crawler.Buffs.Constants;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.MapServer.Entities;
using System.Drawing.Drawing2D;
using System.IO.Ports;
using UnityEngine;

namespace Assets.Scripts.Crawler.UI.WorldUI
{
    public class WorldPanelCompass : PartyBuffUI
    { 
        public GImage CompassDirection;

        protected override void FrameUpdateInternal(PartyData partyData)
        { 
            RectTransform rectTransform = CompassDirection.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                int mapRot = partyData.MapRot;
                if (mapRot % 180 == 0)
                {
                    mapRot += 90;
                }
                else
                {
                    mapRot -= 90;
                }
                rectTransform.localEulerAngles = new Vector3(0, 0, mapRot);
            }
        }
    }
}
