using Genrpg.Shared.Crawler.Maps.Services;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.States.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Crawler.UI.WorldUI
{
    public class PartyBuffUI : BaseBehaviour
    {

        protected ICrawlerService _crawlerService = null;
        protected ICrawlerWorldService _worldService = null;
        protected ICrawlerMapService _crawlerMapService = null;

        public int PartyBuffId;
        public GameObject ContentRoot;

        public override void Init()
        {
            _updateService.AddUpdate(this, FrameUpdate, UpdateTypes.Regular, GetToken());
        }

        protected virtual void FrameUpdateInternal(PartyData partyData)
        {

        }

        protected void FrameUpdate()
        {
            PartyData partyData = _crawlerService.GetParty();

            if (partyData == null || partyData.Buffs.Get(PartyBuffId) == 0)
            {
                _clientEntityService.SetActive(ContentRoot, false);
            }
            else
            {
                _clientEntityService.SetActive(ContentRoot, true);
                FrameUpdateInternal(partyData);
            }

        }
    }
}
