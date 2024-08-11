using Genrpg.Shared.Accounts.PlayerData;
using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.DataStores.Categories;
using Genrpg.Shared.DataStores.Constants;
using Genrpg.Shared.DataStores.DataGroups;
using Genrpg.Shared.Users.PlayerData;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Assets.Scripts.PlayerSearch
{
    public class PlayerSearchService : IPlayerSearchService
    {

        private IFileDownloadService _downloadService;
        public void AccountSearch(string accountId, Action<PublicAccount> handler, CancellationToken token)
        {
            PlayerSearch(accountId, handler, EDataCategories.Accounts, token);
        }

        public void CharacterSearch(string charId, Action<PublicCharacter> handler, CancellationToken token)
        {
            PlayerSearch(charId, handler, EDataCategories.Players, token);
        }

        public void UserSearch(string userId, Action<PublicUser> handler, CancellationToken token)
        {
            PlayerSearch(userId, handler, EDataCategories.Players,token);
        }

        void PlayerSearch<T>(string Id, Action<T> handler, EDataCategories category,CancellationToken token)
        {
            _downloadService.DownloadTypedFile<T>(Id, handler, category, token);

        }
    }
}
