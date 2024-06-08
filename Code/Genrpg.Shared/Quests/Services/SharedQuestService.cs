using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Currencies.Constants;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Levels.Settings;
using Genrpg.Shared.Quests.Constants;
using Genrpg.Shared.Quests.PlayerData;
using Genrpg.Shared.Quests.WorldData;
using Genrpg.Shared.Spawns.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Utils;

namespace Genrpg.Shared.Quests.Services
{
    public interface ISharedQuestService : IInitializable
    {
        int GetQuestState(IRandom rand, Character ch, QuestType qtype);
        bool IsQuestSoonVisible(IRandom rand, Character ch, QuestType qtype);
        List<SpawnResult> GetRewards(IRandom rand, Character ch, QuestType qtype, bool createRewards = false);

    }


    public class SharedQuestService : ISharedQuestService
    {

        private IGameData _gameData = null;
        public async Task Initialize(IGameState gs, CancellationToken toke)
        {
            await Task.CompletedTask;
        }


        public int GetQuestState(IRandom rand, Character ch, QuestType qtype)
        {
            if (qtype == null)
            {
                return QuestState.NotAvailable;
            }

            QuestData questList = ch.Get<QuestData>();

            QuestStatus currQuest = questList.GetStatus(qtype);

            if (currQuest == null)
            {
                if (ch.Level >= qtype.MinLevel)
                {
                    return QuestState.Available;
                }
                else if (ch.Level >= qtype.MinLevel - QuestConstants.QuestAlmostVisibleLevels)
                {
                    return QuestState.AlmostAvailable;
                }
                else
                {
                    return QuestState.NotAvailable;
                }
            }
            else
            {
                if (currQuest.IsComplete())
                {
                    return QuestState.Complete;
                }

                return QuestState.Active;
            }



        }

        public virtual bool IsQuestSoonVisible(IRandom rand, Character ch, QuestType qtype)
        {

            if (ch.Level < qtype.MinLevel - QuestConstants.QuestAlmostVisibleLevels)
            {
                return false;
            }

            return true;
        }


        public List<SpawnResult> GetRewards(IRandom rand, Character ch, QuestType qtype, bool createRewards = false)
        {
            List<SpawnResult> rewards = new List<SpawnResult>();

            if (qtype == null)
            {
                return rewards;
            }

            LevelInfo level = _gameData.Get<LevelSettings>(ch).Get(qtype.MinLevel);

            if (level == null)
            {
                return rewards;
            }

            rewards.Add(new SpawnResult()
            {
                EntityTypeId = EntityTypes.Currency,
                EntityId = CurrencyTypes.Exp,
                Quantity = qtype.CurrencyScale * level.QuestExp
            });
            rewards.Add(new SpawnResult()
            {
                EntityTypeId = EntityTypes.Currency,
                EntityId = CurrencyTypes.Money,
                Quantity = (long)(qtype.CurrencyScale * level.KillMoney * QuestConstants.QuestKillMoneyMultiplier),
            });

            return rewards;
        }
    }
}
