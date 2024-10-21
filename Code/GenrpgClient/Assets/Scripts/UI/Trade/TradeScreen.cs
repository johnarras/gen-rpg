using UnityEngine;
using System.Threading;
using System.Collections.Generic;
using Genrpg.Shared.Trades.Entities;
using Genrpg.Shared.Trades.Messages;
using Genrpg.Shared.Inventory.Constants;
using System.Linq;
using Genrpg.Shared.Inventory.PlayerData;
using Genrpg.Shared.Inventory.Services;
using System.Threading.Tasks;
using Genrpg.Shared.Client.GameEvents;

public class TradeScreen : ItemIconScreen
{
    private IInventoryService _inventoryService;

    public List<TradeCharUI> TradeChars;
    public InventoryPanel Items;
    public GText Header;
    public GButton AcceptButton;

    private string _charName;
    private string _charId;

    private TradeObject _tradeObject = null;

    protected override async Task OnStartOpen(object data, CancellationToken token)
    {
        await base.OnStartOpen(data, token);
        AddListener<OnUpdateTrade>(HandleOnUpdateTrade);
        AddListener<OnCancelTrade>(HandleOnCancelTrade);
        AddListener<OnCompleteTrade>(HandleOnCompleteTrade);
        AddListener<OnAcceptTrade>(HandleOnAcceptTrade);

        _uiService.SetButton(AcceptButton, GetName(), ClickAccept);

        OnStartTrade onStartTrade = data as OnStartTrade;

        if (onStartTrade == null)
        {
            StartClose();
            return;
        }
        _charName = onStartTrade.Name;
        _charId = onStartTrade.CharId;
        TradeChars[0].Init(_gs.ch.Id, this, token);
        TradeChars[1].Init(_charId, this, token);

        _uiService.SetText(Header, "Trading with " + _charName);

        Items?.Init(InventoryGroup.All, this, _gs.ch, null, token);

        
    }

    
    public void HandleOnUpdateTrade(OnUpdateTrade onUpdateTrade)
    {
        _tradeObject = onUpdateTrade.TradeObject;

        foreach (TradeChar tradeChar in _tradeObject.Chars)
        {
            TradeCharUI ui = TradeChars.FirstOrDefault(x => x.GetCharId() == tradeChar.CharId);
            if (ui == null)
            {
                continue;
            }

            _uiService.SetInputText(ui.MoneyInput, tradeChar.Money);

            for (int i = 0; i < ui.ItemIcons.Count; i++)
            {
                InitItemIconData initData = new InitItemIconData()
                {
                    Data = tradeChar.Items[i],
                };

                if (ui.GetCharId() != _gs.ch.Id)
                {
                    initData.Flags |= ItemIconFlags.NoDrag;
                }

                ui.ItemIcons[i].Init(initData, _token);
            }
        }
        ClearAccepted();

    }

    protected override void OnStartClose()
    {
        _networkService.SendMapMessage(new CancelTrade() { CharId = _gs.ch.Id });
    }

    public void HandleOnCompleteTrade(OnCompleteTrade onCompleteTrade)
    {
        foreach (TradeChar tradeChar in onCompleteTrade.TradeObject.Chars)
        {
            for (int i = 0; i < tradeChar.Items.Length; i++)
            {
                if (tradeChar.Items[i] != null)
                {
                    if (tradeChar.CharId == _gs.ch.Id)
                    {
                        _inventoryService.RemoveItem(_gs.ch, tradeChar.Items[i].Id, true);
                    }
                    else
                    {
                        _inventoryService.AddItem(_gs.ch, tradeChar.Items[i], true);
                    }
                }
            }
        }

        StartClose();
    }

    public void HandleOnCancelTrade(OnCancelTrade onCancelTrade)
    {
        if (!string.IsNullOrEmpty(onCancelTrade.ErrorMessage))
        {
            _dispatcher.Dispatch(new ShowFloatingText(onCancelTrade.ErrorMessage, EFloatingTextArt.Error));
        }
        StartClose();
    }

    public void HandleOnAcceptTrade(OnAcceptTrade onAcceptTrade)
    {

        for (int i = 0; i < TradeChars.Count; i++)
        {
            if (onAcceptTrade.CharId == TradeChars[i].GetCharId())
            {
                TradeChars[i].ShowAccepted(true);
            }
        }
    }

    private void ClearAccepted()
    {
        foreach (TradeCharUI tradeCharUI in TradeChars)
        {
            tradeCharUI.ShowAccepted(false);
        }
        if (AcceptButton != null)
        {
            AcceptButton.enabled = true;
        }
    }


    protected override void HandleDragDrop(ItemIconScreen startScreen, ItemIcon dragIcon, ItemIcon otherIconHit, GameObject finalObjectHit)
    {

        bool changedSomething = false;
        TradeCharUI parentUI = null;
        if (otherIconHit != null)
        {
            parentUI = _clientEntityService.FindInParents<TradeCharUI>(otherIconHit.gameObject);
        }

        // If landing spot is not one of our icons, bail out.
        if (parentUI != TradeChars[0])
        {
            for (int i = 0; i < TradeChars[0].ItemIcons.Count; i++)
            {
                ItemIcon itemIcon = TradeChars[0].ItemIcons[i];
                Item item = itemIcon.GetDataItem();
                if (item != null && item == dragIcon.GetDataItem())
                {
                    TradeChars[0].InitIcon(i, null, _token);
                    changedSomething = true;
                    break;
                }
            }

            _logService.Info("Hit wrong side icon");
            return;
        }
        else
        {
            _logService.Info("Hit correct side icon");
            for (int i = 0; i < TradeChars[0].TradeIcons.Count; i++)
            {
                ItemIcon icon = TradeChars[0].ItemIcons[i];
                if (icon == otherIconHit)
                {
                    TradeChars[0].InitIcon(i, dragIcon.GetDataItem(), _token);
                    changedSomething = true;
                }
                else if (icon.GetDataItem() == dragIcon.GetDataItem())              
                {
                    TradeChars[0].InitIcon(i, null, _token);
                }
            }
        }

        if (changedSomething)
        {
            ClearAccepted();
            SendUpdateMessage();
        }

    }

    public void SendUpdateMessage()
    {
        UpdateTrade updateTrade = new UpdateTrade();
        updateTrade.Money = _uiService.GetIntInput(TradeChars[0].MoneyInput);

        for (int i = 0; i < updateTrade.ItemIds.Length; i++)
        {
            updateTrade.ItemIds[i] = TradeChars[0].ItemIcons[i].GetDataItem()?.Id ?? null;
        }

        _networkService.SendMapMessage(updateTrade);
    }

    private void ClickAccept()
    {
        TradeChars[0].ShowAccepted(true);
        if (AcceptButton != null)
        {
            AcceptButton.enabled = false;
        }
        _networkService.SendMapMessage(new AcceptTrade() { CharId = _gs.ch.Id });
    }
}