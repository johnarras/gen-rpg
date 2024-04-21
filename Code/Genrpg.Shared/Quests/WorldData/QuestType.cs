using MessagePack;
using Genrpg.Shared.Core.Entities;

using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Inventory.Entities;
using Genrpg.Shared.Units.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.Utils;
using Genrpg.Shared.DataStores.Categories.WorldData;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Inventory.Constants;
using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Quests.PlayerData;
using Genrpg.Shared.Zones.WorldData;
using Genrpg.Shared.Vendors.WorldData;
using Genrpg.Shared.GameSettings;

namespace Genrpg.Shared.Quests.WorldData
{
    [MessagePackObject]
    public class QuestType : BaseWorldData, IIndexedGameItem, IMapOwnerId
    {
        public override void Delete(IRepositoryService repoSystem) { repoSystem.Delete(this); }
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public string OwnerId { get; set; }
        [Key(2)] public string MapId { get; set; }
        [Key(3)] public long IdKey { get; set; }
        [Key(4)] public string Name { get; set; }
        [Key(5)] public string Desc { get; set; }
        [Key(6)] public string Icon { get; set; }
        [Key(7)] public string Art { get; set; }

        [Key(8)] public List<QuestTask> Tasks { get; set; }

        [Key(9)] public int MapVersion { get; set; }
        [Key(10)] public long ZoneId { get; set; }
        [Key(11)] public long MinLevel { get; set; }
        [Key(12)] public string StartObjId { get; set; }
        [Key(13)] public string EndObjId { get; set; }

        [Key(14)] public int CurrencyScale { get; set; }

        [Key(15)] public int ItemQuantity { get; set; }
        [Key(16)] public long ItemQualityTypeId { get; set; }

        public QuestType()
        {
            Tasks = new List<QuestTask>();
            CurrencyScale = 1;
            ItemQuantity = 1;
            ItemQualityTypeId = QualityTypes.Uncommon;

        }

        public bool IsSameQuest(QuestType other)
        {
            if (other == null)
            {
                return false;
            }

            return IdKey == other.IdKey && MapId == other.MapId && ZoneId == other.ZoneId && MapVersion == other.MapVersion;
        }

        public string PrintTaskText(GameState gs, Character ch, IGameData gameData, int index)
        {
            if (gs.map == null || Tasks == null)
            {
                return "";
            }

            QuestTask task = Tasks.FirstOrDefault(x => x.Index == index);

            if (task == null)
            {
                return "";
            }

            QuestData questList = ch.Get<QuestData>();

            QuestStatus status = questList.GetStatus(this);


            QuestTaskStatus tstatus = null;
            if (status != null)
            {
                tstatus = status.Tasks.FirstOrDefault(x => x.Index == index);

            }

            string finalTxt = "";

            if (task.TaskEntityTypeId == EntityTypes.Unit)
            {
                Zone zone = gs.map.Get<Zone>(ZoneId);

                string namePrefix = "";
                UnitType utype = gameData.Get<UnitSettings>(ch).Get(task.TaskEntityId);
                if (utype == null)
                {
                    return "";
                }

                if (zone != null && zone.Units != null)
                {
                    ZoneUnitStatus unit = zone.GetUnit(task.TaskEntityId);
                    if (unit != null && !string.IsNullOrEmpty(unit.Prefix))
                    {
                        namePrefix = unit.Prefix;
                    }
                }
                long currQuantity = 0;
                if (status != null) // have status, show progress
                {
                    QuestTaskStatus indexStatus = status.Tasks.FirstOrDefault(x => x.Index == index);
                    if (indexStatus != null)
                    {
                        currQuantity = indexStatus.CurrQuantity;
                    }
                    finalTxt = currQuantity + "/" + task.Quantity + " Kill " + namePrefix + " " + utype.Name + ".";
                }
                else
                {
                    finalTxt = "Kill " + task.Quantity + " " + namePrefix + " " + utype.Name;
                }
            }
            else if (task.TaskEntityTypeId == EntityTypes.Item)
            {
                QuestItem qitem = gs.map.Get<QuestItem>(task.TaskEntityId);
                if (qitem == null)
                {
                    return "";
                }
                UnitType utype = gameData.Get<UnitSettings>(ch).Get(task.OnEntityId);
                if (task.OnEntityTypeId == EntityTypes.Unit)
                {
                    if (utype == null)
                    {
                        return "";
                    }

                    if (status == null)
                    {
                        finalTxt = "Kill " + task.Quantity + " " + utype.Name;
                    }
                    else
                    {
                        return tstatus.CurrQuantity + "/" + task.Quantity + " Loot All";
                    }
                }
            }

            if (!string.IsNullOrEmpty(finalTxt) && tstatus != null && tstatus.CurrQuantity >= task.Quantity)
            {
                finalTxt = "(COMPLETE) " + finalTxt;
            }

            return finalTxt;
        }

        public QuestStatus CreateStatus(QuestData questData)
        {
            QuestStatus questStatus = new QuestStatus()
            {
                Quest = this,
                Id = HashUtils.NewGuid(),
                OwnerId = questData.Id,
            };
            if (Tasks != null)
            {
                for (int i = 0; i < Tasks.Count; i++)
                {
                    QuestTaskStatus taskStatus = new QuestTaskStatus()
                    {
                        CurrQuantity = 0,
                        Index = i,
                    };
                    questStatus.Tasks.Add(taskStatus);
                }
            }
            return questStatus;
        }
    }
}
