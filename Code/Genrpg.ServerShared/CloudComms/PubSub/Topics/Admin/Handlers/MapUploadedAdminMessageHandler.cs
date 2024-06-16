using Genrpg.ServerShared.CloudComms.PubSub.Topics.Admin.Entities;
using Genrpg.ServerShared.CloudComms.PubSub.Topics.Admin.Messages;
using Genrpg.ServerShared.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.ServerShared.CloudComms.PubSub.Topics.Admin.Handlers
{
    public class MapUploadedAdminMessageHandler : BaseAdminPubSubMessageHandler<MapUploadedAdminMessage>
    {
        public override Type GetKey() {  return typeof(MapUploadedAdminMessage); }

        protected override async Task InnerHandleMessage(MapUploadedAdminMessage message, CancellationToken token)
        {
            await _adminService.OnMapUploaded(message);
        }
    }
}
