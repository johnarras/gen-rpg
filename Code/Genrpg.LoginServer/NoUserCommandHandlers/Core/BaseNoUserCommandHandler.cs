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
    public abstract class BaseNoUserCommandHandler<C> : INoUserCommandHandler where C : INoUserCommand
    {

        protected abstract Task InnerHandleMessage(LoginContext context, C command, CancellationToken token);

        public Type GetKey()
        {
            return typeof(C);
        }

        public virtual async Task Reset()
        {
            await Task.CompletedTask;
        }

        public async Task Execute(LoginContext context, INoUserCommand command, CancellationToken token)
        {
            await InnerHandleMessage(context, (C)command, token);
        }

        protected void ShowError(LoginContext context, string msg)
        {
            context.Results.Add(new ErrorResult() { Error = msg });
        }
    }

}
