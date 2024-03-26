using Genrpg.ServerShared.CloudComms.PubSub.Topics.Admin.Entities;
using Genrpg.ServerShared.CloudComms.PubSub.Topics.Admin.Messages;
using Genrpg.ServerShared.CloudComms.Services.Admin;
using Genrpg.ServerShared.Core;
using Genrpg.ServerShared.GameSettings.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.ServerShared.CloudComms.PubSub.Topics.Admin.Handlers
{
    public class UpdateGameDataAdminMessageHandler : BaseAdminPubSubMessageHandler<UpdateGameDataAdminMessage>
    {
        public override Type GetKey() { return typeof(UpdateGameDataAdminMessage); }

        protected override async Task InnerHandleMessage(ServerGameState gs, UpdateGameDataAdminMessage message, CancellationToken token)
        {
            _logService.Message("Received Update Game Data Message ");
            await _adminService.HandleReloadGameState(gs);
            await Task.CompletedTask;
        }
    }
}
