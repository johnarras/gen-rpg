using MessagePack;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.MapServer.Constants;
using Genrpg.Shared.Utils.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Genrpg.Shared.DataStores.Categories.WorldData;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Quests.WorldData;
using Genrpg.Shared.Zones.WorldData;
using Genrpg.Shared.Vendors.WorldData;
using Genrpg.Shared.ProcGen.Settings.Names;

namespace Genrpg.Shared.MapServer.Entities
{
    [MessagePackObject]
    public class Map : BaseWorldData, IName, IMapRoot
    {
        public override void Delete(IRepositoryService repoSystem) { repoSystem.Delete(this); }
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

        [Key(13)] public long OverrideZoneId { get; set; }
        [Key(14)] public float OverrideZonePercent { get; set; }

        [Key(15)] public List<QuestType> Quests { get; set; }
        [Key(16)] public List<QuestItem> QuestItems { get; set; }
        [Key(17)] public List<Zone> Zones { get; set; }

        public Map()
        {
            Quests = new List<QuestType>();
            Zones = new List<Zone>();
            QuestItems = new List<QuestItem>();
            SpawnX = -1;
            SpawnY = -1;
        }

        public int GetHwid()
        {
            return BlockCount * (SharedMapConstants.TerrainPatchSize - 1) + 1;
        }

        public int GetHhgt()
        {
            return BlockCount * (SharedMapConstants.TerrainPatchSize - 1) + 1;
        }

        public virtual T Get<T>(long id) where T : class, IIdName
        {
            if (typeof(T) == typeof(Zone))
            {
                return Zones.FirstOrDefault(x => x.IdKey == id) as T;
            }
            else if (typeof(T) == typeof(QuestType))
            {
                return Quests.FirstOrDefault(x => x.IdKey == id) as T;
            }
            else if (typeof(T) == typeof(QuestItem))
            {
                return QuestItems.FirstOrDefault(x => x.IdKey == id) as T;
            }
            return default;
        }
        public virtual void ClearIndex() {}

        public int GetMapSize()
        {
            if (BlockCount < 4)
            {
                return SharedMapConstants.DefaultHeightmapSize;
            }

            return BlockCount * (SharedMapConstants.TerrainPatchSize - 1) + 1;
        }

        public bool IsSingleZone()
        {
            return ZoneSize >= BlockCount;
        }

        public List<IIdName> GetEditorListFromEntityTypeId(long entityTypeId)
        {
            if (entityTypeId == EntityTypes.Quest)
            {
                return Quests.Cast<IIdName>().ToList();
            }
            else if (entityTypeId == EntityTypes.QuestItem || entityTypeId == EntityTypes.GroundObject)
            {
                return QuestItems.Cast<IIdName>().ToList(); ;
            }
            else if (entityTypeId == EntityTypes.Zone)
            {
                return Zones.Cast<IIdName>().ToList();
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
