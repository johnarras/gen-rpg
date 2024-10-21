using Genrpg.Shared.Crawler.Maps.Entities;
using Genrpg.Shared.Crawler.Combat.Entities;
using Genrpg.Shared.Crawler.Loot.Services;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.Training.Settings;
using Genrpg.Shared.Inventory.PlayerData;
using Genrpg.Shared.Utils;
using System.Linq;
using System.Threading;

using Genrpg.Shared.Crawler.Training.Services;
using System.Threading.Tasks;
using Genrpg.Shared.Crawler.States.Entities;
using Genrpg.Shared.Crawler.States.Constants;
using Genrpg.Shared.Crawler.Constants;
using Genrpg.Shared.UI.Services;
using Genrpg.Shared.UI.Constants;

namespace Genrpg.Shared.Crawler.States.StateHelpers.Exploring
{

    public class GiveLootParams
    {
        public string Header { get; set; }
        public double LootScale { get; set; } = 1.0f;
        public long BonusLevels { get; set; } = 0;
        public long MonsterExpCount { get; set; } = 0;
    }

    public class GiveLootStateHelper : BaseStateHelper
    {

        private ILootGenService _lootService;
        private ITrainingService _trainingService;

        public override ECrawlerStates GetKey() { return ECrawlerStates.GiveLoot; }

        public override async Task<CrawlerStateData> Init(CrawlerStateData currentData, CrawlerStateAction action, CancellationToken token)
        {
            CrawlerStateData stateData = CreateStateData();
            stateData.WorldSpriteName = CrawlerClientConstants.TreasureImage;

            CrawlerCombatState combatState = action.ExtraData as CrawlerCombatState;

            PartyLoot loot = null;

            PartyData party = _crawlerService.GetParty();

            if (combatState != null)
            {
                stateData.Actions.Add(new CrawlerStateAction("\nYou are victorious! You receive!\n"));

                party.Combat = combatState;
                loot = _lootService.GiveCombatLoot(party, combatState, _worldService.GetMap(party.MapId));
                party.Combat = null;
            }
            else
            {
                GiveLootParams lootParams = action.ExtraData as GiveLootParams;

                if (lootParams == null)
                {
                    lootParams = new GiveLootParams() { LootScale = 1.0f };
                }

                CrawlerTrainingSettings trainingSettings = _gameData.Get<CrawlerTrainingSettings>(null);

                long level = await _worldService.GetMapLevelAtParty(await _worldService.GetWorld(party.WorldId), party);
                long trainingCost = _trainingService.GetTrainingCost(party.GetActiveParty().First());

                long gold = MathUtils.LongRange(trainingCost / 2, trainingCost, _rand);

                long monsterExp = _gameData.Get<CrawlerTrainingSettings>(_gs.ch).GetMonsterExp(level + lootParams.BonusLevels);


                LootGenData genData = new LootGenData()
                {
                    ItemCount = (int)(lootParams.LootScale * MathUtils.IntRange(3, 6, _rand)),
                    Level = level + lootParams.BonusLevels,
                    Gold = (int)(gold * lootParams.LootScale),
                    Exp = monsterExp * lootParams.MonsterExpCount * party.GetActiveParty().Count,
                };

                loot = _lootService.GiveLoot(party, _worldService.GetMap(party.MapId), genData);

                if (!string.IsNullOrEmpty(lootParams.Header))
                {
                    stateData.Actions.Add(new CrawlerStateAction(lootParams.Header));
                }
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
                    WorldQuestItem questItem = world.QuestItems.FirstOrDefault(x => x.IdKey == questItemId);
                    if (questItem != null)
                    {
                        stateData.Actions.Add(new
                            CrawlerStateAction("************ QUEST ITEM: ************\n " +
                        $"{_textService.HighlightText(questItem.Name, TextColors.ColorWhite)}!\n"));
                    }
                }
            }

            stateData.Actions.Add(new CrawlerStateAction($"\n\nPress {_textService.HighlightText("Space")} to continue...", CharCodes.Space, ECrawlerStates.ExploreWorld));
            await _crawlerService.SaveGame();

            return stateData;
        }
    }
}
