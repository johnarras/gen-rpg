using System;
using System.IO;
using System.Threading.Tasks;
using System.Configuration;
using Azure.Storage.Blobs;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Reflection.Metadata;

namespace FileUploader
{
    class Program
    {
        public static void Main(string[] args)
        {
            string output = "";

            //args = new string[3];
            //args[0] = "False";
            //args[1] = "C:/Data/genrpg/Code/GenrpgClient/Assets/../AssetBundles/win/tilesadventuretile";
            //args[2] = "genrpgdev/0.0.5/win/tilesadventuretile_592db9d979b2b5805fc87e1cf6843a2b";


            try
            {
                string connectionStringVar = (args[0] == "True" ?
                    "BlobWorldsConnection" : "BlobAssetsConnection");

                var _connectionString = ConfigurationManager.AppSettings[connectionStringVar];
                if (args.Length < 3)
                {
                    output += "Not enough args\n";
                    return;
                }


                output += "Arg0: " + args[1] + "\n";
                output += "Arg1: " + args[2] + "\n";

                var fdata = new FileUploadData();
                fdata.SetLocalPath(args[1]);
                fdata.SetRemotePath(args[2]);

                BlobServiceClient blobServiceClient = new BlobServiceClient(_connectionString);

                BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(fdata.GetContainerName());

                BlobClient blobClient = containerClient.GetBlobClient(fdata.GetBlobPath());

                using var stream = new MemoryStream (File.ReadAllBytes(fdata.GetLocalPath()));

                blobClient.Upload(stream, overwrite: true);


            }
            catch (Exception e)
            {
                output += "Exception: " + e.Message + " " + e.StackTrace;
            }

            System.IO.File.WriteAllText("output.txt", output);
        }
    }
}
