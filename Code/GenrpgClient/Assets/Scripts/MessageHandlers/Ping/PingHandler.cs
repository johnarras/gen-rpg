
using Genrpg.Shared.Pings.Messages;
using System.Threading;

public class PingHandler : BaseClientMapMessageHandler<Ping>
{

    protected override void InnerProcess(Ping msg, CancellationToken token)
    {
    }
}
