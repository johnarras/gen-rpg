using Genrpg.ServerShared.CloudComms.Servers.PlayerServer.Queues;
using Genrpg.ServerShared.Core;

namespace Genrpg.PlayerServer.MessageHandlers
{
    public class WhoListRequestHandler : BasePlayerMessageHandler<WhoListRequest>
    {
        protected override async Task InnerHandleMessage(WhoListRequest message)
        {
            _playerService.OnGetWhoList(message);
            await Task.CompletedTask;
        }
    }
}
