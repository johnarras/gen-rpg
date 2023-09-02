using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Genrpg.MapServer.MapMessaging;
using Genrpg.MapServer.Vendors;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.NPCs.Entities;
using Genrpg.Shared.NPCs.Messages;
using Genrpg.Shared.Units.Entities;

namespace Genrpg.MapServer.Units.MessageHandlers
{
    public class GetNPCStatusHandler : BaseServerMapMessageHandler<GetNPCStatus>
    {

        private IVendorService _vendorService;

        protected override void InnerProcess(GameState gs, MapMessagePackage pack, MapObject obj, GetNPCStatus message)
        {
            OnGetNPCStatus result = new OnGetNPCStatus() { UnitId = message.UnitId };
            if (!_objectManager.GetUnit(message.UnitId, out Unit unit))
            {
                SendResult(gs, obj, result);
                return;
            }

            if (unit.NPCStatus == null || unit.NPCType == null)
            {
                SendResult(gs, obj, result);
                return;
            }

            float refreshMinutes = gs.data.GetGameData<VendorSettings>().VendorRefreshMinutes;
            if (refreshMinutes > 0 &&
                unit.NPCStatus.LastItemRefresh > DateTime.UtcNow.AddMinutes(-refreshMinutes))
            {
                result.Status = unit.NPCStatus;
                SendResult(gs, obj, result);
                return;
            }
            _vendorService.UpdateNPCItems(gs, unit);

            result.Status = unit.NPCStatus;
            SendResult(gs, obj, result);
        }

        protected void SendResult(GameState gs, MapObject obj, OnGetNPCStatus result)
        {
            _messageService.SendMessage(obj, result);
        }
    }
}
