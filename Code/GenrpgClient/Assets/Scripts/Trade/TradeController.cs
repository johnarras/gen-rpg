using Assets.Scripts.Core.Interfaces;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Trades.Messages;
using Genrpg.Shared.UI.Entities;
using System.Threading;
using System.Threading.Tasks;

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
            AddListener<OnStartTrade>(HandleOnStartTrade);
            await Task.CompletedTask;
        }

        public void HandleOnStartTrade(OnStartTrade onStartTrade)
        {
            _screenService.Open(ScreenId.Trade, onStartTrade);
        }
    }
}
