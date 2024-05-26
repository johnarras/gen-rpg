using Assets.Scripts.Crawler.Services;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Currencies.Constants;
using Genrpg.Shared.Currencies.Messages;
using Genrpg.Shared.Currencies.PlayerData;
using System.Collections.Generic;

public class MoneyDisplay : BaseBehaviour
{

    private ICrawlerService _crawlerService;
    public List<MoneySegment> _segments;

    
    public bool UpdateToCharMoney = false;

    public override void Initialize(UnityGameState gs)
    {
        base.Initialize(gs);
        if (UpdateToCharMoney)
        {
            _dispatcher.AddEvent<OnAddCurrency>(this, OnCurrencyUpdate);
            UpdateValue();
        }
    }

    private void OnCurrencyUpdate(OnAddCurrency data)
    {
        UpdateValue();

        return;
    }

    protected void UpdateValue()
    {
        if (UpdateToCharMoney)
        {
            if (_gs.ch != null)
            {
                CurrencyData currencies = _gs.ch.Get<CurrencyData>();
                SetMoney(currencies.GetQuantity(CurrencyTypes.Money));
            }
        }
    }

    private const int SegmentDiv = 100;

    private long _money = -1; // Force at least one update.
    public void SetMoney(long money)
    {
        if (money < 0)
        {
            money = 0;
        }

        _money = money;

        if (_segments == null || _segments.Count < 1)
        {
            return;
        }

        long amountLeft = _money;
        for (int s = 0; s < _segments.Count; s++)
        {
            MoneySegment seg = _segments[s];

            long currAmount = amountLeft % SegmentDiv;
            if (currAmount == 0 && (money > 0 || s < _segments.Count-1))
            {
                GEntityUtils.SetActive(seg.GetParent(), false);               
            }
            else
            {
                GEntityUtils.SetActive(seg.GetParent(), true);
                seg.SetQuantityText(currAmount.ToString());
            }
            amountLeft /= SegmentDiv;
        }


      
    }
}