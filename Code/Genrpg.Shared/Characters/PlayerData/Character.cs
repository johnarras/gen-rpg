using System;
using System.Collections.Generic;
using System.Linq;
using Genrpg.Shared.Utils.Data;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.Networking.Interfaces;
using Genrpg.Shared.Factions.Constants;
using Genrpg.Shared.Inventory.Constants;
using Genrpg.Shared.DataStores.PlayerData;
using Genrpg.Shared.Trades.Entities;
using Genrpg.Shared.Characters.Utils;

namespace Genrpg.Shared.Characters.PlayerData
{
    // MessagePackIgnore
    public class Character : Unit, ICoreCharacter
    {
        public string UserId { get; set; }
        public int Version { get; set; }

        public int AbilityPoints { get; set; }
        public string MapId { get; set; }

        public List<PointXZ> NearbyGridsSeen { get; set; } = new List<PointXZ>();

        public DateTime LastServerStatTime { get; set; } = DateTime.UtcNow;

        public TradeObject Trade { get; set; }
        public ulong TradeModifyLockCount = 0;
        public object TradeLock { get; private set; } = new object();

        public Character(IRepositoryService repositoryService) : base(repositoryService)
        {
            Level = 1;
            QualityTypeId = QualityTypes.Common;
            EntityId = 1;
            FactionTypeId = FactionTypes.Player;
        }

        public override void Dispose()
        {
            base.Dispose();
            NearbyGridsSeen.Clear();
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

        public override void SaveData(bool saveAll)
        {
            if (saveAll || IsDirty())
            {
                SetDirty(false);
                CoreCharacter coreChar = new CoreCharacter();

                CharacterUtils.CopyDataFromTo(this, coreChar);

                _repoService.QueueSave(coreChar);
            }

            if (saveAll)
            {
                List<IUnitData> allValues = new List<IUnitData>(_dataDict.Values.ToList());
                foreach (IUnitData value in allValues)
                {
                    value.QueueSave(_repoService);
                }
            }
        }
    }
}
