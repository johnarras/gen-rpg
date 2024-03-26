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
using Genrpg.Shared.Players.Interfaces;
using Genrpg.Shared.Characters.Utils;

namespace Genrpg.Shared.Characters.PlayerData
{
    // MessagePackIgnore
    public class Character : Unit, IDirtyable, ICoreCharacter
    {

        public string UserId { get; set; }
        public int Version { get; set; }

        public int AbilityPoints { get; set; }
        public string MapId { get; set; }

        public DateTime CreationDate { get; set; } = DateTime.UtcNow;

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

        public override void Delete<T>(IRepositoryService repoSystem)
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

        public override void SaveAll(IRepositoryService repoSystem, bool saveClean)
        {
            List<BasePlayerData> itemsToSave = new List<BasePlayerData>();

            if (saveClean || IsDirty())
            {
                SetDirty(false);
                CoreCharacter coreChar = new CoreCharacter();

                CharacterUtils.CopyDataFromTo(this, coreChar);

                itemsToSave.Add(coreChar);
            }

            List<IUnitData> allValues = new List<IUnitData>(_dataDict.Values.ToList());
            foreach (IUnitData value in allValues)
            {
                itemsToSave.AddRange(value.GetSaveObjects(saveClean));
            }
            repoSystem.QueueTransactionSave(itemsToSave, Id);
        }

        
        public List<PointXZ> NearbyGridsSeen { get; set; }

        
        public DateTime LastServerStatTime { get; set; } = DateTime.UtcNow;

        private GameDataOverrideList _overrideList { get; set; }
        public void SetGameDataOverrideList(GameDataOverrideList overrideList)
        {
            _overrideList = overrideList;
        }

        public GameDataOverrideList GetOverrideList()
        {
            return _overrideList;
        }

        public override string GetName(string settingName)
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
