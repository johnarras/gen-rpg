using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.Inventory.Services;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.Currencies.PlayerData;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Inventory.Messages;
using Genrpg.Shared.Currencies.Constants;
using Genrpg.ServerShared.Achievements;
using Genrpg.Shared.Achievements.Constants;
using Genrpg.Shared.Inventory.PlayerData;
using Genrpg.Shared.Inventory.Utils;
using Genrpg.Shared.Vendors.MapObjectAddons;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.MapServer.MapMessaging.MessageHandlers;
using Genrpg.MapServer.Trades.Services;
using Genrpg.MapServer.Vendors.Services;
using Genrpg.Shared.Utils;

namespace Genrpg.MapServer.Vendors.MessageHandlers
{
    public class SellItemHandler : BaseMapObjectServerMapMessageHandler<SellItem>
    {

        private IVendorService _vendorService = null!;

        protected override void InnerProcess(IRandom rand, MapMessagePackage pack, MapObject obj, SellItem message)
        {
            _vendorService.SellItem(rand, obj, message);
        }
    }
}
