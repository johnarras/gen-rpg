using Genrpg.LoginServer.CommandHandlers;
using Genrpg.LoginServer.Core;
using Genrpg.ServerShared.PlayerData;
using Genrpg.Shared.Login.Interfaces;
using Genrpg.Shared.Login.Messages;
using Genrpg.Shared.Login.Messages.Error;
using Genrpg.Shared.Login.Messages.Login;
using Genrpg.Shared.Reflection.Services;
using Genrpg.Shared.Utils;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ZstdSharp.Unsafe;

namespace Genrpg.LoginServer.Controllers
{

    [Route("[controller]")]
    [ApiController]
    public class ClientController : BaseWebController
    {

        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "Client" };
        }

        [HttpPost]
        public async Task<string> Post([FromForm] string Data)
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            List<ILoginCommand> comms = await LoadSessionCommands(Data, cts.Token);

            foreach (ILoginCommand comm in comms)
            {
                if (gs.commandHandlers.TryGetValue(comm.GetType(), out ILoginCommandHandler handler))
                { 
                    await handler.Execute(gs, comm, cts.Token);
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
                return PackageResults(errors);
            }

            await SaveAll(gs);

            return PackageResults(gs.Results);
        }

        private static async Task SaveAll(LoginGameState gs)
        {
            if (gs.user != null)
            {
                await gs.repo.Save(gs.user);
            }
            PlayerDataUtils.SavePlayerData(gs.ch, gs.repo, true);
        }

    }
}
