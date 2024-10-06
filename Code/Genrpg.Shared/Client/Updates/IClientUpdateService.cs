
using Genrpg.Shared.Interfaces;
using System.Threading;
using System;
using Genrpg.Shared.Client.Tokens;
using Genrpg.Shared.Client.Updates;

public interface IClientUpdateService : IInitializable, IMapTokenService, IGameTokenService, IGlobalUpdater
{
    void AddUpdate(object obj, Action funcIn, int updateType, CancellationToken token);
    void AddTokenUpdate(object obj, Action<CancellationToken> funcIn, int updateType, CancellationToken token);
    void AddDelayedUpdate(object obj, Action<CancellationToken> funcIn, float delaySeconds, CancellationToken token);
    void RunOnMainThread(Action funcIn);
}
