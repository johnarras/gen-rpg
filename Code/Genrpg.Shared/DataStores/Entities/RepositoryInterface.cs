using Genrpg.Shared.DataStores.Indexes;
using Genrpg.Shared.Interfaces;
using MessagePack.Formatters;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;


namespace Genrpg.Shared.DataStores.Entities
{
    public enum RepositoryCategory
    {
        None = 0,
        File = 1,
        Sql = 2,
        World = 3,
        GameData=4,
    };

    public interface IRepositorySystem
    {
        Task<T> Load<T>(string id) where T : class, IStringId;
        
        Task<bool> Save<T>(T t) where T : class, IStringId;
        Task<bool> SaveAll<T>(List<T> list) where T : class, IStringId;
        void QueueSave<T>(T t) where T : class, IStringId;
        
        Task<bool> Delete<T>(T t) where T : class, IStringId;
        Task<bool> DeleteAll<T>(Expression<Func<T, bool>> func) where T : class, IStringId;
        void QueueDelete<T>(T t) where T : class, IStringId;

        Task<bool> UpdateDict<T>(string docId, Dictionary<string,object> fieldNameUpdates) where T : class, IStringId;
        void QueueUpdateDict<T>(string docId, Dictionary<string, object> fieldNameUpdates) where T : class, IStringId;

        Task<bool> UpdateAction<T>(string docId, Action<T> action) where T : class, IStringId;
        void QueueUpdateAction<T>(string docId, Action<T> action) where T : class, IStringId;

        Task<List<T>> Search<T>(Expression<Func<T, bool>> func, int quantity = 1000, int skip = 0) where T : class, IStringId;
        Task CreateIndex<T>(List<IndexConfig> configs) where T : class, IStringId;
        Task<bool> TransactionSave<T>(List<T> list) where T : class, IStringId;
        void QueueTransactionSave<T>(List<T> list, string queueId) where T : class, IStringId;
    }

    public interface IRepository
    {
        Task<T> Load<T>(string id) where T : class, IStringId;
        Task<bool> Save<T>(T obj) where T : class, IStringId;
        Task<bool> Delete<T>(T obj) where T : class, IStringId;
        Task<bool> DeleteAll<T>(Expression<Func<T,bool>> func) where T : class, IStringId;
        Task<bool> UpdateDict<T>(string docId, Dictionary<string, object> fieldNameUpdates) where T : class, IStringId;
        Task<bool> UpdateAction<T>(string docId, Action<T> action) where T : class, IStringId;

        Task<List<T>> Search<T>(Expression<Func<T, bool>> func, int quantity = 1000, int skip = 0) where T : class, IStringId;
        Task CreateIndex<T>(List<IndexConfig> configs) where T : class, IStringId;
        Task<bool> SaveAll<T>(List<T> tlist) where T : class, IStringId;
        Task<bool> TransactionSave<T>(List<T> list) where T : class, IStringId;
    }

}
