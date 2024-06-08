using Genrpg.MapServer.MapMessaging.MessageHandlers;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Currencies.Messages;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.MapServer.Currencies.MessageHandlers
{
    public class OnAddCurrencyHandler : BaseMapObjectServerMapMessageHandler<OnAddCurrency>
    {
        protected override void InnerProcess(IRandom rand, MapMessagePackage pack, MapObject obj, OnAddCurrency message)
        {
            obj.AddMessage(message);
        }
    }
}
