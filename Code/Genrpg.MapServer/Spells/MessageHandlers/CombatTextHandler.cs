﻿using Genrpg.MapServer.MapMessaging;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.Spells.Messages;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.MapServer.Spells.MessageHandlers
{
    public class CombatTextHandler : BaseServerMapMessageHandler<CombatText>
    {
        protected override void InnerProcess(GameState gs, MapMessagePackage pack, MapObject obj, CombatText message)
        {
            obj.AddMessage(message);
        }
    }
}
