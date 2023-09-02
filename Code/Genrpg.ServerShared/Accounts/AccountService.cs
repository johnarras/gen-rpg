
using Microsoft.EntityFrameworkCore;
using System;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Genrpg.Shared.Accounts.Constants;
using Genrpg.Shared.Accounts.Entities;
using Genrpg.ServerShared.Config;
using Genrpg.ServerShared.Utils;
using Genrpg.ServerShared.DataStores;

namespace Genrpg.ServerShared.Accounts
{
    public class AccountService
    {
        private MainDbContext GetContext(ServerConfig config)
        {
            return MainDbContext.Create(config);
        }

        public async Task<Account> LoadBy(ServerConfig config, string type, string id)
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



        public virtual async Task<bool> SaveAccount(ServerConfig config, Account acct)
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


        public string GetPasswordHash(string userSalt, string password)
        {
            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(userSalt))
            {
                return "";
            }

            string txt2 = userSalt + password;
            SHA256 sha = SHA256.Create();
            byte[] arr = System.Text.Encoding.UTF8.GetBytes(txt2);
            byte[] arr2 = sha.ComputeHash(arr);
            return Convert.ToBase64String(arr2);

        }

        public string GeneratePasswordSalt()
        {
            byte[] buff = RandomNumberGenerator.GetBytes(32);
            return Convert.ToBase64String(buff);
        }

    }
}
