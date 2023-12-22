
using System.Collections.Generic;

namespace Genrpg.ServerShared.Config
{
    public class ServerConfig
    {
        public string Env { get; set; }

        public Dictionary<string, string> DataEnvs { get; set; } = new Dictionary<string, string>();

        public string MessagingEnv { get; set; }

        public string ServerId { get; set; }

        public string ContentRoot { get; set; }

        public string EtherscanKey { get; set; }

        public Dictionary<string, string> ConnectionStrings { get; set; }  = new Dictionary<string, string>();

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
