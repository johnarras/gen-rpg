using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Accounts.PlayerData
{
    [MessagePackObject]
    public class AccountIdIncrement :BaseAccountData
    {
        public const string DocId = "Default";

        [Key(0)] public override string Id { get; set; }
        [Key(1)] public long AccountId { get; set; } = 0;
    }
}
