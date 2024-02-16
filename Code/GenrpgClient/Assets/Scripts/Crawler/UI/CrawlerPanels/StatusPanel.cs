using Assets.Scripts.UI.Crawler.StatusUI;
using Cysharp.Threading.Tasks;
using Genrpg.Shared.Crawler.Monsters.Entities;
using Genrpg.Shared.Crawler.Parties.Constants;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.UI.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;

namespace Assets.Scripts.UI.Crawler.CrawlerPanels
{
    public class StatusPanel : BaseCrawlerPanel, IStatusPanel
    {

        public List<CrawlerStatusRow> Rows;

        public override async UniTask Init(CrawlerScreen screen, CancellationToken token)
        {
            await base.Init(screen, token);

            UpdatePartyData();

        }

        private void UpdatePartyData(int partyIndexToUpdate = 0)
        { 
            PartyData partyData = _crawlerService.GetParty();
            for (int r = 0; r <= PartyConstants.PartySize; r++)
            {
                if (Rows.Count >= r+1)
                {
                    if (partyIndexToUpdate > 0 && r != partyIndexToUpdate)
                    {
                        continue;
                    }
                    Rows[r].Init(partyData.GetMemberInSlot(r), r);
                }
            }
        }

        public override void OnNewStateData(CrawlerStateData stateData)
        {
            UpdatePartyData();
        }

        public void RefreshAll()
        {
            UpdatePartyData();
        }

        public void RefreshUnit(CrawlerUnit unit, string effect = null)
        {
            if (unit is PartyMember member)
            {
                UpdatePartyData(member.PartySlot);
            }
        }
    }
}
