using Genrpg.ServerShared.Config.Constants;
using Genrpg.Shared.DataStores.DataGroups;
using Genrpg.Shared.Entities.Utils;
using Genrpg.Shared.Inventory.Settings.ItemSets;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.ServerShared.Config
{
    public class ConfigSetup
    {
        public async Task<IServerConfig> SetupServerConfig(CancellationToken token, string serverId)
        {
            ServerConfig serverConfig = new ServerConfig();
            serverConfig.ServerId = serverId;
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            string filePath = config.FilePath;

            serverConfig.Env = ConfigurationManager.AppSettings[ServerConfigKeys.MainEnv];

            foreach (string dataCategory in Enum.GetNames(typeof(EDataCategories)))
            {
                serverConfig.DataEnvs[dataCategory] = GetEnvOrDefault(dataCategory + ServerConfigKeys.EnvSuffix, serverConfig.Env);
            }

            serverConfig.MessagingEnv = GetEnvOrDefault(ServerConfigKeys.MessagingEnv, serverConfig.Env);
            
            serverConfig.ContentRoot = ConfigurationManager.AppSettings[ServerConfigKeys.ContentRoot];

            serverConfig.PublicIP = ConfigurationManager.AppSettings[ServerConfigKeys.PublicIP];

            SetSecret(serverConfig, ServerConfigKeys.EtherscanKey);
            SetSecret(serverConfig, ServerConfigKeys.IOSSecret);
            SetSecret(serverConfig, ServerConfigKeys.GooglePlaySecret);

            serverConfig.PackageName = ConfigurationManager.AppSettings[ServerConfigKeys.PackageName];

            serverConfig.IOSBuyValidationURL = ConfigurationManager.AppSettings[ServerConfigKeys.IOSBuyValidationURL];

            serverConfig.IOSSandboxValidationURL = ConfigurationManager.AppSettings[ServerConfigKeys.IOSSandboxValidationURL];

            serverConfig.GooglePlayValidationURL = ConfigurationManager.AppSettings[ServerConfigKeys.GooglePlayValidationURL];

            List<string> allKeys = ConfigurationManager.AppSettings.AllKeys.ToList();

            foreach (string key in allKeys)
            {
                if (key.IndexOf(ServerConfigKeys.ConnectionSuffix) > 0)
                {
                    string shortKey = key.Replace(ServerConfigKeys.ConnectionSuffix, "");
                    serverConfig.SetSecret(shortKey, ConfigurationManager.AppSettings[key]);
                }
            }

            await Task.CompletedTask;
            return serverConfig;
        }

        private void SetSecret(ServerConfig config, string key)
        {
            config.SetSecret(key, ConfigurationManager.AppSettings[key]);
        }

        private string GetEnvOrDefault(string key, string defaultValue)
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
