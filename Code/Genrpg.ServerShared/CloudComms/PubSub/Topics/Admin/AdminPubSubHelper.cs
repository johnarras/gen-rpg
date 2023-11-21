using Genrpg.ServerShared.CloudComms.PubSub.Constants;
using Genrpg.ServerShared.CloudComms.PubSub.Topics.Admin.Entities;
using Genrpg.ServerShared.CloudComms.PubSub.Topics.Core;
using Genrpg.ServerShared.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.ServerShared.CloudComms.PubSub.Topics.Admin
{
    public class AdminPubSubHelper : BasePubSubHelper<IAdminPubSubMessage, IAdminPubSubMessageHandler>
    {
        public override string BaseTopicName() { return PubSubTopicNames.Admin.ToString(); }

    }
}
