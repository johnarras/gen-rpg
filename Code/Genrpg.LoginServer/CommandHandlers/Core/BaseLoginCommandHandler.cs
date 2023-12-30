using Genrpg.LoginServer.Core;
using Genrpg.ServerShared.Core;
using Genrpg.ServerShared.PlayerData;
using Genrpg.ServerShared.Purchasing.Services;
using Genrpg.Shared.Login.Interfaces;
using Genrpg.Shared.Login.Messages.Error;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.LoginServer.CommandHandlers.Core
{
    public abstract class BaseLoginCommandHandler<C> : ILoginCommandHandler where C : ILoginCommand
    {

        protected IPlayerDataService _playerDataService = null;

        protected abstract Task InnerHandleMessage(LoginGameState gs, C command, CancellationToken token);

        public Type GetKey()
        {
            return typeof(C);
        }

        public virtual async Task Reset()
        {
            await Task.CompletedTask;
        }

        public async Task Execute(LoginGameState gs, ILoginCommand command, CancellationToken token)
        {
            await InnerHandleMessage(gs, (C)command, token);
        }

        protected void ShowError(LoginGameState gs, string msg)
        {
            gs.Results.Add(new ErrorResult() { Error = msg });
        }
    }

}
