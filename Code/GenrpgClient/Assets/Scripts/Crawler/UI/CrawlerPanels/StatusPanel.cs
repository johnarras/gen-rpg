using Assets.Scripts.Crawler.ClientEvents.StatusPanelEvents;
using Assets.Scripts.UI.Crawler.StatusUI;
using Genrpg.Shared.Client.Assets.Constants;
using Genrpg.Shared.Crawler.Monsters.Entities;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.Settings;
using Genrpg.Shared.MVC.Interfaces;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Assets.Scripts.UI.Crawler.CrawlerPanels
{
    public class StatusPanel : BaseCrawlerPanel
    {

        private List<PartyMemberStatusRow> _rows = new List<PartyMemberStatusRow>();

        private string StatusRowPrefab = "PartyMemberStatusRow";

        private object Content;

        public override async Task Init(CrawlerScreen screen, IView view, CancellationToken token)
        {
            await base.Init(screen, view, token);

            Content = view.Get<object>("Content");

            _dispatcher.AddListener<RefreshPartyStatus>(OnRefreshPartyStatus, GetToken());
            _dispatcher.AddListener<RefreshUnitStatus>(OnRefreshUnitStatus, GetToken());
            CrawlerSettings crawlerSettings = _gameData.Get<CrawlerSettings>(_gs.ch);

            for (int i = 0; i < crawlerSettings.CrawlerPartySize; i++)
            {
                _rows.Add(await _assetService.CreateAsync<PartyMemberStatusRow, int>(i + 1, AssetCategoryNames.UI, StatusRowPrefab, Content, token, screen.Subdirectory));
            }

            UpdatePartyData();

        }

        private void OnRefreshPartyStatus(RefreshPartyStatus status)
        {
            UpdatePartyData();
        }
        private void OnRefreshUnitStatus(RefreshUnitStatus status)
        {
            RefreshUnit(status.Unit, status.ElementTypeId);
        }

        private void UpdatePartyData(int partyIndexToUpdate = 0, long elementTypeId = 0)
        { 
            PartyData partyData = _crawlerService.GetParty();

            for (int r = 0; r < _rows.Count; r++)
            {
                if (partyIndexToUpdate > 0 && r+1 != partyIndexToUpdate)
                {
                    continue;
                }
                _rows[r].UpdateData(elementTypeId);
            }
        }

        public override async Task OnNewStateData(CrawlerStateData stateData, CancellationToken token)
        {
            await Task.Delay(10);
            UpdatePartyData();
        }

        public void RefreshUnit(CrawlerUnit unit, long elementTypeId = 0)
        {
            if (unit is PartyMember member)
            {
                UpdatePartyData(member.PartySlot, elementTypeId);
            }
        }
    }
}
