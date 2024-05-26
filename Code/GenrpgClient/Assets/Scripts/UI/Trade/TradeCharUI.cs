using Assets.Scripts.UI.Services;
using Assets.Scripts.UI.Trade;
using Genrpg.Shared.Inventory.PlayerData;
using Genrpg.Shared.Trades.Entities;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine.Events;
using UnityEngine.UI;


public class TradeCharUI : BaseBehaviour
{
    private IUIService _uiService;

    public GInputField MoneyInput;

    public List<ItemIcon> ItemIcons;
    public List<TradeItemIcon> TradeIcons;

    public GImage Background;

    private TradeScreen _tradeScreen;
    private TradeChar _tradechar;
    private string _charId = null;
    private CancellationToken _token;

    public string GetCharId()
    {
        return _charId;
    }

    public void Init(string charId, TradeScreen tradeScreen, CancellationToken token)
    {
        _charId = charId;
        _token = token;
        _tradeScreen = tradeScreen;
        SetTradeChar(null, token);

        if (MoneyInput != null)
        {
            MoneyInput.onValueChanged.AddListener(delegate { UpdateMoney(MoneyInput); });
        }
    }

    private long _oldMoneyValue = 0;
    private void UpdateMoney(GInputField inputField)
    {
        int newVal = _uiService.GetIntInput(inputField);
        if (newVal != _oldMoneyValue)
        {
            _oldMoneyValue = newVal;
            _tradeScreen.SendUpdateMessage();
        }
    }

    public void SetTradeChar(TradeChar tradeChar, CancellationToken token)
    {
        _tradechar = tradeChar;

        if (_tradechar == null)
        {
            SetMoney(0);

            for (int i = 0; i < TradeIcons.Count; i++)
            {
                InitIcon(i, null, token);
            }
        }
        else
        {
            _charId = _tradechar.CharId;
            SetMoney(_tradechar.Money);
            if (_tradechar.CharId != _gs.ch.Id)
            {
                if (MoneyInput != null)
                {
                    MoneyInput.interactable = false;
                }
            }
            for (int i = 0; i < TradeIcons.Count; i++)
            {
                InitIcon(i, _tradechar.Items[i], token);
            }
        }
    }

    public int GetMoney()
    {
        return _uiService.GetIntInput(MoneyInput);
    }

    public void SetMoney(long money)
    {
        _uiService.SetInputText(MoneyInput, money.ToString());
    }

    public void InitIcon(int index, Item item, CancellationToken token)
    {
        InitItemIconData iconInitData = new InitItemIconData()
        {
            Data = item,
            Screen = _tradeScreen,
        };
        if (_charId != _gs.ch.Id)
        {
            iconInitData.Flags |= ItemIconFlags.NoDrag;
        }
        TradeIcons[index].ItemIcon.Init(iconInitData, token);
    }

    public void ShowAccepted(bool accepted)
    {
        if (Background != null)
        {
            Background.Color = (accepted ? GColor.gray : GColor.white);
        }
    }
}
