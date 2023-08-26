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

        public override Dictionary<Type, IUnitDataContainer> GetAllData()
        {
            return new Dictionary<Type, IUnitDataContainer>(_dataDict);
        }

        public override void SaveAll(IRepositorySystem repoSystem, bool saveClean)
        {

            if (saveClean || IsDirty())
            {
                SetDirty(false);
                repoSystem.QueueSave(this);

            }

            List<IUnitDataContainer> allValues = new List<IUnitDataContainer>(_dataDict.Values.ToList());
            foreach (IUnitDataContainer value in allValues)
            {
                value.SaveData(repoSystem, saveClean);
            }
        }

        public List<IUnitData> GetAll()
        {
            List<IUnitData> retval = new List<IUnitData>();
            List<IUnitDataContainer> allValues = new List<IUnitDataContainer>(_dataDict.Values.ToList());
            foreach (IUnitDataContainer value in allValues)
            {
                retval.Add(value.GetData() as IUnitData);
            }
            return retval;
        }

        [JsonIgnore]
        [Key(104)] public List<PointXZ> NearbyGridsSeen { get; set; }

        [JsonIgnore]
        [Key(105)] public DateTime LastServerStatTime { get; set; } = DateTime.UtcNow;
    }
}
