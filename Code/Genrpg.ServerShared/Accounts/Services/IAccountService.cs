using Genrpg.ServerShared.Config;
using Genrpg.Shared.Accounts.PlayerData;
using Genrpg.Shared.Interfaces;
using System.Threading.Tasks;

namespace Genrpg.ServerShared.Accounts.Services
{
    public interface IAccountService : IInitializable
    {
        void AddAccountToProductGraph(Account account, long accountProductId, string referrerId);

        Task<string> GetNextAccountId();
    }
}
