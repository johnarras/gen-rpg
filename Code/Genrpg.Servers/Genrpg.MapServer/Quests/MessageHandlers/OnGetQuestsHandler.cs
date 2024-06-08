using Genrpg.ServerShared.PlayerData;
using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.Players.Constants;
using Genrpg.Shared.Quests.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Genrpg.Shared.Quests.Messages;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.MapServer.MapMessaging.MessageHandlers;
using Genrpg.Shared.Utils;

namespace Genrpg.MapServer.Quests.MessageHandlers
{
    public class OnGetQuestsHandler : BaseCharacterServerMapMessageHandler<OnGetQuests>
    {
        protected override void InnerProcess(IRandom rand, MapMessagePackage pack, Character ch, OnGetQuests message)
        {
            ch.AddMessage(message);
        }
    }
}
