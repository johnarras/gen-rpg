using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Accounts.Constants
{
    [MessagePackObject]
    public class AccountSearch
    {
        public const string Id = "Id";
        public const string Email = "Email";
        public const string Name = "Name";
    }
}

