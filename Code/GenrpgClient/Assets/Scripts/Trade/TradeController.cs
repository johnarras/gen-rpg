using Assets.Scripts.Core.Interfaces;
using Assets.Scripts.Crawler.StateHelpers.Combat;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Trades.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UI.Screens.Constants;

namespace Assets.Scripts.Trade
{
    public interface ITradeController : IInitializable
    {
        void HandleOnStartTrade(OnStartTrade onStartTrade);
    }

    public class TradeController : BaseBehaviour, IInjectOnLoad<ITradeController>, ITradeController
    {

        public async Task Initialize(CancellationToken token)
        {
            _dispatcher.AddEvent<OnStartTrade>(this, HandleOnStartTrade);
            await Task.CompletedTask;
        }

        public void HandleOnStartTrade(OnStartTrade onStartTrade)
        {
            _screenService.Open(ScreenId.Trade, onStartTrade);
        }
    }
}
