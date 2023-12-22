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

namespace Genrpg.Shared.MapServer.Entities
{
    [MessagePackObject]
    public class Map : BaseWorldData, IName, IMapRoot
    {
        public override void Delete(IRepositorySystem repoSystem) { repoSystem.Delete(this); }
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


        public static string GetMapOwnerId(IMapRoot mapRoot)
        {
            return mapRoot.Id + "-" + mapRoot.MapVersion;
        }

        public Map()
        {
            Quests = new List<QuestType>();
            Zones = new List<Zone>();
            QuestItems = new List<QuestItem>();
            SpawnX = -1;
            SpawnY = -1;
            _lookup = new IndexedDataItemLookup(this);
        }

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
            if (entityTypeId == EntityTypes.Quest)
            {
                return Quests;
            }
            else if (entityTypeId == EntityTypes.QuestItem || entityTypeId == EntityTypes.GroundObject)
            {
                return QuestItems;
            }
            else if (entityTypeId == EntityTypes.Zone)
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
