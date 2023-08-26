using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Spells.Entities;
using Genrpg.Shared.Units.Entities;
using System.Collections.Generic;
using System.Linq;

namespace Genrpg.Shared.DataStores.Core
{
    public abstract class IdObjectList<T> : ObjectList<T> where T : IId, new()
    {
        protected abstract bool CreateIfMissingOnGet();

        public void Add(T t)
        {
            Remove(t.IdKey);
            Data.Add(t);
        }

        public T Get(long itemId)
        {
            T item = Data.FirstOrDefault(x => x.IdKey == itemId);
            if (item == null && CreateIfMissingOnGet())
            {
                item = new T();
                item.IdKey = itemId;
                Data.Add(item);
            }
            return item;
        }

        public void Remove(long itemId)
        {
            Data.RemoveAll(x => x.IdKey == itemId);
        }

        public List<T> GetAll()
        {
            return Data.ToList();
        }
    }


}
