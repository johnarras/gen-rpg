using Genrpg.ServerShared.CloudComms.PubSub.Topics.Admin.Handlers;
using Genrpg.ServerShared.CloudComms.PubSub.Topics.Admin.Messages;
using Genrpg.ServerShared.Core;
using Genrpg.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.ServerShared.CloudComms.Services.Admin
{
    public interface IAdminService : IService
    {
        Task HandleReloadGameState(ServerGameState gs);
        Task OnServerStarted(ServerGameState gs, ServerStartedAdminMessage message);
        Task OnMapUploaded(ServerGameState gs,  MapUploadedAdminMessage message);
    }
}
