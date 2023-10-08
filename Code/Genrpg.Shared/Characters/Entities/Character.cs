using MessagePack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Genrpg.Shared.Utils.Data;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.Inventory.Entities;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.Factions.Entities;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.DataStores.Interfaces;
using Genrpg.Shared.Networking.Interfaces;
using Genrpg.Shared.GameSettings.Entities;

namespace Genrpg.Shared.Characters.Entities
{
    // MessagePackKeyOffset 100
    [MessagePackObject]
    public class Character : Unit
    {

        [Key(100)] public string UserId { get; set; }
        [Key(101)] public int Version { get; set; }

        [Key(102)] public int AbilityPoints { get; set; }
        [Key(103)] public string MapId { get; set; }


        public Character()
        {
            Level = 1;
            QualityTypeId = QualityType.Common;
            EntityId = 1;
            FactionTypeId = FactionType.Player;
            NearbyGridsSeen = new List<PointXZ>();
        }

        public override string GetGroupId()
        {
            return Id;
        }

        public void SetConn(IConnection conn)
        {
            if (_conn != null)
            {
                _conn.ForceClose();
            }
            _conn = conn;
        }

        public override bool IsPlayer() { return true; }

        public override void Delete<T>(IRepositorySystem repoSystem)
        {
            T item = Get<T>();
            if (item != null)
            {
                if (_dataDict.ContainsKey(typeof(T)))
                {
                    _dataDict.Remove(typeof(T));
                }
                repoSystem.QueueDelete(item);
            }
        }

        public override Dictionary<Type, IUnitData> GetAllData()
        {
            return new Dictionary<Type, IUnitData>(_dataDict);
        }

        public override void SaveAll(IRepositorySystem repoSystem, bool saveClean)
        {

            if (saveClean || IsDirty())
            {
                SetDirty(false);
                repoSystem.QueueSave(this);
            }

            List<IUnitData> allValues = new List<IUnitData>(_dataDict.Values.ToList());
            foreach (IUnitData value in allValues)
            {
                value.Save(repoSystem, saveClean);
            }
        }

        [JsonIgnore]
        [Key(104)] public List<PointXZ> NearbyGridsSeen { get; set; }

        [JsonIgnore]
        [Key(105)] public DateTime LastServerStatTime { get; set; } = DateTime.UtcNow;

        private SessionOverrideList _overrideList { get; set; }
        public void SetSessionOverrideList(SessionOverrideList overrideList)
        {
            _overrideList = overrideList;
        }

        public SessionOverrideList GetSessionOverrideList()
        {
            return _overrideList;
        }

        public override string GetGameDataName(string settingName)
        {
            if (_overrideList == null)
            {
                return GameDataConstants.DefaultFilename;
            }
            SessionOverrideItem item = _overrideList.Items.FirstOrDefault(x => x.SettingId == settingName);
            return item?.DocId ?? GameDataConstants.DefaultFilename;
        }


    }
}
