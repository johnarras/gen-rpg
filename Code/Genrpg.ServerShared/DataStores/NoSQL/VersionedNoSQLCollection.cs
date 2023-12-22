using Genrpg.Shared.DataStores.Interfaces;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Logs.Interfaces;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.ServerShared.DataStores.NoSQL
{
    public class VersionedNoSQLCollection<T> : NoSQLCollection<T> where T : class, IStringId, IUpdateData
    {
        public VersionedNoSQLCollection(NoSQLRepository mongoRepository, ILogSystem logger) : base(mongoRepository, logger)
        {
        }

        protected override async Task<ReplaceOneResult> ReplaceDocument(T t, ReplaceOptions options, IClientSessionHandle session)
        {
            if (t.CreateTime == DateTime.MinValue)
            {
                t.CreateTime = DateTime.UtcNow;
            }
            t.UpdateTime = DateTime.UtcNow;
            if (session != null)
            {
                return await _collection.ReplaceOneAsync(session, x => x.Id == t.Id && !(x.UpdateTime >= t.UpdateTime), t, options);
            }
            else
            {
                return await _collection.ReplaceOneAsync(x => x.Id == t.Id && !(x.UpdateTime >= t.UpdateTime), t, options);
            }
        }
    }
}
