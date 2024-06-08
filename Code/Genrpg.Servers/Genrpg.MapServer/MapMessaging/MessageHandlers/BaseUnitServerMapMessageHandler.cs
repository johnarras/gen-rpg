using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.GroundObjects.MapObjects;
using Genrpg.Shared.MapMessages.Interfaces;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.Units.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.MapServer.MapMessaging.MessageHandlers
{
    public abstract class BaseUnitServerMapMessageHandler<TMapMessage> : BaseServerMapMessageHandler<Unit,TMapMessage>
        where TMapMessage : class, IMapMessage, new()
    {
    }
}
