
using Genrpg.Shared.DataStores.Categories;
using Genrpg.Shared.DataStores.Constants;
using Genrpg.Shared.Entities.Utils;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.ServerShared.Config
{
    public class ConfigUtils
    {
        const string ConnectionSuffix = "Connection";
    
        public static async Task<ServerConfig> SetupServerConfig(CancellationToken token, string serverId)
        {
            ServerConfig serverConfig = new ServerConfig();
            serverConfig.ServerId = serverId;
            serverConfig.Env = ConfigurationManager.AppSettings["Env"];

            foreach (string dataCategory in DataCategoryTypes.DataCategories)
            {
                serverConfig.DataEnvs[dataCategory] = ConfigurationManager.AppSettings[dataCategory + "Env"];
            }

            serverConfig.MessagingEnv = ConfigurationManager.AppSettings["MessagingEnv"];

            serverConfig.ContentRoot = ConfigurationManager.AppSettings["ContentRoot"];

            serverConfig.EtherscanKey = ConfigurationManager.AppSettings["EtherscanKey"];

            List<string> allKeys = ConfigurationManager.AppSettings.AllKeys.ToList();

            foreach (string key in allKeys)
            {
                if (key.IndexOf(ConnectionSuffix) > 0)
                {
                    string shortKey = key.Replace(ConnectionSuffix, "");
                    serverConfig.ConnectionStrings[shortKey] = ConfigurationManager.AppSettings[key];
                }
            }

            await Task.CompletedTask;
            return serverConfig;
        }
    }
}
