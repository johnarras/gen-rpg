﻿using Genrpg.LoginServer.CommandHandlers.Core;
using Genrpg.LoginServer.Core;
using Genrpg.LoginServer.PlayerData;
using Genrpg.LoginServer.Utils;
using Genrpg.ServerShared.PlayerData;
using Genrpg.Shared.Characters.Entities;
using Genrpg.Shared.GameSettings.Entities;
using Genrpg.Shared.Login.Interfaces;
using Genrpg.Shared.Login.Messages;
using Genrpg.Shared.Login.Messages.Error;
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
    public class ClientService : IClientService
    {

        private IPlayerDataService _playerDataService = null;

        public async Task<List<ILoginResult>> HandleClient(LoginGameState gs, string postData, CancellationToken token)
        {
            LoginServerCommandSet commandSet = SerializationUtils.Deserialize<LoginServerCommandSet>(postData);

            await LoadLoggedInPlayer(gs, commandSet.UserId, commandSet.SessionId);

            try
            {
                foreach (ILoginCommand comm in commandSet.Commands)
                {
                    if (gs.commandHandlers.TryGetValue(comm.GetType(), out ILoginCommandHandler handler))
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

                await SaveAll(gs);
            }
            catch (Exception e)
            {
                string errorMessage = "HandleLoginCommand." + commandSet.Commands.Select(x => x.GetType().Name + " ").ToList();
                gs.logger.Exception(e, errorMessage);
                WebUtils.ShowError(gs, errorMessage);
            }

            return gs.Results;
        }

        private async Task LoadLoggedInPlayer(LoginGameState gs, string userId, string sessionId)
        {
            gs.user = await gs.repo.Load<User>(userId);

            if (gs.user == null || gs.user.SessionId != sessionId)
            {
                return;
            }

            if (!string.IsNullOrEmpty(gs.user.CurrCharId))
            {
                gs.ch = await gs.repo.Load<Character>(gs.user.CurrCharId);
                await gs.ch.GetAsync<GameDataOverrideData>(gs);
            }

            return;
        }

        private async Task SaveAll(LoginGameState gs)
        {
            if (gs.user != null)
            {
                await gs.repo.Save(gs.user);
            }
            _playerDataService.SavePlayerData(gs.ch, gs.repo, true);
        }

    }
}