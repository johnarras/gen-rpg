using MessagePack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using Genrpg.Shared.Utils.Data;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.Networking.Interfaces;
using Genrpg.Shared.Factions.Constants;
using Genrpg.Shared.Inventory.Constants;
using Genrpg.Shared.DataStores.Categories.PlayerData;
using Genrpg.Shared.DataStores.PlayerData;
using Genrpg.Shared.GameSettings.PlayerData;

namespace Genrpg.Shared.Characters.PlayerData
{
    // MessagePackKeyOffset 100
    [MessagePackObject]
    public class Character : Unit
    {

        [Key(100)] public string UserId { get; set; }
        [Key(101)] public int Version { get; set; }

        [Key(102)] public int AbilityPoints { get; set; }
        [Key(103)] public string MapId { get; set; }

        [Key(104)] public DateTime CreationDate { get; set; } = DateTime.UtcNow;

        public Character()
        {
            Level = 1;
            QualityTypeId = QualityTypes.Common;
            EntityId = 1;
            FactionTypeId = FactionTypes.Player;
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
            List<BasePlayerData> itemsToSave = new List<BasePlayerData>();

            if (saveClean || IsDirty())
            {
                SetDirty(false);
                itemsToSave.Add(this);
            }

            List<IUnitData> allValues = new List<IUnitData>(_dataDict.Values.ToList());
            foreach (IUnitData value in allValues)
            {
                itemsToSave.AddRange(value.GetSaveObjects(saveClean));
            }
            repoSystem.QueueTransactionSave(itemsToSave, Id);
        }

        [JsonIgnore]
        [Key(105)] public List<PointXZ> NearbyGridsSeen { get; set; }

        [JsonIgnore]
        [Key(106)] public DateTime LastServerStatTime { get; set; } = DateTime.UtcNow;

        private GameDataOverrideList _overrideList { get; set; }
        public void SetGameDataOverrideList(GameDataOverrideList overrideList)
        {
            _overrideList = overrideList;
        }

        public GameDataOverrideList GetGameDataOverrideList()
        {
            return _overrideList;
        }

        public override string GetGameDataName(string settingName)
        {
            if (_overrideList == null)
            {
                return GameDataConstants.DefaultFilename;
            }
            PlayerSettingsOverrideItem item = _overrideList.Items.FirstOrDefault(x => x.SettingId == settingName);
            return item?.DocId ?? GameDataConstants.DefaultFilename;
        }


    }
}
