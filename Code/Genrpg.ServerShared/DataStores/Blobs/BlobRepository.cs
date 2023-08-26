using Genrpg.Shared.Accounts.Entities;
using Genrpg.Shared.Logs.Entities;
using Microsoft.Azure.Amqp.Framing;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.ServerShared.DataStores.Blobs
{
    public class BlobRepository
    {
        private CloudStorageAccount _account = null;
        private CloudBlobClient _client = null;
        private ILogSystem _logger = null;
        public BlobRepository(ILogSystem logger, string connectionString)
        {
            _account = CloudStorageAccount.Parse(connectionString);
            _client = _account.CreateCloudBlobClient();
        }

        public CloudStorageAccount GetAccount()
        {
            return _account;
        }

        public CloudBlobClient GetClient()
        {
            return _client;
        }
    }
}
