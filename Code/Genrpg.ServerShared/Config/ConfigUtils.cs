using Genrpg.Shared.DataStores.DataGroups;
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
    
        public static async Task<IServerConfig> SetupServerConfig(CancellationToken token, string serverId)
        {
            ServerConfig serverConfig = new ServerConfig();
            serverConfig.ServerId = serverId;
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            string filePath = config.FilePath;

            serverConfig.Env = ConfigurationManager.AppSettings["Env"];

            foreach (string dataCategory in Enum.GetNames(typeof(EDataCategories)))
            {
                serverConfig.DataEnvs[dataCategory] = GetEnvOrDefault(dataCategory + "Env", serverConfig.Env);
            }

            serverConfig.MessagingEnv = GetEnvOrDefault("MessagingEnv", serverConfig.Env);
            
            serverConfig.ContentRoot = ConfigurationManager.AppSettings["ContentRoot"];

            serverConfig.EtherscanKey = ConfigurationManager.AppSettings["EtherscanKey"];

            serverConfig.PublicIP = ConfigurationManager.AppSettings["PublicIP"];

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

        private static string GetEnvOrDefault(string key, string defaultValue)
        {
            string envId = ConfigurationManager.AppSettings[key];

            if (string.IsNullOrEmpty(envId))
            {
                return defaultValue;
            }
            return envId;
        }
    }
}
