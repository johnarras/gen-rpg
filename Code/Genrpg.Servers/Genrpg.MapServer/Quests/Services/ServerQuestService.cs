using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.Entities.Services;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.MapServer.Services;
using Genrpg.Shared.Quests.Constants;
using Genrpg.Shared.Quests.Entities;
using Genrpg.Shared.Quests.PlayerData;
using Genrpg.Shared.Quests.Services;
using Genrpg.Shared.Quests.WorldData;
using Genrpg.Shared.Rewards.Services;
using Genrpg.Shared.Spawns.Entities;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.MapServer.Quests.Services
{

    public interface IServerQuestService : IInjectable
    {
        UpdateQuestResult UpdateQuest(IRandom rand, MapObject mobj, ISpawnResult spawnResult);
    }

    public class ServerQuestService : IServerQuestService
    {
        //private ISharedQuestService _questService = null;
        //private IRewardService _rewardService = null;

        protected IRepositoryService _repoService = null;
        private IMapProvider _mapProvider = null;
        private IGameData _gameData = null;

        public virtual UpdateQuestResult UpdateQuest(IRandom rand, MapObject mobj, ISpawnResult spawnResult)
        {
            UpdateQuestResult retval = new UpdateQuestResult();
            if (!(mobj is Character ch))
            {
                return retval;
            }
            if (ch == null || spawnResult == null || spawnResult.Quantity < 1)
            {
                return retval;
            }

            QuestData questList = ch.Get<QuestData>();

            foreach (QuestStatus questStatus in questList.GetData())
            {
                if (questStatus.Quest == null)
                {
                    continue;
                }

                if (questStatus.Quest.Tasks == null)
                {
                    continue;
                }

                if (questStatus.Tasks == null)
                {
                    questStatus.Tasks = new List<QuestTaskStatus>();
                }

                bool changedSomething = false;
                foreach (QuestTask task in questStatus.Quest.Tasks)
                {
                    if (task.TaskEntityTypeId != spawnResult.EntityTypeId || task.TaskEntityId != spawnResult.EntityId)
                    {
                        continue;
                    }

                    QuestTaskStatus taskStatus = questStatus.Tasks.FirstOrDefault(X => X.Index == task.Index);
                    if (taskStatus == null)
                    {
                        taskStatus = new QuestTaskStatus() { Index = task.Index };
                        questStatus.Tasks.Add(taskStatus);
                    }

                    if (taskStatus.CurrQuantity >= task.Quantity)
                    {
                        continue;
                    }

                    taskStatus.CurrQuantity += spawnResult.Quantity;
                    changedSomething = true;


                    if (taskStatus.CurrQuantity > task.Quantity)
                    {
                        taskStatus.CurrQuantity = task.Quantity;
                    }

                    retval.Message += questStatus.Quest.PrintTaskText(rand, ch, _gameData, _mapProvider, task.Index) + "\n";
                    retval.Success = true;
                }
                if (changedSomething)
                {
                    _repoService.QueueSave(questStatus);
                }
            }
            return retval;
        }


        public AlterQuestStateResult AlterQuestState(IRandom rand, Character ch, AlterQuestStateData alterData)
        {
            AlterQuestStateResult errorResult = new AlterQuestStateResult();

            if (alterData == null)
            {
                errorResult.Message = "Missing Alter Data";
                return errorResult;
            }

            return errorResult;

            //QuestType quest = _mapProvider.GetMap().Get<QuestType>(alterData.QuestTypeId);

            //if (quest == null || quest.Tasks == null)
            //{
            //    errorResult.Message = "Missing Quest info";
            //    return errorResult;
            //}

            //QuestData questData = ch.Get<QuestData>();

            //QuestStatus questStatus = questData.GetStatus(quest);

            //int questState = _questService.GetQuestState(rand, ch, quest);

            //if (alterData.AlterTypeId == AlterQuestType.Accept)
            //{
            //    if (questStatus != null)
            //    {
            //        errorResult.Message = "Already on this quest";
            //        return errorResult;
            //    }

            //    if (questState == QuestState.Complete)
            //    {
            //        errorResult.Message = "Already completed this quest";
            //        return errorResult;
            //    }
            //    else if (questState == QuestState.NotAvailable || questState == QuestState.NotAvailable)
            //    {
            //        errorResult.Message = "Quest is not available";
            //        return errorResult;
            //    }
            //    else if (questState == QuestState.Active)
            //    {
            //        errorResult.Message = "Already on this quest";
            //        return errorResult;
            //    }
            //    else if (questState == QuestState.Available)
            //    {
            //        if (questStatus != null)
            //        {
            //            errorResult.Message = "Already on this quest";
            //            return errorResult;
            //        }

            //        questStatus = quest.CreateStatus(questData);
            //        questData.AddStatus(questStatus);
            //        _repoService.Delete(questStatus);
            //        AlterQuestStateResult alterResult = new AlterQuestStateResult()
            //        {
            //            AlterTypeId = AlterQuestType.Accept,
            //            Status = questStatus,
            //            Success = true,
            //        };
            //        return alterResult;
            //    }
            //}
            //else if (alterData.AlterTypeId == AlterQuestType.Abandon)
            //{
            //    if (questStatus == null)
            //    {
            //        errorResult.Message = "You aren't on this quest.";
            //        return errorResult;
            //    }
            //    questData.RemoveStatus(questStatus);
            //    _repoService.Delete(questStatus);
            //    AlterQuestStateResult alterResult = new AlterQuestStateResult()
            //    {
            //        AlterTypeId = AlterQuestType.Abandon,
            //        Status = questStatus,
            //        Success = true,
            //    };
            //    return alterResult;
            //}
            //else if (alterData.AlterTypeId == AlterQuestType.Complete)
            //{
            //    if (questStatus == null)
            //    {
            //        errorResult.Message = "You're not on this quest.";
            //        return errorResult;
            //    }
            //    if (questState != QuestState.Complete)
            //    {
            //        errorResult.Message = "The Quest is not Complete.";
            //        return errorResult;
            //    }

            //    List<SpawnResult> rewards = _questService.GetRewards(rand, ch, quest, true);

            //    _rewardService.GiveRewards(rand, ch, rewards);
            //    questData.RemoveStatus(questStatus);
            //    _repoService.Delete(questStatus);

            //    AlterQuestStateResult alterResult = new AlterQuestStateResult()
            //    {
            //        AlterTypeId = AlterQuestType.Complete,
            //        Status = questStatus,
            //        Rewards = rewards,
            //        Success = true,
            //    };
            //    return alterResult;
            //}
            //errorResult.Message = "Unknown quest command";
            //return errorResult;
        }


    }
}
