using Azure.ResourceManager.ServiceBus;
using Genrpg.MapServer.MapMessaging.Interfaces;
using Genrpg.MapServer.Maps;
using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Currencies.Constants;
using Genrpg.Shared.Currencies.PlayerData;
using Genrpg.Shared.Currencies.Services;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.Errors.Messages;
using Genrpg.Shared.Inventory.PlayerData;
using Genrpg.Shared.Inventory.Services;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.Trades.Constants;
using Genrpg.Shared.Trades.Entities;
using Genrpg.Shared.Trades.Messages;
using Genrpg.Shared.Units.Constants;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.Users.Entities;
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.MapServer.Trades.Services
{
    public class TradeService : ITradeService
    {
        private IMapObjectManager _objManager;
        private IMapMessageService _messageService;
        private IInventoryService _inventoryService;
        private ICurrencyService _currencyService;
        private IRepositoryService _repoService;

        #region Utils
        private void SendError(Character ch, string message)
        {
            ch.AddMessage(new ErrorMessage(message));
        }

        private void ProcessExistingTrade(Character ch, Action<FullTradeObject> internalTradeAction)
        {
            FullTradeObject fullTrade = GetFullTradeObject(ch);

            if (!string.IsNullOrEmpty(fullTrade.ErrorMessage))
            {
                ch.AddMessage(new OnCancelTrade() { CharId = ch.Id, ErrorMessage = fullTrade.ErrorMessage });
                return;
            }

            lock (fullTrade.TradeObject)
            {
                lock (fullTrade.OrderedCharacters[0].TradeLock)
                {
                    lock (fullTrade.OrderedCharacters[1].TradeLock)
                    {
                        if (!fullTrade.IsOkToUpdate())
                        {
                            foreach (Character tch in fullTrade.OrderedCharacters)
                            {
                                CancelCharTrade(tch);
                                return;
                            }
                        }

                        internalTradeAction(fullTrade);
                    }
                }
            }
        }

        private FullTradeObject GetFullTradeObject(Character ch)
        {
            FullTradeObject fullTrade = new FullTradeObject();

            if (ch.Trade == null)
            {
                fullTrade.ErrorMessage = "You're not trading";
                return fullTrade;
            }

            string otherId = ch.Id;
            bool foundMyCharId = false;
            for (int i = 0; i < ch.Trade.Chars.Length; i++)
            {
                if (ch.Trade.Chars[i].CharId != ch.Id)
                {
                    otherId = ch.Trade.Chars[i].CharId;
                }
                else
                {
                    foundMyCharId = true;
                }
            }

            if (!_objManager.GetChar(otherId, out Character ch2))
            {
                CancelCharTrade(ch);
                fullTrade.ErrorMessage = "Other character does not exist";
                return fullTrade;
            }

            if (!foundMyCharId)
            {
                CancelCharTrade(ch);
                CancelCharTrade(ch2);
                fullTrade.ErrorMessage = "You aren't in this trade.";
                return fullTrade;
            }

            fullTrade.OrderedCharacters.Add(ch);

            if (ch.Id.CompareTo(otherId) < 0)
            {
                fullTrade.OrderedCharacters.Add(ch2);
            }
            else
            {
                fullTrade.OrderedCharacters.Insert(0, ch2);
            }

            fullTrade.TradeObject = ch.Trade;

            return fullTrade;

        }
        #endregion

        #region Accept
        public void HandleAcceptTrade(Character ch, AcceptTrade acceptTrade)
        {
            ProcessExistingTrade(ch, delegate (FullTradeObject fullTrade)
                {
                    HandleAcceptTradeInternal(ch, acceptTrade, fullTrade);
                });
        }

        private void HandleAcceptTradeInternal(Character ch, AcceptTrade acceptTrade, FullTradeObject fullTrade)
        {
            foreach (TradeChar tch in fullTrade.TradeObject.Chars)
            {
                if (tch.CharId == ch.Id)
                {
                    tch.Accepted = true;
                    break;
                }
            }

            bool allAccepted = true;

            foreach (TradeChar tch in fullTrade.TradeObject.Chars)
            {
                if (!tch.Accepted)
                {
                    allAccepted = false;
                    break;
                }
            }

            foreach (Character tch in fullTrade.OrderedCharacters)
            {
                tch.AddMessage(new OnAcceptTrade() { CharId = ch.Id });
            }
            if (!allAccepted)
            {
                return;
            }

            // All accepted so complete.
            fullTrade.TradeObject.State = ETradeStates.Complete;

            foreach (Character tch in fullTrade.OrderedCharacters)
            {
                tch.Trade = null;
            }

            for (int c = 0; c < fullTrade.TradeObject.Chars.Length; c++)
            {
                TradeChar currTrade = fullTrade.TradeObject.Chars[c];

                Character currChar = fullTrade.OrderedCharacters[c];
                Character otherChar = fullTrade.OrderedCharacters[1 - c];

                if (currTrade.Money > 0)
                {
                    _currencyService.Add(currChar, CurrencyTypes.Money, -currTrade.Money);
                    _currencyService.Add(otherChar, CurrencyTypes.Money, currTrade.Money);
                }

                for (int i = 0; i < currTrade.Items.Length; i++)
                {
                    if (currTrade.Items[i] != null)
                    {
                        currTrade.Items[i].OwnerId = otherChar.Id;
                        _repoService.QueueSave(currTrade.Items[i]);
                    }
                }
            }
            foreach (Character tch in fullTrade.OrderedCharacters)
            {
                _messageService.SendMessage(tch, new OnCompleteTrade() { TradeObject = fullTrade.TradeObject });
            }
        }

        public void HandleOnAcceptTrade(Character ch, OnAcceptTrade onAcceptTrade)
        {
            ch.AddMessage(onAcceptTrade);
        }
        #endregion

        #region Cancel
        public void HandleCancelTrade(Character ch, CancelTrade cancelTrade)
        {
            ProcessExistingTrade(ch, delegate (FullTradeObject fullTrade)
            {
                HandleCancelTradeInternal(ch, cancelTrade, fullTrade);
            });
        }

        private void HandleCancelTradeInternal(Character ch, CancelTrade cancelTrade, FullTradeObject fullTrade)
        { 
            foreach (Character tradeChar in fullTrade.OrderedCharacters)
            {
                CancelCharTrade(tradeChar);
            }
        }

        private void CancelCharTrade(Character ch)
        {
            if (ch.Trade != null)
            {
                ch.Trade.State = ETradeStates.Cancelled;
                ch.Trade = null;
            }
            ch.AddMessage(new OnCancelTrade());
        }

        public void HandleOnCancelTrade(Character ch, OnCancelTrade onCancelTrade)
        {
            ch.Trade = null;
            ch.AddMessage(onCancelTrade);
        }
        #endregion

        #region Start
        public void HandleStartTrade(Character ch, StartTrade startTrade)
        {
            if (ch.Id == startTrade.CharId)
            {
                SendError(ch, "You cannot trade with yourself.");
                return;
            }

            if (!_objManager.GetChar(startTrade.CharId, out Character ch2))
            {
                SendError(ch, "That player does not exist.");
            }

            List<Character> orderedChars = new List<Character>();

            orderedChars.Add(ch);
            if (string.Compare(ch.Id, startTrade.CharId) < 0)
            {
                orderedChars.Add(ch2);
            }
            else
            {
                orderedChars.Insert(0, ch2);
            }

            lock (orderedChars[0].TradeLock)
            {
                if (orderedChars[0].Trade != null)
                {
                    if (orderedChars[0] == ch)
                    {
                        SendError(ch, "You are already trading.");
                        return;
                    }
                    else
                    {
                        SendError(ch, "They are already trading.");
                        return;
                    }
                }
                lock (orderedChars[1].TradeLock)
                {
                    if (orderedChars[1].Trade != null)
                    {
                        if (orderedChars[1] == ch)
                        {
                            SendError(ch, "You are already trading.");
                            return;
                        }
                        else
                        {
                            SendError(ch, "They are already trading.");
                            return;
                        }
                    }

                    if (ch.FactionTypeId != ch2.FactionTypeId)
                    {
                        SendError(ch, "You cannot trade with other factions");
                        return;
                    }

                    if (ch.HasFlag(UnitFlags.IsDead))
                    {
                        SendError(ch, "You are dead");
                        return;
                    }

                    if (ch2.HasFlag(UnitFlags.IsDead))
                    {
                        SendError(ch, "They are dead");
                        return;
                    }

                    TradeObject tradeObject = new TradeObject();
                    for (int i = 0; i < orderedChars.Count; i++)
                    {
                        tradeObject.Chars[i].CharId = orderedChars[i].Id;
                        tradeObject.Chars[i].CharName = orderedChars[i].Name;
                        orderedChars[i].Trade = tradeObject;
                    }
                    
                    OnUpdateTrade onUpdateTrade = new OnUpdateTrade() { TradeObject = tradeObject };

                    for (int i = 0; i < orderedChars.Count; i++)
                    {
                        Character currChar = orderedChars[i];
                        Character otherChar = orderedChars[1 - i];

                        currChar.AddMessage(new OnStartTrade() { CharId = otherChar.Id, Name = otherChar.Name });
                    }
                }
            }
        }

        #endregion

        #region SafeModifyObject
        /// <summary>
        /// Try to safely modify inventory in the face of trades. This probably has issues
        /// due to race conditions, so I expect to have to revisit this often.
        /// Not sure how to do the "lock once and don't re lock"
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="modifyFunc"></param>
        /// <param name="failValue"></param>
        /// <returns></returns>
        public T SafeModifyObject<T>(MapObject obj, Func<T> modifyFunc, T failValue)
        {
            if (obj is Character ch)
            {
                if (Interlocked.Read(ref ch.TradeModifyLockCount) > 0)
                {
                    // This may race, not sure how to fix as of now.
                    // OTOH all inventory messages to this player will go through
                    // the same queue, even the trade result messages,
                    // so probably this isn't an issue.
                    return modifyFunc();
                }
                else
                {
                    lock (ch.TradeLock)
                    {
                        Interlocked.Increment(ref ch.TradeModifyLockCount);

                        if (ch.Trade != null)
                        {
                            ch.AddMessage(new ErrorMessage("You are trading."));
                            return default(T);
                        }

                        T returnVal = modifyFunc();
                        Interlocked.Decrement(ref ch.TradeModifyLockCount);
                        return returnVal;
                    }
                }
            }
            else
            {
                return modifyFunc();
            }
        }

        public void SafeModifyObject(MapObject obj, Action modifyFunc)
        {
            Func<bool> wrapper = delegate { modifyFunc(); return true; };
            SafeModifyObject(obj, wrapper, false);
        }
        #endregion

        #region Update
        public void HandleUpdateTrade(Character ch, UpdateTrade updateTrade)
        {
            ProcessExistingTrade(ch, delegate (FullTradeObject fullTrade)
            {
                HandleUpdateTradeInternal(ch, updateTrade, fullTrade);
            });
        }

        private void HandleUpdateTradeInternal(Character ch, UpdateTrade updateTrade, FullTradeObject fullTrade)
        {
            TradeChar tradeChar = null;
            for (int i = 0; i < fullTrade.TradeObject.Chars.Length; i++)
            {
                if (fullTrade.TradeObject.Chars[i].CharId == ch.Id)
                {
                    tradeChar = fullTrade.TradeObject.Chars[i];
                    break;
                }
            }

            if (tradeChar == null)
            {
                SendError(ch, "You aren't in this trade.");
                return;
            }

            long charMoney = ch.Get<CurrencyData>().Get(CurrencyTypes.Money).Quantity;

            if (charMoney < updateTrade.Money)
            {
                HandleCancelTradeInternal(ch, new CancelTrade() { CharId = ch.Id }, fullTrade);
                SendError(ch, "You don't have enough money.");
                return;
            }

            InventoryData inventoryData = ch.Get<InventoryData>();

            Item[] newItems = new Item[TradeConstants.MaxItems];


            List<string> itemIds = new List<string>();
            for (int i = 0; i < updateTrade.ItemIds.Length; i++)
            {
                if (string.IsNullOrEmpty(updateTrade.ItemIds[i]))
                {
                    continue;
                }

                if (itemIds.Contains(updateTrade.ItemIds[i]))
                {
                    HandleCancelTradeInternal(ch, new CancelTrade() { CharId = ch.Id }, fullTrade);
                    SendError(ch, "The same item is in the trade twice.");
                }

                Item myItem = inventoryData.GetItem(updateTrade.ItemIds[i]);
                if (myItem == null)
                {
                    HandleCancelTradeInternal(ch, new CancelTrade() { CharId = ch.Id }, fullTrade);
                    SendError(ch, "You are missing an item.");
                    return;
                }
                newItems[i] = myItem;
            }

            tradeChar.Money = updateTrade.Money;
            tradeChar.Items = newItems;
          
            foreach (TradeChar tradeChar2 in fullTrade.TradeObject.Chars)
            {
                tradeChar2.Accepted = false;
            }

            foreach (Character ch2 in fullTrade.OrderedCharacters)
            {
                ch2.AddMessage(new OnUpdateTrade() { TradeObject = fullTrade.TradeObject });
            }
        }

        public void HandleOnUpdateTrade(Character ch, OnUpdateTrade onUpdateTrade)
        {
            ch.AddMessage(onUpdateTrade);
        }
        #endregion

        #region Complete

        public void HandleOnCompleteTrade(Character ch, OnCompleteTrade onCompleteTrade)
        {
            ch.Trade = null;
            // Safe modify this object still with a trade object
            SafeModifyObject(ch,
                () => { SafeHandleOnCompleteTrade(ch, onCompleteTrade); });
            // Then set the object to null;
            ch.AddMessage(onCompleteTrade);
        }

        private void SafeHandleOnCompleteTrade(Character ch, OnCompleteTrade onCompleteTrade)
        { 
            
            foreach (TradeChar tradeChar in onCompleteTrade.TradeObject.Chars)
            {
                // Remove all items from your inven
                if (tradeChar.CharId == ch.Id)
                {
                    for (int i = 0; i < tradeChar.Items.Length; i++)
                    {
                        if (tradeChar.Items[i] != null)
                        {
                            _inventoryService.RemoveItem(ch, tradeChar.Items[i].Id, false);
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < tradeChar.Items.Length; i++)
                    {
                        if (tradeChar.Items[i] != null)
                        {
                            _inventoryService.AddItem(ch, tradeChar.Items[i], true);
                        }
                    }
                }
            }
        }

        #endregion
    }
}