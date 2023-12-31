﻿using Genrpg.MapServer.MapMessaging;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.MapObjects.Messages;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.MapServer.Spawns.MessageHandlers
{
    public class OnSpawnHandler : BaseServerMapMessageHandler<OnSpawn>
    {
        protected override void InnerProcess(GameState gs, MapMessagePackage pack, MapObject obj, OnSpawn message)
        {
            obj.AddMessage(message);
        }
    }
}
