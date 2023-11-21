using Genrpg.ServerShared.CloudComms.PubSub.Entities;
using Genrpg.ServerShared.Core;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.ServerShared.CloudComms.PubSub.Topics.Admin.Entities
{
    public abstract class BaseAdminPubSubMessageHandler<M> : BasePubSubMessageHandler<M>, IAdminPubSubMessageHandler where M : class, IPubSubMessage
    { 
    }
}
