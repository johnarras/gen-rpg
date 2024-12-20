﻿
using Genrpg.Shared.Errors.Messages;
using System.Threading;

public class ErrorMessageHandler : BaseClientMapMessageHandler<ErrorMessage>
{
    protected override void InnerProcess(ErrorMessage msg, CancellationToken token)
    {
        _logService.Error(msg.ErrorText);
    }
}
