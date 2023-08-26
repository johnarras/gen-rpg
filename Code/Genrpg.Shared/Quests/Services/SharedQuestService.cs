
using Genrpg.Shared.Characters.Entities;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Currencies.Entities;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Levels.Entities;
using Genrpg.Shared.Quests.Entities;
using Genrpg.Shared.Spawns.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Genrpg.Shared.Quests.Services
{
    public interface ISharedQuestService : IService
    {
        int GetQuestState(GameState gs, Character ch, QuestType qtype);
        bool IsQuestSoonVisible(GameState gs, Character ch, QuestType qtype);
        List<SpawnResult> GetRewards(GameState gs, QuestType qtype, bool createRewards = false);

    }


    public class SharedQuestService : ISharedQuestService
    {

        public int GetQuestState(GameState gs, Character ch, QuestType qtype)
        {
            if (qtype == null)
            {
                return QuestState.NotAvailable;
            }

            MapQuestsData mapQuests = ch.Get<MapQuestsData>();

            if (mapQuests.HasCompletedQuest(qtype))
            {
                return QuestState.AlreadyCompleted;
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

        public virtual bool IsQuestSoonVisible(GameState gs, Character ch, QuestType qtype)
        {

            MapQuestsData mapQuests = ch.Get<MapQuestsData>();

            if (mapQuests.HasCompletedQuest(qtype))
            {
                return false;
            }

            if (ch.Level < qtype.MinLevel - QuestConstants.QuestAlmostVisibleLevels)
            {
                return false;
            }

            return true;
        }


        public List<SpawnResult> GetRewards(GameState gs, QuestType qtype, bool createRewards = false)
        {
            List<SpawnResult> rewards = new List<SpawnResult>();

            if (gs.data == null || qtype == null)
            {
                return rewards;
            }

            LevelData level = gs.data.GetGameData<LevelSettings>().GetLevel(qtype.MinLevel);

            if (level == null)
            {
                return rewards;
            }

            rewards.Add(new SpawnResult()
            {
                EntityTypeId = EntityType.Currency,
                EntityId = CurrencyType.Exp,
                Quantity = qtype.CurrencyScale * level.QuestExp
            });
            rewards.Add(new SpawnResult()
            {
                EntityTypeId = EntityType.Currency,
                EntityId = CurrencyType.Money,
                Quantity = (long)(qtype.CurrencyScale * level.KillMoney * QuestConstants.QuestKillMoneyMultiplier),
            });

            return rewards;
        }
    }
}
