using MessagePack;
using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Inventory.PlayerData;
using Genrpg.Shared.Trades.Constants;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Trades.Entities
{
    [MessagePackObject]
    public class TradeChar
    {
        [Key(0)] public string CharId { get; set; }
        [Key(1)] public string CharName { get; set; }
        [Key(2)] public bool Accepted { get; set; }
        [Key(3)] public Item[] Items { get; set; } = new Item[TradeConstants.MaxItems];
        [Key(4)] public long Money { get; set; }
    }

    [MessagePackObject]
    public class FullTradeObject
    {    
        [Key(0)] public List<Character> OrderedCharacters { get; set; } = new List<Character>();
        [Key(1)] public TradeObject TradeObject { get; set; }
        [Key(2)] public string ErrorMessage { get; set; }
        public bool IsOkToUpdate()
        {
            if (OrderedCharacters == null || 
                OrderedCharacters.Count != 2)
            {
                return false;
            }

            if (!string.IsNullOrEmpty(ErrorMessage) || TradeObject == null || TradeObject.State != ETradeStates.InProgress)
            {
                return false;
            }

            return true;         
        }
    }

    [MessagePackObject]
    public class TradeObject
    {

        [Key(0)] public ETradeStates State { get; set; } = ETradeStates.InProgress;

        public TradeObject()
        {
            Chars = new TradeChar[TradeConstants.MaxChars];
            for (int i = 0; i < Chars.Length; i++)
            {
                Chars[i] = new TradeChar();
            }
        }

        [Key(1)] public TradeChar[] Chars { get; set; }

        private object _tradeLock = new object();
        public object GetTradeLock()
        {
            return _tradeLock;
        }
    }
}
