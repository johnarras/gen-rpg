using Genrpg.LoginServer.CommandHandlers.Core;
using Genrpg.LoginServer.Core;
using Genrpg.LoginServer.PlayerData;
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

        public async Task Initialize(GameState gs, CancellationToken token)
        {
            await Task.CompletedTask;
        }

        public async Task<List<ILoginResult>> HandleNoUserCommand(LoginGameState gs, string postData, CancellationToken token)
        {
            LoginServerCommandSet commandSet = SerializationUtils.Deserialize<LoginServerCommandSet>(postData);

            try
            {
                foreach (INoUserCommand comm in commandSet.Commands)
                {
                    if (gs.noUserCommandHandlers.TryGetValue(comm.GetType(), out INoUserCommandHandler handler))
                    {
                        await handler.Execute(gs, comm, token);
                    }
                }

                List<ILoginResult> errors = new List<ILoginResult>();

                foreach (ILoginResult result in gs.Results)
                {
                    if (result is ErrorResult error)
                    {
                        errors.Add(error);
                    }
                }

                if (errors.Count > 0)
                {
                    return errors;
                }

            }
            catch (Exception e)
            {
                string errorMessage = "HandleLoginCommand." + commandSet.Commands.Select(x => x.GetType().Name + " ").ToList();
                _logService.Exception(e, errorMessage);
                WebUtils.ShowError(gs, errorMessage);
            }

            return gs.Results;
        }
    }
}
