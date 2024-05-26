using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.Inventory.Services;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.Currencies.PlayerData;
using Genrpg.Shared.Currencies.Services;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Inventory.Messages;
using Genrpg.Shared.Currencies.Constants;
using Genrpg.ServerShared.Achievements;
using Genrpg.Shared.Achievements.Constants;
using Genrpg.Shared.Inventory.PlayerData;
using Genrpg.Shared.Inventory.Utils;
using Genrpg.Shared.Vendors.WorldData;
using Genrpg.Shared.Vendors.MapObjectAddons;
using Genrpg.Shared.MapServer.Entities;
using Amazon.SecurityToken.Model.Internal.MarshallTransformations;
using Genrpg.MapServer.MapMessaging.MessageHandlers;
using Genrpg.MapServer.Trades.Services;
using Genrpg.Shared.Interfaces;
using System.Configuration;
using Genrpg.MapServer.Vendors.Services;

namespace Genrpg.MapServer.Vendors.MessageHandlers
{
    public class BuyItemHandler : BaseServerMapMessageHandler<BuyItem>
    {
        private IVendorService _vendorService;
        public override void Setup(GameState gs)
        {
            base.Setup(gs);
        }
        protected override void InnerProcess(GameState gs, MapMessagePackage pack, MapObject obj, BuyItem message)
        {
            _vendorService.BuyItem(gs, obj, message);
        }
    }
}
