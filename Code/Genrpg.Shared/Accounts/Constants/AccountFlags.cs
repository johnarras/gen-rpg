using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Accounts.Constants
{
    [MessagePackObject]
    public class AccountFlags
    {
        public const int Banned = 1 << 0;
        public const int AllowEmail = 2 << 0;
    }


}

