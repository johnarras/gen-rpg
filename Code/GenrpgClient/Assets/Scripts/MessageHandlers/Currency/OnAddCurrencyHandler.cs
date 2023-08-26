
using Genrpg.Shared.Currencies.Entities;
using Genrpg.Shared.Currencies.Messages;
using System.Threading;

namespace Assets.Scripts.MessageHandlers.Currency
{
    public class OnAddCurrencyHandler : BaseClientMapMessageHandler<OnAddCurrency>
    {
        protected override void InnerProcess(UnityGameState gs, OnAddCurrency msg, CancellationToken token)
        {
            if (msg.CharId != gs.ch.Id)
            {
                return;
            }
            CurrencyData cdata = gs.ch.Get<CurrencyData>();
            cdata.Add(msg.CurrencyTypeId, msg.QuantityAdded);
            gs.Dispatch(msg);
        }
    }
}
