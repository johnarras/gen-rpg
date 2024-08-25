using Amazon.Runtime.Internal;
using Genrpg.ServerShared.Config;
using Genrpg.Shared.Constants;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.DataStores.Indexes;
using Genrpg.Shared.DataStores.Utils;
using Genrpg.Shared.Entities.Utils;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Logging.Interfaces;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System.IO;
using System.Text;

namespace Genrpg.ServerShared.DataStores.Blobs
{
    public class BlobRepository : IServerRepository
    {
        class BlobConnection
        {
            public string ConnectionString { get; set; }
            public BlobServiceClient Client { get;set;}
            public ConcurrentDictionary<string, BlobContainerClient> Containers { get; set; }= new ConcurrentDictionary<string, BlobContainerClient>();
        }

        private static ConcurrentDictionary<string, BlobConnection> _connections { get; set; }= new ConcurrentDictionary<string, BlobConnection>();    

        private ILogService _logger = null;
        private BlobContainerClient _container = null;

        public BlobRepository(ILogService logger, IServerConfig config, string connectionString, string dataCategory, string blobEnv)
        {
            _ = Task.Run(()=>Init(logger,config,connectionString,dataCategory,blobEnv));    
        }

        public async Task Init(ILogService logger, IServerConfig config, string connectionString, string dataCategory, string blobEnv)
        {

            _logger = logger;

            if (!_connections.TryGetValue(connectionString, out BlobConnection connection))
            {
                connection = new BlobConnection();
                connection.Client = new BlobServiceClient(connectionString);
                _connections[connectionString] = connection;
            }

            string containerName = BlobUtils.GetBlobContainerName(dataCategory, Game.Prefix, blobEnv);

            if (!connection.Containers.TryGetValue(containerName, out BlobContainerClient container))
            {
                container = connection.Client.GetBlobContainerClient(containerName);
                _container = container;
                await container.CreateIfNotExistsAsync(PublicAccessType.BlobContainer, null, null);
                connection.Containers[containerName] = container;
            }

        }

        #region Core
        private BlobClient GetBlockBlobReference(Type t, string id)
        {
            return _container.GetBlobClient(t.Name.ToLower() + "/" + id);
        }

        // Breakd LSP
        public async Task CreateIndex<T>(CreateIndexData data) where T : class, IStringId
        {
            await Task.CompletedTask;
            throw new NotImplementedException();
        }
        #endregion

        #region Save
        public async Task<bool> Save<T>(T t) where  T : class, IStringId
        {
            string data = SerializationUtils.Serialize(t);

            BlobClient blob = GetBlockBlobReference(t.GetType(), t.Id);

            bool success = false;
            int maxTimes = 2;
            for (int times = 0; times < maxTimes; times++)
            {
                try
                {
                    using MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(data)); 
                    await blob.UploadAsync(stream, overwrite:true).ConfigureAwait(false);
                    success = true;
                    break;
                }
                catch (Exception e)
                {
                    if (times < maxTimes - 1)
                    {
                        await Task.Delay(100).ConfigureAwait(false);
                    }
                    _logger.Exception(e, "Save");
                }
            }
            return success;
        }

        public async Task<bool> SaveAll<T>(List<T> tlist) where T : class, IStringId
        {
            bool allOk = true;
            foreach (T t in tlist)
            {
                if (!await Save(t))
                {
                    allOk = false;
                    break;
                }
            }
            return allOk;
        }


        #endregion

        #region Delete
        public async Task<bool> Delete<T>(T t) where T : class, IStringId
        {
            BlobClient blob = GetBlockBlobReference(t.GetType(), t.Id);

            bool success = false;
            try
            {
                await blob.DeleteAsync().ConfigureAwait(false);
                success = true;
            }
            catch (Exception e)
            {
                _logger.Exception(e, "Delete1");
                try
                {
                    await Task.Delay(100).ConfigureAwait(false);
                    await blob.DeleteAsync().ConfigureAwait(false);
                    success = true;
                }
                catch (Exception ee)
                {
                    _logger.Exception(ee, "Delete2");
                }
            }
            return success;
        }


        public async Task<bool> DeleteAll<T>(Expression<Func<T, bool>> func) where T : class, IStringId
        {
            await Task.CompletedTask;
            return false;
        }


        #endregion

        #region Load
        public async Task<T> Load<T>(string id) where T : class, IStringId
        {
            T obj = default;
            try
            {
                BlobClient blob = GetBlockBlobReference(typeof(T), id);

                int maxTimes = 1;
                for (int times = 0; times < maxTimes; times++)
                {
                    try
                    {
                        using (BlobDownloadInfo info = await blob.DownloadAsync().ConfigureAwait(false))
                        {
                            using (StreamReader streamReader = new StreamReader(info.Content))
                            {
                                string txt = await streamReader.ReadToEndAsync();
                                if (!string.IsNullOrEmpty(txt))
                                {
                                    obj = SerializationUtils.Deserialize<T>(txt);
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        _logger.Exception(e, "SaveFile");
                        if (times < maxTimes - 1)
                        {
                            await Task.Delay(100).ConfigureAwait(false);
                        }
                    }

                    if (obj != null)
                    {
                        break;
                    }
                }

            }
            catch (Exception eee)
            {
                _logger.Exception(eee, "SaveFile2");
            }

            return obj;
        }
        #endregion

        #region Search
        // Breaks LSP
        public async Task<List<T>> Search<T>(Expression<Func<T, bool>> func, int quantity, int skip) where T : class, IStringId
        {
            await Task.CompletedTask;
            throw new NotImplementedException();
        }
        // Breaks LSP
        public async Task<bool> TransactionSave<T>(List<T> list) where T : class, IStringId
        {
            return await SaveAll<T>(list);
        }

        // Breaks LSP
        public async Task<T> AtomicIncrement<T>(string docId, string fieldName, long increment) where T : class, IStringId
        {
            await Task.CompletedTask;
            throw new NotImplementedException();
        }

        public virtual async Task<bool> UpdateDict<T>(string docId, Dictionary<string, object> fieldNameUpdates) where T : class, IStringId
        {
            T doc = (T)await Load<T>(docId);

            if (doc != null)
            {
                foreach (string key in fieldNameUpdates.Keys)
                {
                    EntityUtils.SetObjectValue(doc, key, fieldNameUpdates[key]);
                }
                return await Save(doc);
            }
            return false;
        }

        public async Task<bool> UpdateAction<T>(string docId, Action<T> action) where T : class, IStringId
        {
            T doc = (T)await Load<T>(docId);

            if (doc != null)
            {
                action(doc);
                return await Save(doc);
            }
            return false;
        }

        #endregion
    }
}
