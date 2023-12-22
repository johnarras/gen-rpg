using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.DataStores.Interfaces;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.GameSettings.Interfaces;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Utils.Data;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.Shared.DataStores.Categories.GameSettings
{
    [DataCategory(Category = DataCategoryTypes.GameData)]
    public abstract class BaseGameSettings : IGameSettings, IUpdateData
    {
        [MessagePack.IgnoreMember]
        public abstract string Id { get; set; }
        [MessagePack.IgnoreMember]
        public virtual string Name
        {
            get { return GetType().Name; }
            set { }
        }
        [MessagePack.IgnoreMember]
        public DateTime UpdateTime { get; set; } = DateTime.MinValue;
        [MessagePack.IgnoreMember]
        public DateTime CreateTime { get; set; } = DateTime.MinValue;

        public virtual void AddTo(GameData gameData)
        {
            gameData.Set(this);
        }

        public virtual void SetInternalIds() { }
        
        protected IndexedDataItemLookup _lookup { get; set; }

        public BaseGameSettings()
        {
            _lookup = new IndexedDataItemLookup(this);
        }

        public void ClearIndex()
        {
            _lookup.Clear();
        }

        public List<IIdName> GetList(string typeName)
        {
            return _lookup.GetList(typeName);
        }

        public List<T> GetList<T>()
        {
            return _lookup.GetList<T>();
        }

        public virtual async Task SaveAll(IRepositorySystem repo)
        {
            await repo.Save(this);
        }
        public virtual List<IGameSettings> GetChildren() { return new List<IGameSettings>(); }

    }
}
