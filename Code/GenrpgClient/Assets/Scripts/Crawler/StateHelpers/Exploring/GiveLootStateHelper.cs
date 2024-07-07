using Assets.Scripts.Crawler.Constants;
using Assets.Scripts.Crawler.CrawlerStates;
using Assets.Scripts.Crawler.Maps.Entities;
using Assets.Scripts.Crawler.Services.Combat;
using Assets.Scripts.Crawler.UI.Utils;
using Assets.Scripts.UI.Crawler.States;
using Genrpg.Shared.Crawler.Combat.Entities;
using Genrpg.Shared.Crawler.Loot.Services;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.Training.Settings;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Inventory.PlayerData;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading;
using UnityEngine;

namespace Assets.Scripts.Crawler.StateHelpers.Combat
{
    public class GiveLootStateHelper : BaseStateHelper
    {

        private ILootGenService _lootService;

        public override ECrawlerStates GetKey() { return ECrawlerStates.GiveLoot; }

        public override async Awaitable<CrawlerStateData> Init(CrawlerStateData currentData, CrawlerStateAction action, CancellationToken token)
        {
            CrawlerStateData stateData = CreateStateData();
            stateData.WorldSpriteName = CrawlerClientConstants.TreasureImage;

            CombatState combatState = action.ExtraData as CombatState;

            PartyLoot loot = null;

            PartyData party = _crawlerService.GetParty();

            if (combatState != null)
            {
                stateData.Actions.Add(new CrawlerStateAction("\nYou are victorious! You receive!\n"));

               loot = _lootService.GiveCombatLoot(party);

            }
            else
            {
                CrawlerTrainingSettings trainingSettings = _gameData.Get<CrawlerTrainingSettings>(null);

                long level = await _worldService.GetMapLevelAtParty(await _worldService.GetWorld(party.WorldId), party);
                long trainingCost = trainingSettings.GetNextLevelTrainingCost(level);

                long gold = MathUtils.LongRange(trainingCost/2, trainingCost, _rand);

                LootGenData genData = new LootGenData()
                {
                    ItemCount = MathUtils.IntRange(3, 6, _rand),
                    Level = level,
                    Gold = gold,
                };

                loot = _lootService.GiveLoot(party, genData); 
                stateData.Actions.Add(new CrawlerStateAction("\nYou found Treasure!\n"));
            }


            if (loot.Exp > 0)
            {
                stateData.Actions.Add(new CrawlerStateAction(loot.Exp + " Exp per surviving party member!"));
            }
            if (loot.Gold > 0)
            {
                stateData.Actions.Add(new CrawlerStateAction(loot.Gold + " Gold!"));
            }

            foreach (Item item in loot.Items)
            {
                stateData.Actions.Add(new CrawlerStateAction(item.Name + "!"));
            }
            
            if (loot.NewQuestItems.Count > 0)
            {
                CrawlerWorld world = await _worldService.GetWorld(party.WorldId);

                foreach (long questItemId in loot.NewQuestItems)
                {
                    WorldQuestItem questItem = world.QuestItems.FirstOrDefault(x=>x.IdKey == questItemId);
                    if (questItem != null)
                    {
                        stateData.Actions.Add(new
                            CrawlerStateAction("************ QUEST ITEM: ************\n " +
                        $"{CrawlerUIUtils.HighlightText(questItem.Name, CrawlerUIUtils.ColorWhite)}!\n"));
                    }
                }
            }

            stateData.Actions.Add(new CrawlerStateAction($"\n\nPress {CrawlerUIUtils.HighlightText("Space")} to continue...", KeyCode.Space, ECrawlerStates.ExploreWorld));

            await _crawlerService.SaveGame();

            return stateData;
        }
    }
}
