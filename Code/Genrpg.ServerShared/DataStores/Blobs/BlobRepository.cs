using Genrpg.Shared.Accounts.Entities;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.DataStores.Indexes;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Logs.Interfaces;
using Genrpg.Shared.Utils;
using Microsoft.Azure.Amqp.Framing;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using static MongoDB.Driver.WriteConcern;

namespace Genrpg.ServerShared.DataStores.Blobs
{
    public class BlobRepository : IRepository
    {
        private CloudStorageAccount _account = null;
        private CloudBlobClient _client = null;
        private ILogSystem _logger = null;
        private ConcurrentDictionary<Type, string> _collectionNames = new ConcurrentDictionary<Type, string>();
        private ConcurrentDictionary<Type, CloudBlobContainer> _containers = new ConcurrentDictionary<Type, CloudBlobContainer>();
    
        public BlobRepository(ILogSystem logger, string connectionString)
        {
            _account = CloudStorageAccount.Parse(connectionString);
            _client = _account.CreateCloudBlobClient();
            _logger = logger;
        }

        #region Core
        public CloudStorageAccount GetAccount()
        {
            return _account;
        }

        public CloudBlobClient GetClient()
        {
            return _client;
        }

        private async Task<CloudBlobContainer> GetContainer(Type t)
        {
            if (_containers.TryGetValue(t, out CloudBlobContainer container))
            {
                return container;
            }
            string newCollectionName = (t.Name.ToLower() + "doc");

            container = _client.GetContainerReference(newCollectionName);
            await container.CreateIfNotExistsAsync().ConfigureAwait(false);
            _containers[t] = container;
            return container;
        }

        public async Task CreateIndex<T>(List<IndexConfig> configs) where T : class, IStringId
        {
            await Task.CompletedTask;
            throw new NotImplementedException();
        }
        #endregion

        #region Save
        public async Task<bool> Save<T>(T t) where  T : class, IStringId
        {
            string data = SerializationUtils.Serialize(t);

            CloudBlobContainer container = await GetContainer(t.GetType());

            CloudBlockBlob blob = container.GetBlockBlobReference(t.Id);

            bool success = false;
            int maxTimes = 2;
            for (int times = 0; times < maxTimes; times++)
            {
                try
                {
                    await blob.UploadTextAsync(data).ConfigureAwait(false);
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

        async Task<bool> IRepository.SaveAll<T>(List<T> tlist)
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
            CloudBlobContainer container = await GetContainer(t.GetType());

            CloudBlockBlob blob = container.GetBlockBlobReference(t.Id);

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

        #endregion

        #region Load
        public async Task<T> Load<T>(string id) where T : class, IStringId
        {
            T obj = default;
            try
            {
                CloudBlobContainer container = await GetContainer(typeof(T));

                CloudBlockBlob blob = container.GetBlockBlobReference(id);

                int maxTimes = 1;
                for (int times = 0; times < maxTimes; times++)
                {
                    try
                    {

                        string txt = await blob.DownloadTextAsync().ConfigureAwait(false);
                        if (!string.IsNullOrEmpty(txt))
                        {
                            obj = SerializationUtils.Deserialize<T>(txt);
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
        public async Task<List<T>> Search<T>(Expression<Func<T, bool>> func, int quantity, int skip) where T : class, IStringId
        {
            await Task.CompletedTask;
            throw new NotImplementedException();
        }

        public async Task<bool> TransactionSave<T>(List<T> list) where T : class, IStringId
        {
            await Task.CompletedTask;
            throw new NotImplementedException();
        }

        #endregion
    }
}
