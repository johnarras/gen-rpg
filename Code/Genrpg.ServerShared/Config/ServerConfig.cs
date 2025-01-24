
using Genrpg.Shared.Configs.Interfaces;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Logging.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

namespace Genrpg.ServerShared.Config
{

    public interface IServerConfig : IInjectable
    {

        string Env { get; set; }

        Dictionary<string, string> DataEnvs { get; set; }

        string MessagingEnv { get; set; }

        string ServerId { get; set; }

        string ContentRoot { get; set; }

        string PublicIP { get; set; }    
        
        string PackageName { get; set; }

        string GooglePlayValidationURL { get; set; }

        string IOSBuyValidationURL { get; set; }

        string IOSSandboxValidationURL { get; set; }

        void SetSecret(string key, string val); // Eventually use ISecretsService with some kind of vault.
        string GetSecret(string key); // Eventually use ISecretsService with some kind of vault.
    }

    public class ServerConfig : IServerConfig
    {

        public string Env { get; set; }

        public Dictionary<string, string> DataEnvs { get; set; } = new Dictionary<string, string>();

        public string MessagingEnv { get; set; }

        public string ServerId { get; set; }

        public string ContentRoot { get; set; }

        public string PublicIP { get; set; }

        public string PackageName { get; set; }

        public string GooglePlayValidationURL { get; set; }

        public string IOSBuyValidationURL { get; set; }

        public string IOSSandboxValidationURL { get; set; }


        private Dictionary<string, string> _secrets { get; set; } = new Dictionary<string, string>();

        public void SetSecret(string key, string val)
        {
            _secrets.Add(key, val);
        }

        public string GetSecret(string key)
        {
            if (_secrets.TryGetValue(key, out var value))
            {
                return value;
            }
            return null;
        }

    }
}
