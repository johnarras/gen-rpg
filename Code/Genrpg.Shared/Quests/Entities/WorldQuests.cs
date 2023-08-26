using MessagePack;
using Genrpg.Shared.DataStores.Core;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.Utils.Data;
using System.Linq;
using Genrpg.Shared.Spells.Entities;
using System.Collections.Generic;

namespace Genrpg.Shared.Quests.Entities
{

    [MessagePackObject]
    public class MapQuests : IStatusItem
    {
        [Key(0)] public string? MapId { get; set; }
        [Key(1)] public int MapVersion { get; set; }
        [Key(2)] public BitList Quests { get; set; }

        public MapQuests()
        {
            Quests = new BitList();
        }
    }

    /// <summary>
    /// Use this to mark completed quests.
    /// 
    /// Each quest has a MapId, along with the MapVersion and the Id of the quest. use bits to store
    /// this information.
    /// 
    /// </summary>
    /// 

    [MessagePackObject]
    public class MapQuestsData : ObjectList<MapQuests>
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override List<MapQuests> Data { get; set; } = new List<MapQuests>();
        public override void AddTo(Unit unit) { unit.Set(this); }
        public override void Delete(IRepositorySystem repoSystem) { repoSystem.Delete(this); }
        public MapQuests GetQuestList(string mapId, int mapVersion)
        {
            MapQuests wl = Data.FirstOrDefault(x => x.MapId == mapId && x.MapVersion == mapVersion);
            if (wl == null)
            {
                wl = new MapQuests() { MapId = mapId, MapVersion = mapVersion };
                Data.Add(wl);
            }
            return wl;
        }

        public bool HasCompletedQuest(QuestType qtype)
        {
            if (qtype == null)
            {
                return false;
            }

            MapQuests qlist = GetQuestList(qtype.MapId, qtype.MapVersion);
            return qlist.Quests.HasBit(qtype.IdKey);
        }

        public void AddCompletedQuest(QuestType qtype)
        {
            if (qtype == null)
            {
                return;
            }

            MapQuests qlist = GetQuestList(qtype.MapId, qtype.MapVersion);
            qlist.Quests.SetBit(qtype.IdKey);
        }

    }

}
