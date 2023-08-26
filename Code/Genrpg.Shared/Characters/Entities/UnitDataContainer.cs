using MessagePack;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.DataStores.Interfaces;
using Genrpg.Shared.Interfaces;
using System.Threading.Tasks;

namespace Genrpg.Shared.Characters.Entities
{
    [MessagePackObject]
    public class UnitDataContainer<T> : IUnitDataContainer where T : class, IUnitData
    {
        protected T _data;

        public UnitDataContainer(T data)
        {
            _data = data;  
        }

        public IUnitData GetData()
        {
            return _data;
        }

        public void SaveData(IRepositorySystem repoSystem, bool saveClean)
        {
            if (saveClean || _data.IsDirty())
            {
                repoSystem.QueueSave(_data);
                if (saveClean)
                {
                    _data.SaveAll(repoSystem);
                }
            }
            _data.SetDirty(false);
        }

        public void Delete(IRepositorySystem repoSystem)
        {
            repoSystem.QueueDelete(_data);
        }
    }
}
