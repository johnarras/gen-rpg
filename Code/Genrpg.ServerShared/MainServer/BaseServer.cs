using Genrpg.ServerShared.CloudMessaging.Messages;
using Genrpg.ServerShared.CloudMessaging.Services;
using Genrpg.ServerShared.Config;
using Genrpg.ServerShared.Core;
using Genrpg.ServerShared.Setup;
using Genrpg.Shared.Reflection.Services;
using Genrpg.Shared.Setup.Services;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.ServerShared.MainServer
{
    public abstract class BaseServer
    {
        protected ServerGameState _gs;
        protected CancellationTokenSource _tokenSource = new CancellationTokenSource();
        protected string _serverId;
        protected ICloudMessageService _cloudMessageService;
        protected IReflectionService _reflectionService;

        public virtual async Task Init(object data, CancellationToken serverToken)
        {
            _tokenSource = CancellationTokenSource.CreateLinkedTokenSource(serverToken, _tokenSource.Token);
            _serverId = GetServerId(data);
            SetupService setupService = GetSetupService(data);

            _gs = await SetupUtils.SetupFromConfig<ServerGameState>(this, _serverId, setupService,
                _tokenSource.Token);

            SetupMessageHandlers();

            await Task.CompletedTask;
        }

        protected abstract string GetServerId(object data);
        protected abstract SetupService GetSetupService(object data);
        protected abstract void SetupMessageHandlers();
    }
}
