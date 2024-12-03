using Genrpg.Shared.Core.Constants;
using Genrpg.Shared.Crawler.GameEvents;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.Roguelikes.Services;
using Genrpg.Shared.Crawler.Roguelikes.Settings;
using Genrpg.Shared.Crawler.States.Constants;
using Genrpg.Shared.Crawler.States.Entities;
using Genrpg.Shared.UI.Constants;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.Shared.Crawler.States.StateHelpers.Guilds.Upgrades
{
    public class UpgradePartyStateHelper : BaseStateHelper
    {

        private IRoguelikeUpgradeService _roguelikeUpgradeService;
        public override ECrawlerStates GetKey() {  return ECrawlerStates.UpgradeParty; }

        public override async Task<CrawlerStateData> Init(CrawlerStateData currentData, CrawlerStateAction action, CancellationToken token)
        {
            CrawlerStateData stateData = CreateStateData();

            string oldErrorText = action.ExtraData as String;



            PartyData partyData = _crawlerService.GetParty();

            stateData.Actions.Add(new CrawlerStateAction("Roguelike Upgrades:\n", forceText: true));
            if (!string.IsNullOrEmpty(oldErrorText))
            {
                stateData.Actions.Add(new CrawlerStateAction(_textService.HighlightText(oldErrorText, TextColors.ColorRed)));
            }
            else
            {
                stateData.Actions.Add(new CrawlerStateAction(" \n", forceText:true));
            }

            string errorText = (partyData.Members.Count > 0 ? "You can only change upgrades with 0 characters." : null);

            stateData.Actions.Add(new CrawlerStateAction($"Reset Points ({partyData.UpgradePoints} available)", CharCodes.None, ECrawlerStates.UpgradeParty,
                () => 
                { 
                    _roguelikeUpgradeService.ResetPoints(partyData); 

                }, errorText, forceText: true));

            RoguelikeUpgradeSettings settings = _gameData.Get<RoguelikeUpgradeSettings>(_gs.ch);

            StringBuilder sb = new StringBuilder();
            foreach (RoguelikeUpgrade upgrade in settings.GetData())
            {
                sb.Clear();

                long currTier = partyData.GetUpgradeTier(upgrade.IdKey);
                long nextTier = currTier + 1;

                sb.Append(upgrade.Name + "[T" + partyData.GetUpgradeTier(upgrade.IdKey) + " +" + _roguelikeUpgradeService.GetBonus(partyData, upgrade.IdKey) + "]");

                long nextUpgradeCost = _roguelikeUpgradeService.GetUpgradeCost(upgrade.IdKey, nextTier);

                if (nextUpgradeCost > 0)
                {
                    sb.Append(" N: [$" + nextUpgradeCost);
                    sb.Append(" +" + _roguelikeUpgradeService.GetBonus(partyData, upgrade.IdKey, nextTier) + "]");
                }

                stateData.Actions.Add(new CrawlerStateAction(sb.ToString(), CharCodes.None, ECrawlerStates.UpgradeParty,
                    () =>
                    {
                        if (partyData.Members.Count > 0)
                        {

                        }
                        _roguelikeUpgradeService.PayForUpgrade(partyData, upgrade.IdKey);

                    }, errorText, null,  () => ShowUpgradeTooltop(upgrade.IdKey)));
            }


            stateData.Actions.Add(new CrawlerStateAction("Escape", CharCodes.Escape, ECrawlerStates.GuildMain));
            return stateData;
        }


        private void ShowUpgradeTooltop(long roguelikeUpgradeId)
        {
            List<string> allLines = new List<string>();

            RoguelikeUpgrade upgrade = _gameData.Get<RoguelikeUpgradeSettings>(_gs.ch).Get(roguelikeUpgradeId);

            if (upgrade == null)
            {
                return;
            }

            allLines.Add("Upgrade: " + upgrade.Name + "\n\n");

            allLines.Add(upgrade.Desc + "\n\n");

            allLines.Add("Base Upgrade Cost: " + upgrade.BasePointCost + "\n\n");

            allLines.Add("Total upgrade cost is NewTier*BaseUpgradeCost\n\n");

            allLines.Add("Bonus Per Tier: " + upgrade.BonusPerTier +"\n\n");

            allLines.Add("Max Tier: " + upgrade.MaxTier + "\n\n");  

            _dispatcher.Dispatch(new ShowCrawlerTooltipEvent() { Lines = allLines });
        }
    }
}
