using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Utils.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.DataStores.Categories
{
    [DataCategory(Category = DataCategory.GameData)]
    public abstract class BaseGameData : IStringId
    {
        [MessagePack.IgnoreMember]
        public abstract string Id { get; set; }
        public abstract void Set(GameData gameData);
        [MessagePack.IgnoreMember] public string Name
        {
            get { return GetType().Name; }
            set { }
        }
        virtual public bool SendToClient() { return true; }

        protected IndexedDataItemLookup _lookup { get; set; }

        public BaseGameData()
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
    }
}
