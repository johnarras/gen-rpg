using Genrpg.LoginServer.Core;
using Genrpg.MonsterServer.MessageHandlers;
using Genrpg.ServerShared.Maps;
using Genrpg.ServerShared.MapSpawns;
using Genrpg.Shared.Constants;
using Genrpg.Shared.Login.Messages.UploadMap;
using Genrpg.Shared.MapServer.Entities;
using Microsoft.Azure.Amqp.Framing;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.LoginServer.CommandHandlers
{
    public class UploadMapHandler : BaseLoginCommandHandler<UploadMapCommand>
    {
        IMapDataService _mapDataService;
        protected override async Task InnerHandleMessage(LoginGameState gs, UploadMapCommand command, CancellationToken token)
        {
            if (gs.config.Env == EnvNames.Prod)
            {
                ShowError(gs, "Cannot update maps in live");
                return;
            }

            if (command == null)
            {
                ShowError(gs, "No map update data sent");
                return;
            }

            if (command.Map == null)
            {
                ShowError(gs, "Missing map on update");
                return;
            }

            if (command.Map.Zones == null || command.Map.Zones.Count < 1)
            {
                ShowError(gs, "Map had no zones");
                return;
            }

            if (command.SpawnData == null || command.SpawnData.Data == null)
            {
                ShowError(gs, "No spawn data sent to server");
                return;
            }

            IMapDataService mds = gs.loc.Get<IMapDataService>();
            await mds.SaveMap(gs, command.Map);

            IMapSpawnService mps = gs.loc.Get<IMapSpawnService>();
            await mps.SaveMapSpawnData(gs, command.SpawnData, Map.GetMapOwnerId(command.Map));
          

            foreach (ILoginCommandHandler handler in gs.commandHandlers.Values)
            {
                await handler.Reset();
            }

            gs.Results.Add(new UploadMapResult());
        }
    }
}
