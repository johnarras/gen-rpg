
using Genrpg.Shared.Errors.Messages;
using System.Threading;

public class ErrorMessageHandler : BaseClientMapMessageHandler<ErrorMessage>
{
    protected override void InnerProcess(UnityGameState gs, ErrorMessage msg, CancellationToken token)
    {
        gs.logger.Error(msg.ErrorText);
    }
}
