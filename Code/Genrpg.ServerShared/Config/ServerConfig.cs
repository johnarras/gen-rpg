
using Genrpg.Shared.Configs.Interfaces;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Logging.Interfaces;
using System.Collections.Generic;

namespace Genrpg.ServerShared.Config
{

    public interface IServerConfig : IService, IConnectionConfig
    {

        string Env { get; set; }

        Dictionary<string, string> DataEnvs { get; set; }

        string MessagingEnv { get; set; }

        string ServerId { get; set; }

        string ContentRoot { get; set; }

        string EtherscanKey { get; set; }

        string PublicIP { get; set; }       

    }

    public class ServerConfig : IServerConfig
    {
        public string Env { get; set; }

        public Dictionary<string, string> DataEnvs { get; set; } = new Dictionary<string, string>();

        public string MessagingEnv { get; set; }

        public string ServerId { get; set; }

        public string ContentRoot { get; set; }

        public string EtherscanKey { get; set; }

        public string PublicIP { get; set; }

        public Dictionary<string, string> ConnectionStrings { get; set; }  = new Dictionary<string, string>();

        public Dictionary<string,string> GetConnectionStrings()
        {
            return ConnectionStrings;
        }

        public string GetConnectionString(string key)
        {
            if (ConnectionStrings.TryGetValue(key, out var value))
            {
                return value;
            }
            return null;
        }

    }
}
