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
    public interface IAdminService : IInjectable
    {
        Task HandleReloadGameState();
        Task OnServerStarted(ServerStartedAdminMessage message);
        Task OnMapUploaded(MapUploadedAdminMessage message);
    }
}
