using Amazon.Runtime.Internal;
using Genrpg.ServerShared.Config;
using Genrpg.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.ServerShared.Secrets.Services
{

    public interface ISecretsService : IInitializable
    {
        Task<string> GetSecret(string key);
    }



    public class SecretsService : ISecretsService
    {
        private IServerConfig _serverConfig;
        public async Task Initialize(CancellationToken token)
        {
            await Task.CompletedTask;
        }

        public async Task<string> GetSecret(string key)
        {
            await Task.CompletedTask;

            // Replace this with some kind of secure vault.
            return _serverConfig.GetSecret(key);
        }
    }
}
