
using Genrpg.Shared.Currencies.Messages;
using Genrpg.Shared.Currencies.Services;
using System.Threading;

namespace Assets.Scripts.MessageHandlers.Currency
{
    public class OnAddCurrencyHandler : BaseClientMapMessageHandler<OnAddCurrency>
    {
        private ICurrencyService _currencyService = null;
        protected override void InnerProcess(UnityGameState gs, OnAddCurrency msg, CancellationToken token)
        {
            if (msg.CharId != gs.ch.Id)
            {
                return;
            }
            _currencyService.Add(gs.ch, msg.CurrencyTypeId, msg.QuantityAdded);
            _dispatcher.Dispatch(msg);
        }
    }
}
