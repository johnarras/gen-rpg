using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Genrpg.MapServer.MapMessaging.MessageHandlers;
using Genrpg.MapServer.Vendors.Services;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.MapObjects.MapObjectAddons.Entities;
using Genrpg.Shared.MapObjects.Messages;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Vendors.MapObjectAddons;
using Genrpg.Shared.Vendors.Settings;

namespace Genrpg.MapServer.MapObjects.Messages
{
    public class GetMapObjectStatusHandler : BaseMapObjectServerMapMessageHandler<GetMapObjectStatus>
    {

        private IVendorService _vendorService = null;

        protected override void InnerProcess(IRandom rand, MapMessagePackage pack, MapObject obj, GetMapObjectStatus message)
        {
            OnGetMapObjectStatus result = new OnGetMapObjectStatus() { ObjId = message.ObjId };
            if (_objectManager.GetObject(message.ObjId, out MapObject mapObject))
            {
                result.Addons = mapObject.GetAddons();
                _vendorService.UpdateItems(rand, mapObject);
            }

            _messageService.SendMessage(obj, result);
        }
    }
}
