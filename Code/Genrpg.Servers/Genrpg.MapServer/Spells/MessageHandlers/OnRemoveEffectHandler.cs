using Genrpg.MapServer.MapMessaging;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.Shared.Spells.Messages;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.MapServer.Spells.MessageHandlers
{
    public class OnRemoveEffectHandler : BaseServerMapMessageHandler<OnRemoveEffect>
    {
        protected override void InnerProcess(GameState gs, MapMessagePackage pack, MapObject obj, OnRemoveEffect message)
        {
            obj.AddMessage(message);
        }
    }
}
