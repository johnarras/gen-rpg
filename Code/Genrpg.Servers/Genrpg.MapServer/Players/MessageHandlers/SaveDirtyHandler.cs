using Genrpg.MapServer.MapMessaging.MessageHandlers;
using Genrpg.ServerShared.PlayerData;
using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.Shared.Players.Constants;
using Genrpg.Shared.Players.Messages;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.MapServer.Players.MessageHandlers
{
    public class SaveDirtyHandler : BaseCharacterServerMapMessageHandler<SaveDirty>
    {
        protected IPlayerDataService _playerDataService = null;     
        protected override void InnerProcess(IRandom rand, MapMessagePackage pack, Character ch, SaveDirty message)
        {
            _playerDataService.SavePlayerData(ch, false);

            if (!message.IsCancelled())
            {
                _messageService.SendMessage(ch, message, PlayerConstants.SaveDelay);
            }
        }
    }
}
