using Genrpg.ServerShared.CloudComms.PubSub.Topics.Admin.Entities;
using Genrpg.ServerShared.CloudComms.PubSub.Topics.Admin.Messages;
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
        private IGameDataService _gameDataService = null;

        public override Type GetKey() { return typeof(UpdateGameDataAdminMessage); }

        protected override async Task InnerHandleMessage(ServerGameState gs, UpdateGameDataAdminMessage message, CancellationToken token)
        {
            gs.logger.Message("Received Update Game Data Message ");
            await _gameDataService.ReloadGameData(gs);
            await Task.CompletedTask;
        }
    }
}
