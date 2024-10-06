using Assets.Scripts.UI.Crawler.StatusUI;
using Genrpg.Shared.Client.Assets.Constants;
using Genrpg.Shared.Crawler.Monsters.Entities;
using Genrpg.Shared.Crawler.Parties.Constants;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.UI.Interfaces;
using Genrpg.Shared.MVC.Interfaces;
using Genrpg.Shared.UI.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Assets.Scripts.UI.Crawler.CrawlerPanels
{
    public class StatusPanel : BaseCrawlerPanel, IStatusPanel
    {

        private List<PartyMemberStatusRow> _rows = new List<PartyMemberStatusRow>();

        private object Content;

        public override async Task Init(CrawlerScreen screen, IView view, CancellationToken token)
        {
            await base.Init(screen, view, token);

            Content = view.Get<object>("Content");

            for (int i = 0; i <= PartyConstants.MaxPartySize; i++)
            {
                _rows.Add(await _assetService.CreateAsync<PartyMemberStatusRow, int>(i, AssetCategoryNames.UI, "PartyMemberStatusRow", Content, token, screen.Subdirectory));
            }

            UpdatePartyData();

        }

        private void UpdatePartyData(int partyIndexToUpdate = 0)
        { 
            PartyData partyData = _crawlerService.GetParty();
            for (int r = 0; r <= PartyConstants.MaxPartySize; r++)
            {
                if (_rows.Count >= r+1)
                {
                    if (partyIndexToUpdate > 0 && r != partyIndexToUpdate)
                    {
                        continue;
                    }
                    _rows[r].UpdateText();
                }
            }
        }

        public override async Task OnNewStateData(CrawlerStateData stateData, CancellationToken token)
        {
            await Task.Delay(10);
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
