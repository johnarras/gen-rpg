using Genrpg.ServerShared.CloudComms.PubSub.Entities;
using Genrpg.ServerShared.CloudComms.Services.Admin;
using Genrpg.ServerShared.Core;
using Genrpg.Shared.Logging.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.ServerShared.CloudComms.PubSub.Topics.Admin.Entities
{
    public abstract class BaseAdminPubSubMessageHandler<M> : BasePubSubMessageHandler<M>, IAdminPubSubMessageHandler where M : class, IPubSubMessage
    {
        protected IAdminService _adminService = null;
        protected ILogService _logService = null;
    }
}
