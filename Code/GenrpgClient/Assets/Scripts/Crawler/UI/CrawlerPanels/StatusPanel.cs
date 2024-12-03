using Assets.Scripts.UI.Crawler.StatusUI;
using Genrpg.Shared.Client.Assets.Constants;
using Genrpg.Shared.Core.Constants;
using Genrpg.Shared.Crawler.Monsters.Entities;
using Genrpg.Shared.Crawler.Parties.Constants;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.UI.Interfaces;
using Genrpg.Shared.MVC.Interfaces;
using Genrpg.Shared.UI.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
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


        private PartyMemberStatusRow _topRow = null;
        private List<PartyMemberStatusRow> _rows = new List<PartyMemberStatusRow>();

        private string StatusRowPrefab = "PartyMemberStatusRow";

        private object Content;

        public override async Task Init(CrawlerScreen screen, IView view, CancellationToken token)
        {
            await base.Init(screen, view, token);

            Content = view.Get<object>("Content");

            if (_gs.GameMode == EGameModes.Roguelike)
            {
                _rows.Add(await _assetService.CreateAsync<PartyMemberStatusRow, int>(1, AssetCategoryNames.UI, "RoguelikeStatusRow", Content, token, screen.Subdirectory));
            }
            else if (_gs.GameMode == EGameModes.Crawler2)
            {
                StatusRowPrefab += "2";
                for (int i = 0; i < PartyConstants.MaxPartySize; i++)
                {
                    _rows.Add(await _assetService.CreateAsync<PartyMemberStatusRow, int>(i + 1, AssetCategoryNames.UI, StatusRowPrefab, Content, token, screen.Subdirectory));
                }
            }
            else
            {
                _topRow = await _assetService.CreateAsync<PartyMemberStatusRow, int>(0, AssetCategoryNames.UI, StatusRowPrefab, Content, token, screen.Subdirectory);
                _topRow.UpdateData();
                for (int i = 0; i < PartyConstants.MaxPartySize; i++)
                {
                    _rows.Add(await _assetService.CreateAsync<PartyMemberStatusRow, int>(i+1, AssetCategoryNames.UI, StatusRowPrefab, Content, token, screen.Subdirectory));
                }
            }

            UpdatePartyData();

        }

        private void UpdatePartyData(int partyIndexToUpdate = 0)
        { 
            PartyData partyData = _crawlerService.GetParty();

            for (int r = 0; r < _rows.Count; r++)
            {
                if (partyIndexToUpdate > 0 && r+1 != partyIndexToUpdate)
                {
                    continue;
                }
                _rows[r].UpdateData();
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
