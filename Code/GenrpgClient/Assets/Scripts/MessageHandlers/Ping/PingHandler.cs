using Cysharp.Threading.Tasks;
using Genrpg.Shared.Pings.Messages;
using System.Threading;

public class PingHandler : BaseClientMapMessageHandler<Ping>
{

    protected override void InnerProcess(UnityGameState gs,  Ping msg, CancellationToken token)
    {
    }
}
