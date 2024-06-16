
using Microsoft.EntityFrameworkCore;
using System;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Genrpg.Shared.Accounts.Constants;
using Genrpg.Shared.Accounts.Entities;
using Genrpg.ServerShared.Config;
using Genrpg.ServerShared.Utils;
using Genrpg.ServerShared.DataStores;
using Genrpg.Shared.Interfaces;
using Genrpg.ServerShared.Accounts;
using Genrpg.Shared.Core.Entities;
using System.Threading;

namespace Genrpg.ServerShared.Accounts.Services
{
    public class AccountService : IAccountService
    {
        private MainDbContext GetContext(IServerConfig config)
        {
            return MainDbContext.Create(config);
        }

        public async Task<Account> LoadBy(IServerConfig config, string type, string id)
        {
            if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(type))
            {
                return null;
            }

            MainDbContext dbContext = GetContext(config);

            DbAccount acct = null;

            if (type == AccountSearch.Id)
            {
                long idVal = 0;
                if (long.TryParse(id, out idVal))
                {
                    acct = await dbContext.Accounts.FirstOrDefaultAsync(x => x.Id == idVal);
                }
            }
            else if (type == AccountSearch.Email)
            {
                acct = await dbContext.Accounts.FirstOrDefaultAsync(x => x.Email == id);
            }
            else if (type == AccountSearch.Name)
            {
                acct = await dbContext.Accounts.FirstOrDefaultAsync(x => x.Name == id);
            }

            if (acct != null)
            {
                return SqlUtils.MapTo<DbAccount, Account>(acct);
            }

            return null;
        }

        public virtual async Task<bool> SaveAccount(IServerConfig config, Account acct)
        {
            if (acct == null)
            {
                return false;
            }


            MainDbContext dbContext = GetContext(config);

            DbAccount dbAccount = await dbContext.Accounts.FirstOrDefaultAsync(x => x.Id == acct.Id);

            if (dbAccount == null)
            {
                dbContext.Accounts.Add(SqlUtils.MapTo<Account, DbAccount>(acct));
                await dbContext.SaveChangesAsync();
            }
            else
            {
                dbAccount.Name = acct.Name;
                dbAccount.Email = acct.Email;
                dbContext.Entry(dbAccount).State = EntityState.Modified;
                await dbContext.SaveChangesAsync();
            }
            return true;
        }

    }
}
