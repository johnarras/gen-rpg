using Genrpg.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.ServerShared.Accounts
{
    public class DbAccount : IDbId
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string PasswordSalt { get; set; }
        public DateTime CreatedOn { get; set; }

        public int Flags { get; set; }
    }
}
