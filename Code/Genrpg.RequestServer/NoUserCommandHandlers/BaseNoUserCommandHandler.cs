using Genrpg.ServerShared.Core;
using Genrpg.ServerShared.PlayerData;
using Genrpg.ServerShared.Purchasing.Services;
using Genrpg.Shared.Website.Messages.Error;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Website.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Genrpg.RequestServer.Core;

namespace Genrpg.RequestServer.NoUserCommandHandlers
{
    public abstract class BaseNoUserCommandHandler<C> : INoUserCommandHandler where C : INoUserCommand
    {

        protected abstract Task InnerHandleMessage(WebContext context, C command, CancellationToken token);

        public Type GetKey()
        {
            return typeof(C);
        }

        public virtual async Task Reset()
        {
            await Task.CompletedTask;
        }

        public async Task Execute(WebContext context, INoUserCommand command, CancellationToken token)
        {
            await InnerHandleMessage(context, (C)command, token);
        }

        protected void ShowError(WebContext context, string msg)
        {
            context.Results.Add(new ErrorResult() { Error = msg });
        }
    }

}
