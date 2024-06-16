using Genrpg.LoginServer.CommandHandlers.Core;
using Genrpg.LoginServer.Core;
using Genrpg.LoginServer.PlayerData;
using Genrpg.LoginServer.Services.LoginServer;
using Genrpg.LoginServer.Services.NoUsers;
using Genrpg.LoginServer.Utils;
using Genrpg.ServerShared.GameSettings.Services;
using Genrpg.ServerShared.PlayerData;
using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Characters.Utils;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.GameSettings.PlayerData;
using Genrpg.Shared.Logging.Interfaces;
using Genrpg.Shared.Login.Interfaces;
using Genrpg.Shared.Login.Messages;
using Genrpg.Shared.Login.Messages.Error;
using Genrpg.Shared.Login.Messages.RefreshGameSettings;
using Genrpg.Shared.Users.Entities;
using Genrpg.Shared.Utils;
using Microsoft.Azure.Amqp.Framing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.LoginServer.Services.Clients
{
    public class NoUserService : INoUserService
    {
        private ILogService _logService = null;
        private ILoginServerService _loginServerService = null;

        public async Task HandleNoUserCommand(LoginContext context, string postData, CancellationToken token)
        {
            LoginServerCommandSet commandSet = SerializationUtils.Deserialize<LoginServerCommandSet>(postData);

            try
            {
                foreach (INoUserCommand comm in commandSet.Commands)
                {
                    INoUserCommandHandler handler = _loginServerService.GetNoUserCommandHandler(comm.GetType());

                    if (handler != null)
                    {
                        await handler.Execute(context, comm, token);
                    }
                }

                List<ILoginResult> errors = new List<ILoginResult>();

                foreach (ILoginResult result in context.Results)
                {
                    if (result is ErrorResult error)
                    {
                        errors.Add(error);
                    }
                }

                if (errors.Count > 0)
                {
                    context.Results.Clear();
                    context.Results.AddRange(errors);
                    return;
                }

            }
            catch (Exception e)
            {
                string errorMessage = "HandleLoginCommand." + commandSet.Commands.Select(x => x.GetType().Name + " ").ToList();
                _logService.Exception(e, errorMessage);
                WebUtils.ShowError(context, errorMessage);
            }

            return;
        }
    }
}
