﻿using Genrpg.ServerShared.Config;
using Genrpg.Shared.Accounts.Entities;
using Genrpg.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.ServerShared.Accounts.Services
{
    public interface IAccountService : IService
    {
        Task<Account> LoadBy(ServerConfig config, string type, string id);
        Task<bool> SaveAccount(ServerConfig config, Account acct);
    }

}
