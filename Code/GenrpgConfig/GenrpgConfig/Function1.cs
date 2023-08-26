using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace GenrpgConfig
{
    public static class Function1
    {
        [FunctionName("GenrpgConfig")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            string env = req.Query["env"];
            if (env == null)
            {
                env = "local";
            }
            env = env.ToLower();

            ConfigResponse response = new ConfigResponse();
            if (env == "dev")
            {
                response.ServerURL = "http://localhost:5000";
                response.ArtURLPrefix = "http://yourblobstorage.blob.core.windows.net/";
            }
            else if (env == "test")
            {
                response.ServerURL = "https://yourloginservertest.azurewebsites.net";
                response.ArtURLPrefix = "http://yourblobstorage.blob.core.windows.net/";
            }
            else if (env == "live")
            {
                response.ServerURL = "https://yourloginserver.azurewebsites.net";
                response.ArtURLPrefix = "http://yourblobstorage.blob.core.windows.net/";
            }
            else if (env == "local")
            {
                response.ServerURL = "http://localhost:5000";
                response.ArtURLPrefix = "http://yourblobstorage.blob.core.windows.net/";
            }

            return new OkObjectResult(response);
        }
    }
}
