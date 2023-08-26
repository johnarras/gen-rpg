using Genrpg.ServerShared.Config;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.DataStores.Indexes;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Logs.Entities;
using Genrpg.Shared.Utils;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.ServerShared.DataStores.Blobs
{

    /// <summary>
    /// An implementation of Blob storage
    /// </summary>
    /// <typeparam name="T">The Type of items to be stored</typeparam>
    public class BlobRepositoryCollection<T> : IRepositoryCollection<T> where T : class, IStringId
    {
        private static CloudBlobContainer _cont = null;
        private static object lockObject = new object();
        private BlobRepository _repo;
        private ILogSystem _logger;
        public BlobRepositoryCollection(BlobRepository repo, ILogSystem logger)
        {
            _logger = logger;
            _repo = repo;
            _cont = _repo.GetClient().GetContainerReference(GetContainerName());
            _cont.CreateIfNotExistsAsync().GetAwaiter().GetResult();
        }

        protected string GetContainerName()
        {
            return typeof(T).Name.ToLower() + "doc";
        }

        public async Task<T> Load(string id)
        {
            T obj = default;
            try
            {
                string blobId = id;
                CloudBlockBlob blob = _cont.GetBlockBlobReference(blobId);


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


        public async Task<bool> Save(T t)
        {
            if (t == null)
            {
                return false;
            }

            string data = SerializationUtils.Serialize(t);

            return await StringSave(t.Id, data).ConfigureAwait(false);
        }

        public async Task<bool> StringSave(string id, string data)
        {

            if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(data))
            {
                return false;
            }

            CloudBlockBlob blob = _cont.GetBlockBlobReference(id);

            bool success = false;
            int maxTimes = 2;
            for (int times = 0; times < maxTimes; times++)
            {
                if (string.IsNullOrEmpty(data))
                {
                    return false;
                }

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
                    _logger.Exception(e, "StringSave");
                }
            }


            return success;
        }

        public async Task<bool> Delete(T t)
        {
            if (t == null)
            {
                return false;
            }

            string blobId = t.Id;
            if (string.IsNullOrEmpty(blobId))
            {
                return false;
            }

            CloudBlockBlob blob = _cont.GetBlockBlobReference(blobId);

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

        public async Task<List<T>> Search(Expression<Func<T, bool>> func, int quantity = 1000, int skip = 0)
        {
            throw new NotImplementedException();
        }

        public async Task CreateIndex(List<IndexConfig> configs)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> SaveAll(List<T> items)
        {
            bool everFailed = false;
            foreach (T item in items)
            {
                if (!await Save(item))
                {
                    everFailed = true;
                }
            }
            return !everFailed;
        }
    }

}
