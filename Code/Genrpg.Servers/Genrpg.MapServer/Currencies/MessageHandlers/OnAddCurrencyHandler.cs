using Genrpg.MapServer.MapMessaging.MessageHandlers;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Currencies.Messages;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.MapServer.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.MapServer.Currencies.MessageHandlers
{
    public class OnAddCurrencyHandler : BaseServerMapMessageHandler<OnAddCurrency>
    {
        protected override void InnerProcess(GameState gs, MapMessagePackage pack, MapObject obj, OnAddCurrency message)
        {
            obj.AddMessage(message);
        }
    }
}
