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


            string contentRoot = "https://oxdbassets.azureedge.net";

            ConfigResponse response = new ConfigResponse();
            if (env == "dev")
            {
                response.ServerURL = "http://localhost:5000";
                response.ContentRoot = contentRoot;
                response.AssetEnv = "dev";
            }
            else if (env == "test")
            {
                response.ServerURL = "https://genrpgtest.azurewebsites.net";
                response.ContentRoot = contentRoot;
                response.AssetEnv = "dev";
            }
            else if (env == "prod")
            {
                response.ServerURL = "https://genrpg.azurewebsites.net";
                response.ContentRoot = contentRoot;
                response.AssetEnv = "prod";
            }
            else if (env == "local")
            {
                response.ServerURL = "http://localhost:5000";
                response.ContentRoot = contentRoot;
                response.AssetEnv = "dev";
            }

            return new OkObjectResult(response);
        }
    }
}
