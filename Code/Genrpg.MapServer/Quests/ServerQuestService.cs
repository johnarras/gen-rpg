﻿using Genrpg.Shared.Characters.Entities;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Entities.Services;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.Quests.Entities;
using Genrpg.Shared.Quests.Services;
using Genrpg.Shared.Spawns.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.MapServer.Quests
{

    public interface IServerQuestService : IService
    {
        UpdateQuestResult UpdateQuest(GameState gs, MapObject mobj, ISpawnResult spawnResult);
    }

    public class ServerQuestService : IServerQuestService
    {
        private ISharedQuestService _questService;
        private IEntityService _entityService;

        public virtual UpdateQuestResult UpdateQuest(GameState gs, MapObject mobj, ISpawnResult spawnResult)
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

            foreach (QuestStatus questStatus in questList.Data)
            {
                if (questStatus.Quest == null)
                {
                    continue;
                }

                if (questStatus.Quest.Tasks == null)
                {
                    continue;
                }

                if (questStatus.Statuses == null)
                {
                    questStatus.Statuses = new List<QuestTaskStatus>();
                }

                foreach (QuestTask task in questStatus.Quest.Tasks)
                {
                    if (task.TaskEntityTypeId != spawnResult.EntityTypeId || task.TaskEntityId != spawnResult.EntityId)
                    {
                        continue;
                    }


                    QuestTaskStatus taskStatus = questStatus.Statuses.FirstOrDefault(X => X.Index == task.Index);
                    if (taskStatus == null)
                    {
                        taskStatus = new QuestTaskStatus() { Index = task.Index };
                        questStatus.Statuses.Add(taskStatus);
                    }

                    if (taskStatus.CurrQuantity >= task.Quantity)
                    {
                        continue;
                    }

                    taskStatus.CurrQuantity += spawnResult.Quantity;
                    questList.SetDirty(true);


                    if (taskStatus.CurrQuantity > task.Quantity)
                    {
                        taskStatus.CurrQuantity = task.Quantity;
                    }

                    retval.Message += questStatus.Quest.PrintTaskText(gs, ch, task.Index) + "\n";
                    retval.Success = true;
                }
            }
            return retval;
        }


        public AlterQuestStateResult AlterQuestState(GameState gs, Character ch, AlterQuestStateData alterData)
        {
            AlterQuestStateResult errorResult = new AlterQuestStateResult();

            if (alterData == null)
            {
                errorResult.Message = "Missing Alter Data";
                return errorResult;
            }

            return errorResult;

            QuestType quest = gs.map.Get<QuestType>(alterData.QuestTypeId);

            if (quest == null || quest.Tasks == null)
            {
                errorResult.Message = "Missing Quest info";
                return errorResult;
            }

            QuestData questList = ch.Get<QuestData>();

            QuestStatus questStatus = questList.GetStatus(quest);

            int questState = _questService.GetQuestState(gs, ch, quest);

            if (alterData.AlterTypeId == AlterQuestType.Accept)
            {
                if (questStatus != null)
                {
                    errorResult.Message = "Already on this quest";
                    return errorResult;
                }

                if (questState == QuestState.Complete)
                {
                    errorResult.Message = "Already completed this quest";
                    return errorResult;
                }
                else if (questState == QuestState.NotAvailable || questState == QuestState.NotAvailable)
                {
                    errorResult.Message = "Quest is not available";
                    return errorResult;
                }
                else if (questState == QuestState.Active)
                {
                    errorResult.Message = "Already on this quest";
                    return errorResult;
                }
                else if (questState == QuestState.Available)
                {
                    if (questStatus != null)
                    {
                        errorResult.Message = "Already on this quest";
                        return errorResult;
                    }

                    questStatus = quest.CreateStatus();
                    questList.AddStatus(questStatus);
                    AlterQuestStateResult alterResult = new AlterQuestStateResult()
                    {
                        AlterTypeId = AlterQuestType.Accept,
                        Status = questStatus,
                        Success = true,
                    };
                    return alterResult;
                }
            }
            else if (alterData.AlterTypeId == AlterQuestType.Abandon)
            {
                if (questStatus == null)
                {
                    errorResult.Message = "You aren't on this quest.";
                    return errorResult;
                }
                questList.RemoveStatus(questStatus);
                AlterQuestStateResult alterResult = new AlterQuestStateResult()
                {
                    AlterTypeId = AlterQuestType.Abandon,
                    Status = questStatus,
                    Success = true,
                };
                return alterResult;
            }
            else if (alterData.AlterTypeId == AlterQuestType.Complete)
            {
                if (questStatus == null)
                {
                    errorResult.Message = "You're not on this quest.";
                    return errorResult;
                }
                if (questState != QuestState.Complete)
                {
                    errorResult.Message = "The Quest is not Complete.";
                    return errorResult;
                }

                List<SpawnResult> rewards = _questService.GetRewards(gs, quest, true);

                _entityService.GiveRewards(gs, ch, rewards);
                questList.RemoveStatus(questStatus);

                MapQuestsData mapQuestData = ch.Get<MapQuestsData>();

                mapQuestData.AddCompletedQuest(quest);

                AlterQuestStateResult alterResult = new AlterQuestStateResult()
                {
                    AlterTypeId = AlterQuestType.Complete,
                    Status = questStatus,
                    Rewards = rewards,
                    Success = true,
                };
                return alterResult;
            }
            errorResult.Message = "Unknown quest command";
            return errorResult;
        }


    }
}