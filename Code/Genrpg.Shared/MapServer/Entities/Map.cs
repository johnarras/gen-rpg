using MessagePack;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.DataStores.Categories;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.MapServer.Constants;
using Genrpg.Shared.NPCs.Entities;
using Genrpg.Shared.Quests.Entities;
using Genrpg.Shared.Utils.Data;
using Genrpg.Shared.Zones.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Genrpg.Shared.MapServer.Entities
{
    [MessagePackObject]
    public class Map : BaseWorldData, IName, IMapRoot
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public string Name { get; set; }
        [Key(2)] public string Desc { get; set; }
        [Key(3)] public string Icon { get; set; }
        [Key(4)] public string Art { get; set; }

        [Key(5)] public int MinLevel { get; set; }
        [Key(6)] public int MaxLevel { get; set; }

        [Key(7)] public int BlockCount { get; set; }
        [Key(8)] public float ZoneSize { get; set; }

        [Key(9)] public long Seed { get; set; }

        [Key(10)] public int MapVersion { get; set; }

        [Key(11)] public int SpawnX { get; set; }
        [Key(12)] public int SpawnY { get; set; }

        [Key(13)] public float EdgeMountainChance { get; set; }

        [Key(14)] public List<NPCType> NPCs { get; set; }
        [Key(15)] public List<QuestType> Quests { get; set; }
        [Key(16)] public List<QuestItem> QuestItems { get; set; }
        [Key(17)] public List<Zone> Zones { get; set; }


        public static string GetMapOwnerId(IMapRoot mapRoot)
        {
            return mapRoot.Id + "-" + mapRoot.MapVersion;
        }

        public Map()
        {
            Quests = new List<QuestType>();
            Zones = new List<Zone>();
            NPCs = new List<NPCType>();
            QuestItems = new List<QuestItem>();
            SpawnX = -1;
            SpawnY = -1;
            EdgeMountainChance = 0.98f;
            _lookup = new IndexedDataItemLookup(this);
        }

        public override void Delete(IRepositorySystem repoSystem) { repoSystem.Delete(this); }

        public int GetHwid()
        {
            return BlockCount * (SharedMapConstants.TerrainPatchSize - 1) + 1;
        }

        public int GetHhgt()
        {
            return BlockCount * (SharedMapConstants.TerrainPatchSize - 1) + 1;
        }

        protected IndexedDataItemLookup _lookup = null;
        public virtual object Get(Type type, int id) { return _lookup.Get(type, id); }
        public virtual T Get<T>(long id) where T : class, IIndexedGameItem { return _lookup.Get<T>(id); }
        public virtual T Get<T>(ulong id) where T : class, IIndexedGameItem { return _lookup.Get<T>(id); }
        public virtual void ClearIndex() { _lookup.Clear(); }

        public int GetMapSize(GameState gs)
        {
            if (BlockCount < 4)
            {
                return SharedMapConstants.DefaultHeightmapSize;
            }

            return BlockCount * (SharedMapConstants.TerrainPatchSize - 1) + 1;
        }

        public bool IsSingleZone(GameState gs)
        {
            return ZoneSize >= BlockCount;
        }

        public object GetEditorListFromEntityTypeId(int entityTypeId)
        {
            if (entityTypeId == EntityType.NPC)
            {
                return NPCs;
            }
            else if (entityTypeId == EntityType.Quest)
            {
                return Quests;
            }
            else if (entityTypeId == EntityType.QuestItem || entityTypeId == EntityType.GroundObject)
            {
                return QuestItems;
            }
            else if (entityTypeId == EntityType.Zone)
            {
                return Zones;
            }

            return null;
        }

        public object GetEditorListFromName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return null;
            }

            if (name.IndexOf("ZoneId") >= 0)
            {
                return Zones;
            }

            if (name.IndexOf("NPCTypeId") >= 0)
            {
                return NPCs;
            }

            if (name.IndexOf("QuestTypeId") >= 0)
            {
                return Quests;
            }

            if (name.IndexOf("QuestItemId") >= 0)
            {
                return QuestItems;
            }

            return null;
        }

        private Dictionary<long, List<QuestType>> _npcQuestList = null;
        public List<QuestType> GetQuestsForNPC(GameState gs, long npcTypeId)
        {
            if (_npcQuestList == null)
            {
                _npcQuestList = new Dictionary<long, List<QuestType>>();
                if (NPCs != null && Quests != null)
                {
                    foreach (NPCType npc in NPCs)
                    {
                        _npcQuestList[npc.IdKey] = Quests.Where(X => X.StartNPCTypeId == npc.IdKey || X.EndNPCTypeId == npc.IdKey).ToList();
                    }
                }
            }

            if (!_npcQuestList.ContainsKey(npcTypeId))
            {
                return new List<QuestType>();
            }

            return _npcQuestList[npcTypeId];

        }

        public void CleanForClient()
        {
            foreach (Zone zone in Zones)
            {
                zone.CleanForClient();
            }
            Quests = new List<QuestType>();
        }
    }
}
