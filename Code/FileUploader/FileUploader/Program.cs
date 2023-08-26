using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Configuration;

namespace FileUploader
{
    class Program
    {
        public static void Main(string[] args)
        {
            string output = "";
            try
            {
                var _connectionString = ConfigurationManager.AppSettings["BlobContentConnection"];
                if (args.Length < 2)
                {
                    output += "Not enough args\n";
                    return;
                }

                output += "Arg0: " + args[0] + "\n";
                output += "Arg1: " + args[1] + "\n";


                var fdata = new FileUploadData();
                fdata.SetLocalPath(args[0]);
                fdata.SetRemotePath(args[1]);


                var account = CloudStorageAccount.Parse(_connectionString);

                var client = account.CreateCloudBlobClient();

                string containerName = fdata.GetContainerName();
                var cont = client.GetContainerReference(containerName);
                BlobContainerPublicAccessType accessType = BlobContainerPublicAccessType.Container;

                var opts = new BlobRequestOptions();

                var context = new OperationContext();
                cont.CreateIfNotExistsAsync(accessType, opts, context).GetAwaiter().GetResult();

                var remotePathRemainder = fdata.GetBlobPath();


                CloudBlockBlob blockBlob = cont.GetBlockBlobReference(remotePathRemainder);

                if (blockBlob != null)
                {
                    Task.Run(() => blockBlob.UploadFromFileAsync(fdata.GetLocalPath())).Wait();
                }
            }
            catch (Exception e)
            {
                output += "Esception: " + e.Message + " " + e.StackTrace;
            }

            File.WriteAllText("output.txt", output);
        }
    }
}
