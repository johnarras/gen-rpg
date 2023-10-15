using Genrpg.MapServer.MapMessaging;
using Genrpg.ServerShared.PlayerData;
using Genrpg.Shared.Characters.Entities;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.Players.Constants;
using Genrpg.Shared.Players.Messages;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.MapServer.Players.MessageHandlers
{
    public class SaveDirtyHandler : BaseServerMapMessageHandler<SaveDirty>
    {
        IPlayerDataService _playerDataService = null;
        protected override void InnerProcess(GameState gs, MapMessagePackage pack, MapObject obj, SaveDirty message)
        {
            if (!_objectManager.GetChar(obj.Id, out Character ch))
            {
                return;
            }

            _playerDataService.SavePlayerData(ch, gs.repo, false);

            if (!message.IsCancelled())
            {
                _messageService.SendMessage(ch, message, PlayerConstants.SaveDelay);
            }
        }
    }
}
